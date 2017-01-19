using System;
using System.IO;
using System.Linq;
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
            
            _siteInfo = JsonConvert.DeserializeObject<SiteInfo>(File.ReadAllText(Directory.GetFiles(Environment.CurrentDirectory, "siteinfo*.json").First()));
            return _siteInfo;
        }
    }
}
