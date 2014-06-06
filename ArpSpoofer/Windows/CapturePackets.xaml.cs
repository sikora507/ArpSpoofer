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
    /// Interaction logic for CapturePackets.xaml
    /// </summary>
    public partial class CapturePackets : Window
    {
        CapturingService _capturingService;
        public CapturePackets(CapturingService capturingService)
        {
            InitializeComponent();
            _capturingService = capturingService;
            _capturingService.PacketCaptured += _capturingService_PacketCaptured;
            _capturingService.StatusChanged += _capturingService_StatusChanged;
        }

        void _capturingService_PacketCaptured(object sender, PacketCapturedEventArgs e)
        {
            var newMessage = string.Format("Packet length: {0},\tTime: {1}\n",e.PacketLength,e.TimestampString);
            Dispatcher.Invoke(() => {
                tbTraffic.Text += newMessage;
                tbTraffic.ScrollToEnd();
            });
        }

        void _capturingService_StatusChanged(object sender, string e)
        {
            sbMessage.Text = e;
        }

        private void btnStartCapture_Click(object sender, RoutedEventArgs e)
        {
            _capturingService.StartCapturing();
        }

        private void btnStopCapture_Click(object sender, RoutedEventArgs e)
        {
            _capturingService.StopCapturing();
        }
    }
}
