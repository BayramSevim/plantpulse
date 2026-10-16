using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Application.Abstractions;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Alarms.RuleEngine;

namespace PlantPulse.Application.Readings
{
    public class ReadingProcessor
    {
        private readonly IAlarmRuleRepository _alarmRuleRepository;
        private readonly IAlarmRepository _alarmRepository;
        private readonly IBreachStateStore _breachStateStore;
        private readonly IUnitOfWork _unitOfWork;

        public ReadingProcessor(IAlarmRuleRepository alarmRuleRepository, IAlarmRepository alarmRepository, IBreachStateStore breachStateStore, IUnitOfWork unitOfWork)
        {
            _alarmRuleRepository = alarmRuleRepository;
            _alarmRepository = alarmRepository;
            _breachStateStore = breachStateStore;
            _unitOfWork = unitOfWork;
        }

        public async Task ProcessAsync(string deviceCode, double value, DateTime at, CancellationToken ct)
        {
            if (!double.IsFinite(value))
                return;

            var rules = await _alarmRuleRepository.GetEnabledByDeviceCodeAsync(deviceCode, ct);

            foreach (var rule in rules)
            {
                var activeAlarm = await _alarmRepository.GetActiveByRuleIdAsync(rule.Id, ct);
                var result = AlarmEvaluator.Evaluate(rule, value, at, _breachStateStore.Get(rule.Id), activeAlarm is not null);
                _breachStateStore.Set(rule.Id, result.BreachStartedAt);

                switch (result.Outcome)
                {
                    case EvaluationOutcome.OpenAlarm:
                        Alarm alarm = new Alarm(rule, value, at);
                        await _alarmRepository.AddAsync(alarm, ct);
                        break;
                    case EvaluationOutcome.ClearAlarm:
                        activeAlarm!.Clear(value, at);
                        break;
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}