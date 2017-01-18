using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;

namespace JamCast.Services
{
    public interface IMacAddressReportingService
    {
        void ReportMacAddress();
    }

    public class MacAddressReportingService : IMacAddressReportingService
    {
        private readonly IComputerInfoService _computerInfo;
        private readonly IJamHostApiService _jamHostApiService;

        public MacAddressReportingService(IComputerInfoService computerInfo, IJamHostApiService jamHostApiService)
        {
            _computerInfo = computerInfo;
            _jamHostApiService = jamHostApiService;
        }

        public void ReportMacAddress()
        {
            try
            {
                var computer = _computerInfo.GetComputerInfo();
                
                foreach (var mac in computer.HwAddresses)
                {
                    _jamHostApiService.ReportMacAddressInformation(
                        computer.Host,
                        mac.ToString(),
                        string.Join(",", computer.IpAddresses.Select(i => i.ToString())));
                }
            }
            catch (Exception ex)
            {
                // Do nothing.
            }
        }
    }
}
