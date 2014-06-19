using ArpSpoofer.ServiceContracts;
using ArpSpoofer.Services;
using ArpSpoofer.WindowContracts;
using PcapDotNet.Packets;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArpSpoofer.Windows
{
    /// <summary>
    /// Interaction logic for CapturePackets.xaml
    /// </summary>
    public partial class CapturePackets : Window, ICapturePackets
    {
        ICapturingService _capturingService;
        uint _packetsCount;
        const int maxLines = 1000;
        uint _nextSequenceNumber = 0;
        public CapturePackets(ICapturingService capturingService)
        {
            InitializeComponent();
            _capturingService = capturingService;
            _capturingService.PacketCaptured += _capturingService_PacketCaptured;
            _capturingService.StatusChanged += _capturingService_StatusChanged;
            _packetsCount = 0;
        }

        void _capturingService_PacketCaptured(object sender, Packet packet)
        {
            _packetsCount++;
            var ip = packet.Ethernet.IpV4;
            var tcp = ip.Tcp;

            var httpDatagram = packet.Ethernet.IpV4.Tcp.Http;
            string httpHeader = "";
            string httpBody = "";

            if (tcp.IsValid && tcp.PayloadLength > 0)
            {
                // pull the payload 
                Datagram dg = tcp.Payload;
                MemoryStream ms = dg.ToMemoryStream();
                StreamReader sr = new StreamReader(ms);
                string content = sr.ReadToEnd();
                if (content.IndexOf("HTTP") != -1)
                {
                    int endHeaderIndex = content.IndexOf("\r\n\r\n");
                    if (endHeaderIndex != -1)
                    {
                        httpHeader = content.Substring(0, endHeaderIndex);
                    }
                }

                string newMessage = string.Format("Packet length: {0},\tTime: {1}\n{2}:{3} -> {4}:{5}\n",
                    packet.Length.ToString("D8"), packet.Timestamp.ToShortTimeString(), ip.Source, tcp.SourcePort, ip.Destination, tcp.DestinationPort);

                string headerMessage = "";
                if (!string.IsNullOrEmpty(httpHeader))
                {
                    headerMessage += string.Format("Header: {0}\n", httpHeader);
                }

                // parse out body
                // but make sure it isn't just composed of only the CRLF CRLF breaks
                if (httpDatagram.Body != null && !string.IsNullOrEmpty(httpHeader))
                {
                    httpBody = httpDatagram.Body.Decode(Encoding.UTF8);
                }
                else if (tcp.SequenceNumber == _nextSequenceNumber)
                {
                    httpBody = tcp.Payload.Decode(Encoding.UTF8);
                }
                string bodyMessage = "";
                if (!string.IsNullOrEmpty(httpBody))
                {
                    bodyMessage += string.Format("Body: {0}\n", httpBody);
                }
                if (!string.IsNullOrEmpty(headerMessage) || !string.IsNullOrEmpty(bodyMessage))
                {
                    Dispatcher.Invoke(() =>
                    {
                        var textLines = tbTraffic.Text.Split('\n').ToList();
                        var linesCount = textLines.Count;
                        if (linesCount > maxLines)
                        {
                            textLines = textLines.Skip(linesCount - maxLines).Take(maxLines).ToList();
                        }
                        newMessage += headerMessage + bodyMessage;
                        textLines.Add(newMessage);

                        lblPacketsCount.Content = _packetsCount.ToString();
                        tbTraffic.Text = string.Join("\n", textLines);
                        tbTraffic.ScrollToEnd();
                    });
                }
            }
            if (tcp.Http.IsRequest) { 
                _nextSequenceNumber = tcp.NextSequenceNumber;
            }
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
