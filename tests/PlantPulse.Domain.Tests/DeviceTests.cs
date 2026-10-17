using PlantPulse.Domain.Common;
using PlantPulse.Domain.Devices;

namespace PlantPulse.Domain.Tests;

public class DeviceTests
{
    [Fact]
    public void Code_is_trimmed_and_upper_cased()
    {
        var device = new Device("  press-02 ", "  Pelet Presi 2 ", DeviceType.MotorCurrent);

        Assert.Equal("PRESS-02", device.Code);
        Assert.Equal("Pelet Presi 2", device.Name);
        Assert.True(device.IsActive);
    }

    [Theory]
    [InlineData("", "Pres")]
    [InlineData("PRESS-02", " ")]
    public void Code_and_name_are_required(string code, string name)
        => Assert.Throws<DomainException>(() => new Device(code, name, DeviceType.MotorCurrent));

    [Fact]
    public void Deactivate_marks_device_inactive()
    {
        var device = new Device("PRESS-02", "Pelet Presi 2", DeviceType.MotorCurrent);

        device.Deactivate();

        Assert.False(device.IsActive);
    }
}
