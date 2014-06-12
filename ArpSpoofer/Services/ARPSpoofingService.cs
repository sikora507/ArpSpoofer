using ArpSpoofer.DTO;
using PcapDotNet.Core;
using PcapDotNet.Packets;
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
        List<Packet> _receivedPackets = new List<Packet>();
        public bool IsScanning { get; set; }
        public ARPSpoofingService(DeviceWithDescription device)
        {
            Device = device;
            cancelTokenSource = new CancellationTokenSource();
            IsScanning = false;
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
                PingAddresses();
            }
        }

        internal void StopScan()
        {
            if (IsScanning == true) { 
                cancelTokenSource.Cancel();
                IsScanning = false;
            }
        }

        private void PingAddresses()
        {
            var communicator = Device.Device.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000);

            MacAddress source = new MacAddress(Device.MacString.Replace('-', ':'));
            MacAddress destination = new MacAddress("ff:ff:ff:ff:ff:ff");
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = source,
                Destination = destination
            };
            var ipVals = Device.IpV4.Split('.').Select(x => Int32.Parse(x)).ToArray();
            IpV4Layer ipv4Layer = new IpV4Layer
            {
                Source = new IpV4Address(Device.IpV4),
                Ttl = 128
            };

            IcmpEchoLayer icmpLayer = new IcmpEchoLayer();

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipv4Layer, icmpLayer);

            for (var i = 0; i < 256; ++i)
            {
                if (i == ipVals[3]) continue;
                // Set IPv4 parameters
                ipv4Layer.CurrentDestination = new IpV4Address("192.168.1." + i);
                ipv4Layer.Identification = (ushort)i;

                // Set ICMP parameters
                icmpLayer.SequenceNumber = (ushort)i;
                icmpLayer.Identifier = (ushort)i;

                // Build the packet
                Packet packet = builder.Build(DateTime.Now);

                // Send down the packet
                communicator.SendPacket(packet);
            }
        }

        private void ListenForReplys()
        {
            var communicator = Device.Device.Open(200, PacketDeviceOpenAttributes.Promiscuous, 1000);

            using (BerkeleyPacketFilter filter = communicator.CreateFilter("icmp"))
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
                                    if (packet.Ethernet.IpV4.Icmp.MessageType == IcmpMessageType.EchoReply)
                                    {
                                        _receivedPackets.Add(packet);
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
