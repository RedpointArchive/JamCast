using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Client;
using JamCast.Models;
using Newtonsoft.Json;

namespace JamCast.Services
{
    public interface IJamHostApiService
    {
        AuthInfo Authenticate(string email, string password, string deviceIdentifier);

        AuthInfo ValidateSession();

        void ReportMacAddressInformation(string hostName, string macAddress, string ipAddresses);

        StreamInfo ProjectorPing();

        StreamInfo ClientPing();

        RoleInfo GetSessionRole();

        void SetSessionRole(string newRole);

        void UploadClientScreenshot(MemoryStream memory);
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

            dynamic resultParsed;
            try
            {
                var result = client.UploadValues(url, "POST", new NameValueCollection
                {
                    {"email", email},
                    {"password", password}
                });
                resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            }
            catch (WebException c)
            {
                resultParsed = new { has_error = true, error=$"WebException: {c.Message}" };
            }

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
                AccountType = (string)resultParsed.result.accountType
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
                    AccountType = (string)resultParsed.result.accountType
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

        public StreamInfo ProjectorPing()
        {
            var siteInfo = _siteInfoService.GetSiteInfo();
            var computerInfo = _computerInfoService.GetComputerInfo();

            var url = siteInfo.Url + @"jamcast/api/projector/ping?_domain=" + siteInfo.Id;
            var client = new WebClient();
            var result = client.UploadValues(url, "POST", new NameValueCollection
            {
                {"sessionId", computerInfo.PersistentData.SessionId},
                {"secretKey", computerInfo.PersistentData.SecretKey}
            });

            var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            var hasError = (bool?)resultParsed.has_error;
            if (hasError.HasValue && hasError.Value)
            {
                throw new Exception((string) resultParsed.error);
            }

            return new StreamInfo
            {
                RtmpUrl = (string)resultParsed.result.rtmpUrl,
                RtmpsUrl = (string)resultParsed.result.rtmpsUrl,
                ShouldStream = true,
                ActiveClientId = (string)resultParsed.result.activeClientId,
                ActiveClientFullName = (string)resultParsed.result.activeClientFullName,
                ActiveClientScreenshotHash = (string)resultParsed.result.activeClientScreenshotHash,
                OperationMode = ((string)resultParsed.result.operationMode) == "rtmp" ? OperationMode.Rtmp : OperationMode.Jpeg
            };
        }

        public StreamInfo ClientPing()
        {
            var siteInfo = _siteInfoService.GetSiteInfo();
            var computerInfo = _computerInfoService.GetComputerInfo();

            var url = siteInfo.Url + @"jamcast/api/client/ping?_domain=" + siteInfo.Id;
            var client = new WebClient();

            var result = client.UploadValues(url, "POST", new NameValueCollection
            {
                {"sessionId", computerInfo.PersistentData.SessionId},
                {"secretKey", computerInfo.PersistentData.SecretKey}
            });

            var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            var hasError = (bool?)resultParsed.has_error;
            if (hasError.HasValue && hasError.Value)
            {
                throw new Exception((string)resultParsed.error);
            }

            return new StreamInfo
            {
                RtmpUrl = (string)resultParsed.result.rtmpUrl,
                RtmpsUrl = (string)resultParsed.result.rtmpsUrl,
                ShouldStream = (bool)resultParsed.result.shouldStream,
                OperationMode = ((string)resultParsed.result.operationMode) == "rtmp" ? OperationMode.Rtmp : OperationMode.Jpeg
            };
        }

        public RoleInfo GetSessionRole()
        {
            var siteInfo = _siteInfoService.GetSiteInfo();
            var computerInfo = _computerInfoService.GetComputerInfo();

            var url = siteInfo.Url + @"jamcast/api/getrole?_domain=" + siteInfo.Id;
            var client = new WebClient();

            var result = client.UploadValues(url, "POST", new NameValueCollection
            {
                {"sessionId", computerInfo.PersistentData.SessionId},
                {"secretKey", computerInfo.PersistentData.SecretKey}
            });

            var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            var hasError = (bool?)resultParsed.has_error;
            if (hasError.HasValue && hasError.Value)
            {
                throw new Exception((string)resultParsed.error);
            }

            if ((string) resultParsed.result == "projector")
            {
                return RoleInfo.Projector;
            }

            return RoleInfo.Client;
        }

        public void SetSessionRole(string role)
        {
            var siteInfo = _siteInfoService.GetSiteInfo();
            var computerInfo = _computerInfoService.GetComputerInfo();

            var url = siteInfo.Url + @"jamcast/api/setrole?_domain=" + siteInfo.Id;
            var client = new WebClient();

            var result = client.UploadValues(url, "POST", new NameValueCollection
            {
                {"sessionId", computerInfo.PersistentData.SessionId},
                {"secretKey", computerInfo.PersistentData.SecretKey},
                {"role", role}
            });

            var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            var hasError = (bool?)resultParsed.has_error;
            if (hasError.HasValue && hasError.Value)
            {
                throw new Exception((string)resultParsed.error);
            }
        }

        public void UploadClientScreenshot(MemoryStream memory)
        {
            var siteInfo = _siteInfoService.GetSiteInfo();
            var computerInfo = _computerInfoService.GetComputerInfo();

            var url = siteInfo.Url + @"jamcast/api/client/uploadscreenshot?_domain=" + siteInfo.Id;

            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(computerInfo.PersistentData.SessionId), "sessionId");
            form.Add(new StringContent(computerInfo.PersistentData.SecretKey), "secretKey");

            using (var gzip = new GZipStream(memory, CompressionMode.Compress))
            {
                using (var compressedMemory = new MemoryStream())
                {
                    gzip.CopyTo(compressedMemory);
                    compressedMemory.Seek(0, SeekOrigin.Begin);
                    
                    var br = new byte[compressedMemory.Length];
                    compressedMemory.Read(br, 0, br.Length);

                    form.Add(new ByteArrayContent(br), "screenshot", "screenshot.jpeg.gz");
                    Task.Run(async () => { await httpClient.PostAsync(url, form); }).Wait();
                }
            }
        }
    }
}
