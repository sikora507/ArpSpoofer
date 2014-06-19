using ArpSpoofer.Enum;
using ArpSpoofer.ServiceContracts;
using Microsoft.Win32;
using System;
namespace ArpSpoofer.Services
{
    public class RegistryService : IRegistryService
    {
        const string TcpIpParametersRegistryString = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
        const string NetworkCardsRegistryString = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkCards";
        public WinRoutingStatus GetWinRoutingStatus()
        {
            var regKey = Registry.LocalMachine.OpenSubKey(TcpIpParametersRegistryString);
            var value = (int)regKey.GetValue("IPEnableRouter");
            switch (value)
            {
                case 1:
                    return WinRoutingStatus.Enabled;
                case 0:
                    return WinRoutingStatus.Disabled;
                default:
                    return WinRoutingStatus.Unknown;
            }
        }

        public string GetWifiRegistryParameter(WifiRegistryInformation parameter)
        {
            var regKey = Registry.LocalMachine.OpenSubKey(NetworkCardsRegistryString);
            var regDevices = regKey.GetSubKeyNames();
            foreach (var subkey in regDevices)
            {
                var deviceName = (string)regKey.OpenSubKey(subkey).GetValue("Description", "");
                if (deviceName.ToLower().Contains("wifi") || deviceName.ToLower().Contains("wireless"))
                {
                    switch (parameter)
                    {
                        case WifiRegistryInformation.Guid:
                            return (string)regKey.OpenSubKey(subkey).GetValue("ServiceName", "");
                        case WifiRegistryInformation.Name:
                            return deviceName;
                    }
                }
            }

            throw new InvalidOperationException("WIFI card in registry not found.");
        }

        /// <summary>
        /// Enables or disables Windows IP routing by setting proper value in registry.
        /// </summary>
        /// <returns>WinRoutingStatus enum</returns>
        public WinRoutingStatus ToggleWinRouting()
        {
            var regKey = Registry.LocalMachine.OpenSubKey(TcpIpParametersRegistryString, true);

            var status = GetWinRoutingStatus();
            if (status == WinRoutingStatus.Enabled)
            {
                // disable routing
                regKey.SetValue("IPEnableRouter", 0, RegistryValueKind.DWord);
            }
            else if (status == WinRoutingStatus.Disabled)
            {
                // enable routing
                regKey.SetValue("IPEnableRouter", 1, RegistryValueKind.DWord);
            }
            // return new status
            return GetWinRoutingStatus();
        }
    }
}
