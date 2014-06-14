using ArpSpoofer.DTO;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.IpV4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArpSpoofer.Services
{
    public class ARPSpoofingService
    {
        public DeviceWithDescription Device;
        CancellationTokenSource cancelTokenSource;
        Dictionary<string, string> _ipMacPairs;
        public event EventHandler<double> ScanTick;
        public event EventHandler<IpMacPair> NewIpMacFound;
        public bool IsScanning { get; set; }
        public ARPSpoofingService(DeviceWithDescription device)
        {
            Device = device;
            cancelTokenSource = new CancellationTokenSource();
            IsScanning = false;
            _ipMacPairs = new Dictionary<string, string>();
        }

        internal void StartPoisoning(string getawayIp)
        {

        }

        internal void StopPoisoning()
        {

        }

        internal void Scan()
        {
            if (IsScanning == false)
            {
                IsScanning = true;
                cancelTokenSource = new CancellationTokenSource();
                ListenForReplys();
                ArpAddresses();
            }
        }

        internal void StopScan()
        {
            if (IsScanning == true) { 
                cancelTokenSource.Cancel();
                IsScanning = false;
            }
        }

        private async void ArpAddresses()
        {
            _ipMacPairs.Clear();
            var communicator = Device.Device.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000);

            MacAddress source = new MacAddress(Device.MacString.Replace('-', ':'));
            MacAddress destination = new MacAddress("ff:ff:ff:ff:ff:ff");
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = source,
                Destination = destination,
                EtherType = EthernetType.None
            };

            var ipVals = Device.IpV4.Split('.').Select(x => byte.Parse(x)).ToArray();

            ArpLayer arpLayer = new ArpLayer
            {
                ProtocolType = EthernetType.IpV4,
                Operation = ArpOperation.Request,
                SenderHardwareAddress = Array.AsReadOnly(Device.MacByte),
                SenderProtocolAddress = Array.AsReadOnly(ipVals),
                TargetHardwareAddress = Array.AsReadOnly(new byte[] { 0, 0, 0, 0, 0, 0 }),
                //TargetProtocolAddress = new byte[] { 192, 168, 1, 1 }, // 11.22.33.44.
            };

            //IpV4Layer ipv4Layer = new IpV4Layer
            //{
            //    Source = new IpV4Address(Device.IpV4),
            //    Ttl = 128
            //};

            //IcmpEchoLayer icmpLayer = new IcmpEchoLayer();

            //PacketBuilder builder = new PacketBuilder(ethernetLayer, ipv4Layer, icmpLayer);
            PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);
            await Task.Factory.StartNew(() =>
            {
                for (var i = 1; ; ++i)
                {
                    if (i >= 256)
                    {
                        i = 1;
                    }
                    if (i == ipVals[3]) continue;
                    // Set IPv4 parameters
                    arpLayer.TargetProtocolAddress = Array.AsReadOnly(new byte[] { 192, 168, 1, (byte)i });
                    //ipv4Layer.Identification = (ushort)i;

                    //// Set ICMP parameters
                    //icmpLayer.SequenceNumber = (ushort)i;
                    //icmpLayer.Identifier = (ushort)i;

                    // Build the packet
                    Packet packet = builder.Build(DateTime.Now);
                    Thread.Sleep(200);
                    // Send down the packet
                    communicator.SendPacket(packet);
                    if (ScanTick != null)
                    {
                        ScanTick(this, (double)i / 255.0);
                    }
                }
            });
            communicator.Dispose();
        }

        private void ListenForReplys()
        {
            var communicator = Device.Device.Open(200, PacketDeviceOpenAttributes.Promiscuous, 1000);

            using (BerkeleyPacketFilter filter = communicator.CreateFilter("arp"))
            {
                // Set the filter
                communicator.SetFilter(filter);
            }

            Task.Factory.StartNew(async () =>
            {
                Packet packet;
                PacketCommunicatorReceiveResult receiveResult;
                do
                {
                    receiveResult = await Task.Factory.StartNew<PacketCommunicatorReceiveResult>(
                        () =>
                        {
                            PacketCommunicatorReceiveResult result = communicator.ReceivePacket(out packet);
                            switch (result)
                            {
                                case PacketCommunicatorReceiveResult.Timeout:
                                    // Timeout elapsed
                                    break;
                                case PacketCommunicatorReceiveResult.Ok:
                                    if (packet.Ethernet.Arp.Operation == ArpOperation.Reply)
                                    {
                                        //_receivedPackets.Add(packet);
                                        string mac = BitConverter.ToString(packet.Ethernet.Arp.SenderHardwareAddress.ToArray());
                                        string ip = packet.Ethernet.Arp.SenderProtocolIpV4Address.ToString();
                                        if(!_ipMacPairs.ContainsKey(mac)){
                                            _ipMacPairs.Add(mac, ip);
                                            if (NewIpMacFound != null)
                                            {
                                                NewIpMacFound(this, new IpMacPair { Ip = ip, Mac = mac });
                                            }
                                        }
                                    }
                                    break;
                                case PacketCommunicatorReceiveResult.Eof:
                                    break;
                                default:
                                    throw new InvalidOperationException("The result " + result + " shoudl never be reached here");
                            }
                            return result;
                        }
                    );
                    if (cancelTokenSource.Token.IsCancellationRequested)
                    {
                        communicator.Dispose();
                        cancelTokenSource.Token.ThrowIfCancellationRequested();
                    }
                } while (receiveResult != PacketCommunicatorReceiveResult.Eof);
            }, cancelTokenSource.Token);
        }
    }
}
