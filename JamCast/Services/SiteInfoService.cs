using System.IO;
using JamCast.Models;
using Newtonsoft.Json;

namespace JamCast.Services
{
    public interface ISiteInfoService
    {
        SiteInfo GetSiteInfo();
    }

    public class SiteInfoService : ISiteInfoService
    {
        private SiteInfo _siteInfo;

        public SiteInfo GetSiteInfo()
        {
            if (_siteInfo != null)
            {
                return _siteInfo;
            }

            _siteInfo = JsonConvert.DeserializeObject<SiteInfo>(File.ReadAllText("siteinfo.json"));
            return _siteInfo;
        }
    }
}
