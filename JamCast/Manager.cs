using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using JamCast.Services;

namespace Jamcast
{
    public interface IManager
    {
        void Run();
    }

    public partial class Manager : IManager
    {
        private readonly IComputerInfoService _computerInfo;
        private readonly IMacAddressReportingService _macAddressReporting;

        public Manager(IComputerInfoService computerInfo, IMacAddressReportingService macAddressReporting)
        {
            _computerInfo = computerInfo;
            _macAddressReporting = macAddressReporting;
        }

        private string _name = "Unknown!";
        private string _email = string.Empty;

        private string _guid;

        public string User { get { return this._name; } }

        string userPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "JamCast",
            "user.txt");

        private void LoadUsername()
        {
            Directory.CreateDirectory(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "JamCast"));

            if (File.Exists(userPath))
            {
                using (var reader = new StreamReader(userPath))
                {
                    _name = reader.ReadLine()?.Trim();
                    _email = reader.ReadLine()?.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(_name) || string.IsNullOrWhiteSpace(_email))
            {
                LoginPrompt();
            }
        }

        public void Run()
        {
            LoadUsername();

            _macAddressReporting.ReportMacAddress();

            ListenForApplicationExit(OnStop);

            using (var reader = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JamCast", "guid.txt")))
            {
                _guid = reader.ReadToEnd().Trim();
            }

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

        partial void LoginPrompt();

        partial void ConfigureSystemTrayIcon();

        partial void StartLoop();
    }
}
