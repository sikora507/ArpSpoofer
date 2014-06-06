using PcapDotNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArpSpoofer.DTO
{
    public class DeviceWithDescription
    {
        public string Name { get; set; }
        public string Driver { get; set; }
        public string IpV4 { get; set; }
        public string Guid { get; set; }
        public LivePacketDevice Device { get; set; }
    }
}
