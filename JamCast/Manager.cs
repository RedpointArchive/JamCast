using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JamCast.Models;
using JamCast.Services;
using Protoinject;

namespace JamCast
{
    public interface IManager
    {
        void Run();
    }

    public partial class Manager : IManager
    {
        private readonly IComputerInfoService _computerInfo;
        private readonly IMacAddressReportingService _macAddressReporting;
        private readonly IImageService _imageService;
        private readonly IUserInfoService _userInfo;
        private readonly IJamHostApiService _jamHostApiService;
        private readonly IRoleInfoService _roleInfoService;
        private readonly IClientRole _clientRole;
        private readonly IProjectorRole _projectorRole;
        private IRole _currentRole;

        public Manager(IComputerInfoService computerInfo, IMacAddressReportingService macAddressReporting, IImageService imageService, IUserInfoService userInfo, IJamHostApiService jamHostApiService, IRoleInfoService roleInfoService, IClientRole clientRole, IProjectorRole projectorRole)
        {
            _computerInfo = computerInfo;
            _macAddressReporting = macAddressReporting;
            _imageService = imageService;
            _userInfo = userInfo;
            _jamHostApiService = jamHostApiService;
            _roleInfoService = roleInfoService;
            _clientRole = clientRole;
            _projectorRole = projectorRole;
        }

        public void Run()
        {
            if (!_userInfo.Authenticated)
            {
                throw new InvalidOperationException("Not authenticated when Manager started - This should be impossible!");
            }

            _macAddressReporting.ReportMacAddress();

            ListenForApplicationExit(OnStop);

            ConfigureSystemTrayIcon();

            StartLoop();
        }

        /// <summary>
        /// This will be called just before we terminate.
        /// </summary>
        private void OnStop()
        {
            
        }

        partial void ListenForApplicationExit(Action onExit);

        partial void ConfigureSystemTrayIcon();

        partial void StartLoop();
    }
}
