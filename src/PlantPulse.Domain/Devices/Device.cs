using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Devices
{
    /// <summary>
    /// Sahadaki fiziksel makine/sensör (örn. Pelet Presi 2, Silo C).
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Cihazın benzersiz kimliği.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Cihaz kodu (örn. PRESS-02). Boşlukları kırpılıp büyük harfe çevrilerek saklanır.
        /// </summary>
        public string Code { get; private set; } = null!;
        /// <summary>
        /// Cihazın ekranda görünen adı.
        /// </summary>
        public string Name { get; private set; } = null!;
        /// <summary>
        /// Cihazın ne ölçtüğü (sıcaklık, motor akımı, silo seviyesi).
        /// </summary>
        public DeviceType Type { get; private set; }
        /// <summary>
        /// Cihaz aktif mi? Devre dışı cihaz silinmez, geçmiş alarmları korunur.
        /// </summary>
        public bool IsActive { get; private set; }

        private Device()
        {

        }

        /// <summary>
        /// Yeni cihaz oluşturur. Kod ve ad boş olamaz.
        /// </summary>
        public Device(string code, string name, DeviceType type)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Code is required");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Name is required");

            Id = Guid.NewGuid();
            Code = code.Trim().ToUpperInvariant();
            Name = name.Trim();
            Type = type;
            IsActive = true;
        }

        /// <summary>
        /// Cihazı devre dışı bırakır (sökülen/kullanılmayan cihaz).
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}