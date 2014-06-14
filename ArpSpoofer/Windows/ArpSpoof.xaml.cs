using ArpSpoofer.DTO;
using ArpSpoofer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArpSpoofer.Windows
{
    /// <summary>
    /// Interaction logic for ArpSpoof.xaml
    /// </summary>
    public partial class ArpSpoof : Window
    {
        ARPSpoofingService _spoofingService;
        ObservableCollection<IpMacPair> _ipMacPairs = new ObservableCollection<IpMacPair>();
        public ObservableCollection<IpMacPair> IpMacPairs
        {
            get { return _ipMacPairs; }
        }
        public ArpSpoof(ARPSpoofingService spoofingService)
        {
            InitializeComponent();
            _spoofingService = spoofingService;
            tbGetawayIp.Text = _spoofingService.Device.GatewayString;
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
