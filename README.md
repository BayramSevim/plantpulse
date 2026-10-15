# PlantPulse

Industrial telemetry and alarm platform built with .NET 8.

> 🚧 Work in progress — currently building the ingestion pipeline.

## Planned architecture

Simulator → Kafka → Worker (batch consume) → PostgreSQL + Redis → Rule engine → SignalR dashboard
Alarm notifications via transactional outbox with retry and exponential backoff.

## Stack

.NET 8, Apache Kafka (KRaft), PostgreSQL, Redis, SignalR, xUnit, Testcontainers, Docker Compose, GitHub Actions

## Run

docker compose up -d
