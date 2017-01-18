using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JamCast.Services;

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

        public Manager(IComputerInfoService computerInfo, IMacAddressReportingService macAddressReporting, IImageService imageService, IUserInfoService userInfo)
        {
            _computerInfo = computerInfo;
            _macAddressReporting = macAddressReporting;
            _imageService = imageService;
            _userInfo = userInfo;
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
