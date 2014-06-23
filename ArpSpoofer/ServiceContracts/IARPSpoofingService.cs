using ArpSpoofer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArpSpoofer.ServiceContracts
{
    public interface IARPSpoofingService
    {
        void StopScan();
        void Scan();
        void StopPoisoning();
        void StartPoisoning();
        event EventHandler<double> ScanTick;
        event EventHandler<IpMacPair> NewIpMacFound;
        bool IsScanning { get; set; }
        bool IsPoisoning { get; set; }
    }
}
