using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace JamCast.Models
{
    public class ComputerInfo
    {
        public PersistentComputerInfo PersistentData { get; set; }

        public string Host { get; set; }

        public IPAddress[] IpAddresses { get; set; }

        public PhysicalAddress[] HwAddresses { get; set; }
    }
}
