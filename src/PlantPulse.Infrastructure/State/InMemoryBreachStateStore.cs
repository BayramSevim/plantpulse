using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Application.Abstractions;

namespace PlantPulse.Infrastructure.State
{
    public class InMemoryBreachStateStore : IBreachStateStore
    {
        private readonly ConcurrentDictionary<Guid, DateTime> _starts = new();

        public DateTime? Get(Guid ruleId)
        {
            if (_starts.TryGetValue(ruleId, out var start))
                return start;
            return null;
        }

        public void Set(Guid ruleId, DateTime? breachStartedAt)
        {
            if (breachStartedAt is null)
            {
                _starts.TryRemove(ruleId, out _);
                return;
            }

            _starts[ruleId] = breachStartedAt.Value;
        }
    }
}