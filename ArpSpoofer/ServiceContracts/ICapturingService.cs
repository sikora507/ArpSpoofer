using PcapDotNet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArpSpoofer.ServiceContracts
{
    public interface ICapturingService
    {
        event EventHandler<Packet> PacketCaptured;
        event EventHandler<string> StatusChanged;
        void StartCapturing();
        void StopCapturing();
    }
}
