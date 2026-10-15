namespace PlantPulse.Domain.Devices
{
    /// <summary>
    /// Sensörün ölçtüğü büyüklük.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Sıcaklık (°C).
        /// </summary>
        Temperature,
        /// <summary>
        /// Motor akımı (A).
        /// </summary>
        MotorCurrent,
        /// <summary>
        /// Silo doluluk seviyesi (%).
        /// </summary>
        SiloLevel
    }
}