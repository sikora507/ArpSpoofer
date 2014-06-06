﻿using PcapDotNet.Core;
using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArpSpoofer.Services
{
    public class CapturingService
    {
        private LivePacketDevice _device;
        private PacketCommunicator _communicator;

        public event EventHandler<PacketCapturedEventArgs> PacketCaptured;
        public event EventHandler<string> StatusChanged;

        //private bool stopSignal;
        public CapturingService(LivePacketDevice device)
        {
            _device = device;
        }
        public async void StartCapturing()
        {
            //stopSignal = false;
            _communicator = _device.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);
            FireStatusChanged("Listening on " + _device.Description + "...");

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
            PacketCapturedEventArgs args = new PacketCapturedEventArgs
            {
                PacketLength = packet.Length,
                TimestampString = packet.Timestamp.ToShortTimeString()
            };
            FirePacketCaptured(args);
        }

        private void FirePacketCaptured(PacketCapturedEventArgs args)
        {
            if (PacketCaptured != null)
            {
                PacketCaptured(this, args);
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
