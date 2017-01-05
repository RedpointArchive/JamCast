using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Jamcast
{
    public partial class Manager
    {
        public Manager(DeploymentInfo siteinfo)
        {
            SiteInfo = siteinfo;
        }

        private string _name = "Unknown!";
        private string _email = string.Empty;

        private string _guid;

        public string User { get { return this._name; } }

        public DeploymentInfo SiteInfo { get; private set; }

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

            ReportMacAddress();

            ListenForApplicationExit(OnStop);

            using (var reader = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JamCast", "guid.txt")))
            {
                _guid = reader.ReadToEnd().Trim();
            }

            ConfigureSystemTrayIcon();

            StartLoop();
        }

        private void ReportMacAddress()
        {
            var client = new WebClient();
            client.BaseAddress = SiteInfo.Url;
            var ip_addresses = Program.GetAllKnownIPAddresses();
            var hostname = Program.Host;
            foreach (var mac in Program.GetAllKnownHWAddresses())
            {
                var data = new NameValueCollection();
                data["email"] = _email;
                data["hostname"] = hostname;
                data["mac_address"] = mac.ToString();
                data["ip_addresses"] = string.Join(",", ip_addresses.Select(i => i.ToString()));
                client.UploadValues("/jamcast/reportmacaddress", data);
            }
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
