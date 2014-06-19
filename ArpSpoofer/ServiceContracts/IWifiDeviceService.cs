using ArpSpoofer.DTO;
using ArpSpoofer.Enum;

namespace ArpSpoofer.ServiceContracts
{
    public interface IWifiDeviceService
    {
        DeviceWithDescription DeviceWithDescription { get; }
    }
}
