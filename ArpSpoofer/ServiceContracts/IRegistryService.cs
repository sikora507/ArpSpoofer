using ArpSpoofer.Enum;

namespace ArpSpoofer.ServiceContracts
{
    public interface IRegistryService
    {
        WinRoutingStatus GetWinRoutingStatus();
        WinRoutingStatus ToggleWinRouting();
        string GetWifiRegistryParameter(WifiRegistryInformation parameter);
    }
}
