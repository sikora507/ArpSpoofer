using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArpSpoofer.Services
{
    public class PacketCapturedEventArgs : EventArgs
    {
        public string TimestampString { get; set; }
        public int PacketLength { get; set; }
    }
}
