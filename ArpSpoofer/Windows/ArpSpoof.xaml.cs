using ArpSpoofer.DTO;
using ArpSpoofer.ServiceContracts;
using ArpSpoofer.WindowContracts;
using System.Collections.ObjectModel;
using System.Windows;

namespace ArpSpoofer.Windows
{
    public partial class ArpSpoof : Window, IArpSpoof
    {
        IARPSpoofingService _spoofingService;
        IWifiDeviceService _deviceService;
        ObservableCollection<IpMacPair> _ipMacPairs = new ObservableCollection<IpMacPair>();
        public ObservableCollection<IpMacPair> IpMacPairs
        {
            get { return _ipMacPairs; }
        }
        public ArpSpoof(IARPSpoofingService spoofingService, IWifiDeviceService deviceService)
        {
            InitializeComponent();
            _deviceService = deviceService;
            _spoofingService = spoofingService;
            tbGetawayIp.Text = _deviceService.DeviceWithDescription.GatewayIpString;
            _spoofingService.ScanTick += _spoofingService_ScanTick;
            _spoofingService.NewIpMacFound += _spoofingService_NewIpMacFound;
        }

        void _spoofingService_NewIpMacFound(object sender, IpMacPair e)
        {
            Dispatcher.Invoke(() =>
            {
                _ipMacPairs.Add(e);
            });
        }

        void _spoofingService_ScanTick(object sender, double e)
        {
            Dispatcher.Invoke(() =>
            {
                pbScan.Value = e;
            });
        }

        private void btnStartARP_Click(object sender, RoutedEventArgs e)
        {
            _spoofingService.StartPoisoning();
        }

        private void btnStopArp_Click(object sender, RoutedEventArgs e)
        {
            _spoofingService.StopPoisoning();
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (_spoofingService.IsScanning == false)
            {
                _ipMacPairs.Clear();
                _spoofingService.Scan();
                btnScan.Content = "STOP scanning";
            }
            else
            {
                _spoofingService.StopScan();
                btnScan.Content = "SCAN";
            }
        }
    }
}
