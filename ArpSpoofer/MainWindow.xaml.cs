using ArpSpoofer.DTO;
using ArpSpoofer.Services;
using ArpSpoofer.Windows;
using Microsoft.Win32;
using PcapDotNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArpSpoofer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CapturePackets _wndCapturePackets;
        ArpSpoof _wndArpSpoof;
        DeviceWithDescription _deviceWithDescription;
        public MainWindow()
        {
            InitializeComponent();
            _deviceWithDescription = GetWifiDevice();
            tbDriver.Text = _deviceWithDescription.Driver;
            tbName.Text = _deviceWithDescription.Name;
            tbIp.Text = _deviceWithDescription.IpV4;
            tbGuid.Text = _deviceWithDescription.Guid;
            tbMac.Text = _deviceWithDescription.MacString;
            tbGateway.Text = _deviceWithDescription.GatewayString;
        }

        private DeviceWithDescription GetWifiDevice()
        {
            var result = new DeviceWithDescription();

            //first get GUID of wifi device from registry
            const string rootKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkCards";
            var regKey = Registry.LocalMachine.OpenSubKey(rootKey);
            var regDevices = regKey.GetSubKeyNames();
            var wifiGuid = "";
            foreach(var subkey in regDevices){
                var deviceName = (string)regKey.OpenSubKey(subkey).GetValue("Description", "");
                if (deviceName.ToLower().Contains("wifi") || deviceName.ToLower().Contains("wireless"))
                {
                    wifiGuid = (string)regKey.OpenSubKey(subkey).GetValue("ServiceName", "");
                    result.Guid = wifiGuid;
                    result.Name = deviceName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(wifiGuid))
            {
                return null;
            }

            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            
            if (allDevices.Count == 0)
            {
                MessageBox.Show("No interfaces found! Make sure WinPcap is installed.");
                return null;
            }

            var wifiDevice = allDevices.FirstOrDefault(d => d.Name.Contains(wifiGuid));
            var addresses = wifiDevice.Addresses;
            if (wifiDevice == null)
            {
                return null;
            }

            string address = wifiDevice.Addresses.Single(x => x.Address.Family.ToString() == "Internet").Address.ToString();
            var ipv4 = address.Substring(address.IndexOf("Internet ")+9);

            var nic = NetworkInterface.GetAllNetworkInterfaces().Single(x => x.Id.Contains(result.Guid));
            var gatewayBytes = nic.GetIPProperties().GatewayAddresses.First().Address.GetAddressBytes();
            var mac = nic.GetPhysicalAddress();
            result.MacByte = mac.GetAddressBytes();
            result.GatewayByte = gatewayBytes;
            result.IpV4 = ipv4;
            result.Driver = wifiDevice.Description;
            result.Device = wifiDevice;
            return result;
        }

        private void btnCapturePackets_Click(object sender, RoutedEventArgs e)
        {
            if(_wndCapturePackets == null){
                var capturingService = new CapturingService(_deviceWithDescription.Device);
                _wndCapturePackets = new CapturePackets(capturingService);
                _wndCapturePackets.Closing += wnd_Closing;
                _wndCapturePackets.Show();
            }
            else
            {
                if (_wndCapturePackets.IsVisible)
                {
                    return;
                }
                else
                {
                    _wndCapturePackets.Show();
                }
            }
        }

        private void btnArpSpoof_Click(object sender, RoutedEventArgs e)
        {
            if (_wndArpSpoof == null)
            {
                var arpSpoofingService = new ARPSpoofingService(_deviceWithDescription);
                _wndArpSpoof = new ArpSpoof(arpSpoofingService);
                _wndArpSpoof.Closing += wnd_Closing;
                _wndArpSpoof.Show();
            }
            else { 
                if (_wndArpSpoof.IsVisible)
                {
                    return;
                }
                else
                {
                    _wndArpSpoof.Show();
                }
            }
        }

        void wnd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            ((Window)sender).Visibility = Visibility.Hidden;
        }
    }
}
