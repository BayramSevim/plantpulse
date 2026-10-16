namespace PlantPulse.Application.Abstractions
{
    public interface IBreachStateStore
    {
        DateTime? Get(Guid ruleId);
        void Set(Guid ruleId, DateTime? breachStartedAt);
    }
}