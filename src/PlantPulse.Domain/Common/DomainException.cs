namespace PlantPulse.Domain.Common
{
    /// <summary>
    /// İş kuralı ihlal edildiğinde fırlatılan hata (API'de 400 olarak döner).
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {

        }
    }
}