# PlantPulse

Industrial telemetry and alarm platform built with .NET 8.

Factory devices (pellet presses, dryers, silos...) stream readings to Kafka. A worker stores them in batches, keeps the latest value of every device in Redis and runs a rule engine that opens an alarm when a reading stays beyond its threshold for a configured duration, and closes it on recovery with hysteresis. Alarm events leave the system through a transactional outbox with retries and exponential backoff, and reach a live dashboard through SignalR.
 

## Architecture

```
 Simulator ──► Kafka "plant.readings" (key = device code)
                    │
                    ├──► Worker (consumer group: plantpulse-worker)
                    │       ├─ batch: 200 messages or 1 s
                    │       ├─ PostgreSQL: readings (bulk insert)
                    │       ├─ Redis: latest value per device
                    │       └─ Rule engine ─► Alarm aggregate ─► domain events ─► outbox (same transaction)
                    │
                    │    OutboxProcessor (in Worker)
                    │       ├─ Kafka "plant.alarm-events"
                    │       └─ notification (e-mail), retry 2 → 4 → 8 → 16 s
                    │
                    └──► Api (consumer group: plantpulse-api-live)
                            ├─ REST: devices, readings, alarms, acknowledge
                            └─ SignalR /hubs/plant ─► live dashboard
```

| Project | Responsibility |
|---|---|
| `PlantPulse.Domain` | Devices, alarm rules, `Threshold` value object (hysteresis), `Alarm` aggregate (open / clear / acknowledge), domain events, rule engine |
| `PlantPulse.Application` | `ReadingProcessor` (reading → rules → alarms), `AcknowledgeAlarmHandler`, ports (repositories, stores) |
| `PlantPulse.Infrastructure` | EF Core + PostgreSQL, outbox, Redis, Kafka producer, notification sender |
| `PlantPulse.Worker` | Batch Kafka consumer + outbox processor |
| `PlantPulse.Api` | REST endpoints, SignalR hub, live dashboard (`wwwroot`) |
| `PlantPulse.Simulator` | Stands in for PLCs / OPC UA: publishes readings for 10 devices every second, with random faults |

## Design decisions

**Rule engine as a pure function.** `AlarmEvaluator` takes the rule, the reading, its timestamp, when the current breach started and whether an alarm is already open, and returns a decision. It works like a PLC on-delay (TON) timer: the threshold must be exceeded *continuously* for the configured duration; a single normal reading resets the timer. No database, no clock inside, so every edge case is a one-line unit test.

**Hysteresis.** `Threshold` is a value object (`sealed record`). An alarm opens when the value crosses the limit but closes only after it moves back by the hysteresis band (e.g. limit 95 °C, hysteresis 3 → closes at 92 °C). This prevents flapping alarms when the value hovers around the limit.

**Domain events raised by the aggregate.** `Alarm` raises `AlarmRaised`, `AlarmCleared` and `AlarmAcknowledged` itself. `SaveChangesAsync` collects them and writes them to `outbox_messages` in the same transaction as the alarm, so an alarm can never be stored without its event (or the other way round). No handler has to remember to publish.

**Outbox with retries and exponential backoff.** `OutboxProcessor` picks pending messages with `FOR UPDATE SKIP LOCKED` (safe with several worker instances), publishes them to Kafka and sends notifications. A failed message is retried after 2, 4, 8 and 16 seconds and given up after 5 attempts, keeping the last error for inspection. Set `Notifications:FailureRate` to watch retries happen.

**Ordering in Kafka.** Readings are keyed by device code, so every reading of a device lands on the same partition and is processed in order. This matters because the rule engine reasons about time. Verified by an integration test against a real broker.

**Batch consumption and commits.** The worker collects up to 200 messages or 1 second, writes readings in one round trip, runs the rule engine in order, then commits. Auto-commit is disabled and offsets are committed for *all* assigned partitions only after processing (at-least-once). A malformed message is logged and skipped instead of crashing the consumer forever (poison message).

**Database as the last line of defence.** A filtered unique index (`RuleId WHERE Status = 'Active'`) guarantees at most one active alarm per rule even if two workers race. Rules reference devices with `ON DELETE RESTRICT`: deactivating is the supported path, rules are never deleted silently.

**Redis for the latest values.** The dashboard asks "what is the current value of every device?" on every load. Answering that from a table with millions of rows is wasteful; one Redis hash (`HGETALL`) answers it in microseconds and is shared between the Worker (writer) and the Api (reader). Redis is treated as a cache: if it is down, alarms keep working.

**Breach timers in memory.** "When did the current breach start?" changes on every reading, so it lives in a singleton `ConcurrentDictionary` instead of the database. Trade-off: after a restart running timers start over, so an alarm can be delayed by at most one rule duration. Open alarms themselves are in PostgreSQL and survive restarts.

## Running locally

Requirements: .NET SDK (see `global.json`), Docker.

```bash
docker compose up -d                                 # PostgreSQL, Kafka, Kafka UI, Redis

dotnet run --project src/PlantPulse.Api              # migrations + seed, dashboard on http://localhost:5027
dotnet run --project src/PlantPulse.Worker
dotnet run --project src/PlantPulse.Simulator
```

- Dashboard: http://localhost:5027
- Swagger: http://localhost:5027/swagger
- Kafka UI: http://localhost:8080

Faults are random (about one every 20 seconds across all devices); an alarm opens when a fault lasts longer than the rule duration (30 s).

## API

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/devices` | Devices with latest value, rule and active alarm severity |
| GET | `/api/devices/{code}/readings?minutes=5` | Recent readings for charts |
| GET | `/api/alarms?status=active\|cleared&limit=50` | Alarms, newest first |
| POST | `/api/alarms/{id}/acknowledge` | Operator acknowledgement `{ "by": "name" }` (400 if already acknowledged) |

SignalR hub `/hubs/plant` pushes `reading` and `alarm` events.

## Tests

```bash
dotnet test
```

- **Unit tests** (`PlantPulse.Domain.Tests`): threshold and hysteresis boundaries, rule engine timer behaviour, alarm lifecycle and invariants.
- **Integration tests** (`PlantPulse.IntegrationTests`, Testcontainers): full reading → rule engine → alarm → outbox flow and the filtered unique index on real PostgreSQL 17, Redis latest-value store, Kafka key-based ordering across partitions.

GitHub Actions runs both on every push.

## Possible next steps

- OPC UA bridge replacing the simulator (same Kafka message contract, PLC `SourceTimestamp`, quality codes)
- Breach timers in Redis to survive restarts
- Dead-letter topic for malformed messages
- Authentication for operators
