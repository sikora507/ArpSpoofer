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
        public byte[] MacByte { get; set; }
        public string MacString
        {
            get
            {
                return BitConverter.ToString(MacByte);
            }
        }
        public byte[] GatewayByte { get; set; }
        public string GatewayString
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}", GatewayByte[0], GatewayByte[1], GatewayByte[2], GatewayByte[3]).ToString();
            }
        }
    }
}
