using System;
using System.Windows.Forms;
using Client;
using JamCast.Services;

namespace JamCast
{
    public class Authenticator : IAuthenticator
    {
        private readonly IUserInfoService _userInfo;
        private readonly ISiteInfoService _siteInfo;

        public Authenticator(IUserInfoService userInfo, ISiteInfoService siteInfo)
        {
            _userInfo = userInfo;
            _siteInfo = siteInfo;
        }

        public void EnsureAuthenticated()
        {
            AuthForm way = new AuthForm(_siteInfo.GetSiteInfo());
            if (way.ShowDialog() != DialogResult.OK)
            {
                Environment.Exit(1);
            }

            _userInfo.FullName = way.AuthResult.FullName;
            _userInfo.Email = way.AuthResult.EmailAddress;

            way.Dispose();
            GC.Collect();
        }
    }
}
