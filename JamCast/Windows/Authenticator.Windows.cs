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
        private readonly IImageService _imageService;
        private readonly IComputerInfoService _computerInfoService;
        private readonly IJamHostApiService _jamHostApiService;

        public Authenticator(IUserInfoService userInfo, ISiteInfoService siteInfo, IImageService imageService, IComputerInfoService computerInfoService, IJamHostApiService jamHostApiService)
        {
            _userInfo = userInfo;
            _siteInfo = siteInfo;
            _imageService = imageService;
            _computerInfoService = computerInfoService;
            _jamHostApiService = jamHostApiService;
        }

        public void EnsureAuthenticated()
        {
            var computerInfo = _computerInfoService.GetComputerInfo();
            if (computerInfo.PersistentData.SessionId != null && computerInfo.PersistentData.SecretKey != null)
            {
                // Check if the existing session is still valid.
                var authInfo = _jamHostApiService.ValidateSession();
                if (authInfo.IsValid)
                {
                    // Already authenticated.
                    _userInfo.Authenticated = true;
                    _userInfo.FullName = authInfo.FullName;
                    _userInfo.Email = authInfo.EmailAddress;
                    return;
                }
            }

            AuthForm way = new AuthForm(_siteInfo.GetSiteInfo(), _imageService, _jamHostApiService, _computerInfoService);
            if (way.ShowDialog() != DialogResult.OK)
            {
                Environment.Exit(1);
            }

            _userInfo.Authenticated = true;
            _userInfo.FullName = way.AuthResult.FullName;
            _userInfo.Email = way.AuthResult.EmailAddress;

            way.Dispose();
            GC.Collect();
        }
    }
}
