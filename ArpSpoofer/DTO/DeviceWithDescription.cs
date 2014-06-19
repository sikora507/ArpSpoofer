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
        public string DriverName { get; set; }
        public string IpV4 { get; set; }
        public string Guid { get; set; }
        public LivePacketDevice Device { get; set; }
        public byte[] DeviceMacByte { get; set; }
        public string DeviceMacString
        {
            get
            {
                return BitConverter.ToString(DeviceMacByte);
            }
        }
        public byte[] GatewayIpByte { get; set; }
        public string GatewayIpString
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}", GatewayIpByte[0], GatewayIpByte[1], GatewayIpByte[2], GatewayIpByte[3]).ToString();
            }
        }
    }
}
