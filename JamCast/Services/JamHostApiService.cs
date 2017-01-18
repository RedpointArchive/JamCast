using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Client;
using Newtonsoft.Json;

namespace JamCast.Services
{
    public interface IJamHostApiService
    {
        AuthInfo Authenticate(string email, string password, string deviceIdentifier);

        AuthInfo ValidateSession();

        void ReportMacAddressInformation(string hostName, string macAddress, string ipAddresses);
    }

    public class JamHostApiService : IJamHostApiService
    {
        private readonly ISiteInfoService _siteInfoService;
        private readonly IComputerInfoService _computerInfoService;

        public JamHostApiService(ISiteInfoService siteInfoService, IComputerInfoService computerInfoService)
        {
            _siteInfoService = siteInfoService;
            _computerInfoService = computerInfoService;
        }

        public AuthInfo Authenticate(string email, string password, string deviceIdentifier)
        {
            var siteInfo = _siteInfoService.GetSiteInfo();

            var url = siteInfo.Url + @"jamcast/api/authenticate?_domain=" + siteInfo.Id;
            var client = new WebClient();

            var result = client.UploadValues(url, "POST", new NameValueCollection
            {
                {"email", email},
                {"password", password}
            });

            var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            var hasError = (bool?)resultParsed.has_error;
            if (hasError.HasValue && hasError.Value)
            {
                return new AuthInfo
                {
                    IsValid = false,
                    Error = (string)resultParsed.error
                };
            }

            return new AuthInfo
            {
                IsValid = true,
                FullName = (string)resultParsed.result.fullName,
                EmailAddress = (string)resultParsed.result.email,
                SessionId = (string)resultParsed.result.sessionId,
                SecretKey = (string)resultParsed.result.secretKey,
            };
        }

        public AuthInfo ValidateSession()
        {
            try
            {
                var siteInfo = _siteInfoService.GetSiteInfo();
                var computerInfo = _computerInfoService.GetComputerInfo();

                var url = siteInfo.Url + @"jamcast/api/validatesession?_domain=" + siteInfo.Id;
                var client = new WebClient();

                var result = client.UploadValues(url, "POST", new NameValueCollection
                {
                    {"sessionId", computerInfo.PersistentData.SessionId},
                    {"secretKey", computerInfo.PersistentData.SecretKey}
                });

                var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
                var hasError = (bool?) resultParsed.has_error;
                if (hasError.HasValue && hasError.Value)
                {
                    return new AuthInfo
                    {
                        IsValid = false,
                        Error = (string) resultParsed.error
                    };
                }

                return new AuthInfo
                {
                    IsValid = true,
                    FullName = (string)resultParsed.result.fullName,
                    EmailAddress = (string)resultParsed.result.email,
                    SessionId = (string)resultParsed.result.sessionId,
                    SecretKey = (string)resultParsed.result.secretKey,
                };
            }
            catch (Exception ex)
            {
                return new AuthInfo
                {
                    IsValid = false,
                    Error = ex.ToString()
                };
            }
        }

        public void ReportMacAddressInformation(string hostName, string macAddress, string ipAddresses)
        {
            try
            {
                var siteInfo = _siteInfoService.GetSiteInfo();
                var computerInfo = _computerInfoService.GetComputerInfo();

                var url = siteInfo.Url + @"jamcast/api/reportmacaddress?_domain=" + siteInfo.Id;
                var client = new WebClient();

                var result = client.UploadValues(url, "POST", new NameValueCollection
                {
                    {"sessionId", computerInfo.PersistentData.SessionId},
                    {"secretKey", computerInfo.PersistentData.SecretKey},
                    {"hostname", hostName},
                    {"mac_address", macAddress},
                    {"ip_addresses", ipAddresses}
                });

                var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
                var hasError = (bool?)resultParsed.has_error;
                if (hasError.HasValue && hasError.Value)
                {
                    // Do nothing.
                    return;
                }

                // Success.
                return;
            }
            catch (Exception)
            {
                // Do nothing.
            }
        }
    }
}
