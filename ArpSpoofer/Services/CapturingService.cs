using ArpSpoofer.ServiceContracts;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArpSpoofer.Services
{
    public class CapturingService : ICapturingService
    {
        private IWifiDeviceService _deviceService;
        private PacketCommunicator _communicator;

        public event EventHandler<Packet> PacketCaptured;
        public event EventHandler<string> StatusChanged;

        //private bool stopSignal;
        public CapturingService(IWifiDeviceService deviceService)
        {
            _deviceService = deviceService;
        }
        public async void StartCapturing()
        {
            //stopSignal = false;
            _communicator = _deviceService.DeviceWithDescription.Device.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);
            FireStatusChanged("Listening on " + _deviceService.DeviceWithDescription.Device.Description + "...");
            
            using (BerkeleyPacketFilter filter = _communicator.CreateFilter("tcp port 80 or 443"))
            {
                // Set the filter
                _communicator.SetFilter(filter);
            }
            
            Packet packet;
            PacketCommunicatorReceiveResult receiveResult;
            do
            {
                receiveResult = await Task.Factory.StartNew <PacketCommunicatorReceiveResult>(
                    ()=>{
                        PacketCommunicatorReceiveResult result = _communicator.ReceivePacket(out packet);
                        switch (result)
                        {
                            case PacketCommunicatorReceiveResult.Timeout:
                                // Timeout elapsed
                                break;
                            case PacketCommunicatorReceiveResult.Ok:
                                PacketHandler(packet);
                                break;
                            case PacketCommunicatorReceiveResult.Eof:
                                break;
                            default:
                                throw new InvalidOperationException("The result " + result + " shoudl never be reached here");
                        }
                        return result;
                    }
                );
            } while (receiveResult!=PacketCommunicatorReceiveResult.Eof);
        }

        public void StopCapturing()
        {
            //stopSignal = true;
            _communicator.Break();
            FireStatusChanged("Listening stopped.");
        }

        private void PacketHandler(Packet packet)
        {
            var ip = packet.Ethernet.IpV4;
            var tcp = ip.Tcp;
            if (PacketCaptured != null)
            {
                PacketCaptured(this, packet);
            }
        }

        private void FireStatusChanged(string status)
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, status);
            }
        }
    }
}
