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
        private readonly ISiteInfoService _siteInfo;
        private readonly IComputerInfoService _computerInfo;
        private readonly IUserInfoService _userInfo;

        public MacAddressReportingService(ISiteInfoService siteInfo, IComputerInfoService computerInfo, IUserInfoService userInfo)
        {
            _siteInfo = siteInfo;
            _computerInfo = computerInfo;
            _userInfo = userInfo;
        }

        public void ReportMacAddress()
        {
            var site = _siteInfo.GetSiteInfo();
            var computer = _computerInfo.GetComputerInfo();

            var client = new WebClient();
            client.BaseAddress = site.Url;
            var ipAddresses = computer.IpAddresses;
            var hostname = computer.Host;
            foreach (var mac in computer.HwAddresses)
            {
                var data = new NameValueCollection();
                data["email"] = _userInfo.Email;
                data["hostname"] = hostname;
                data["mac_address"] = mac.ToString();
                data["ip_addresses"] = string.Join(",", ipAddresses.Select(i => i.ToString()));
                client.UploadValues("/jamcast/reportmacaddress", data);
            }
        }
    }
}
