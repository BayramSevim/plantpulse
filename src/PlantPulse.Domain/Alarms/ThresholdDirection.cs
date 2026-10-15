namespace PlantPulse.Domain.Alarms
{
    /// <summary>
    /// Eşiğin yönü: sorun, değerin sınırın üstüne mi altına mı çıkması?
    /// </summary>
    public enum ThresholdDirection
    {
        /// <summary>
        /// Üstü: değer sınırı geçerse sorun (sıcaklık, motor akımı).
        /// </summary>
        Above,
        /// <summary>
        /// Altı: değer sınırın altına düşerse sorun (silo seviyesi).
        /// </summary>
        Below
    }
}