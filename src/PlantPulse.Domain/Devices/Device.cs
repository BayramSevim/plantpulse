using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Devices
{
    public class Device
    {
        public Guid Id { get; private set; }
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public DeviceType Type { get; private set; }
        public bool IsActive { get; private set; }

        private Device()
        {

        }

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

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}