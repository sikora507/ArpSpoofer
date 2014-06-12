using ArpSpoofer.Services;
using System;
using System.Collections.Generic;
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
        public ArpSpoof(ARPSpoofingService spoofingService)
        {
            InitializeComponent();
            _spoofingService = spoofingService;
            tbGetawayIp.Text = _spoofingService.Device.GatewayString;
        }

        private void btnStartARP_Click(object sender, RoutedEventArgs e)
        {
            _spoofingService.StartPoisoning(tbGetawayIp.Text);
        }

        private void btnStopArp_Click(object sender, RoutedEventArgs e)
        {
            _spoofingService.StopPoisoning();
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (_spoofingService.IsScanning == false)
            {
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
