using ArpSpoofer.DTO;
using ArpSpoofer.Enum;
using ArpSpoofer.ServiceContracts;
using Ninject;
using PcapDotNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ArpSpoofer.Services
{
    public class WifiDeviceService : IWifiDeviceService
    {
        DeviceWithDescription _deviceWithDescription;
        IRegistryService _registryService;
        public DeviceWithDescription DeviceWithDescription {
            get {
                return _deviceWithDescription;
            }
        }
        public WifiDeviceService(IRegistryService registryService)
        {
            _registryService = registryService;

            _deviceWithDescription = GetDeviceWithDescription();
        }

        private DeviceWithDescription GetDeviceWithDescription()
        {
            var result = new DeviceWithDescription();

            //first get GUID of wifi device from registry
            result.Guid = _registryService.GetWifiRegistryParameter(WifiRegistryInformation.Guid);
            result.Name = _registryService.GetWifiRegistryParameter(WifiRegistryInformation.Name);

            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            if (allDevices.Count == 0)
            {
                throw new InvalidOperationException("No interfaces found! Make sure WinPcap is installed.");
            }

            var wifiDevice = allDevices.FirstOrDefault(d => d.Name.Contains(result.Guid));
            var addresses = wifiDevice.Addresses;
            if (wifiDevice == null)
            {
                throw new InvalidOperationException("Could not match wifi device from registry with device from WinPcap");
            }

            string address = wifiDevice.Addresses.Single(x => x.Address.Family.ToString() == "Internet").Address.ToString();
            var ipv4 = address.Substring(address.IndexOf("Internet ") + 9);

            var nic = NetworkInterface.GetAllNetworkInterfaces().Single(x => x.Id.Contains(result.Guid));
            var gatewayBytes = nic.GetIPProperties().GatewayAddresses.First().Address.GetAddressBytes();
            var mac = nic.GetPhysicalAddress();
            result.DeviceMacByte = mac.GetAddressBytes();
            result.GatewayIpByte = gatewayBytes;
            result.IpV4 = ipv4;
            result.DriverName = wifiDevice.Description;
            result.Device = wifiDevice;
            return result;
        }
    }
}
