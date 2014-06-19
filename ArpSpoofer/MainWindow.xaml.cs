using ArpSpoofer.DTO;
using ArpSpoofer.ServiceContracts;
using ArpSpoofer.Services;
using ArpSpoofer.WindowContracts;
using ArpSpoofer.Windows;
using Ninject;
using System.Windows;

namespace ArpSpoofer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        //windows
        readonly ICapturePackets _wndCapturePackets;
        readonly IArpSpoof _wndArpSpoof;
        //services
        readonly IRegistryService _registryService;
        readonly IWifiDeviceService _wifiDeviceService;
        public MainWindow(
            IRegistryService registryService, IWifiDeviceService wifiDeviceService,
            ICapturePackets capturePackets, IArpSpoof arpSpoof)
        {
            InitializeComponent();
            _registryService = registryService;
            _wifiDeviceService = wifiDeviceService;
            _wndArpSpoof = arpSpoof;
            _wndCapturePackets = capturePackets;

            _wndArpSpoof.Closing += wnd_Closing;
            _wndCapturePackets.Closing += wnd_Closing;

            tbDriver.Text = _wifiDeviceService.DeviceWithDescription.DriverName;
            tbName.Text = _wifiDeviceService.DeviceWithDescription.Name;
            tbIp.Text = _wifiDeviceService.DeviceWithDescription.IpV4;
            tbGuid.Text = _wifiDeviceService.DeviceWithDescription.Guid;
            tbMac.Text = _wifiDeviceService.DeviceWithDescription.DeviceMacString;
            tbGateway.Text = _wifiDeviceService.DeviceWithDescription.GatewayIpString;
            lblWinRouting.Content = _registryService.GetWinRoutingStatus();
        }

        private void btnCapturePackets_Click(object sender, RoutedEventArgs e)
        {
            _wndCapturePackets.Show();
        }

        private void btnArpSpoof_Click(object sender, RoutedEventArgs e)
        {
            _wndArpSpoof.Show();
        }

        void wnd_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            ((Window)sender).Visibility = Visibility.Hidden;
        }

        private void btnToggleWinIpRouting_Click(object sender, RoutedEventArgs e)
        {
            lblWinRouting.Content = _registryService.ToggleWinRouting();
        }
    }
}
