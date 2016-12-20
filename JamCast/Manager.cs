using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamcast
{
    public partial class Manager
    {
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

            ListenForApplicationExit(OnStop);

            using (var reader = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JamCast", "guid.txt")))
            {
                _guid = reader.ReadToEnd().Trim();
            }

            //_pubSub = new PubSubPersistent();
            //this._pubSub.TopicsToCreate.Add("projectors");
            //this._pubSub.TopicsToCreate.Add("client-" + _guid);
            //this._pubSub.SubscriptionsToCreate.Add(new KeyValuePair<string, string>(
            //    "client-" + _guid,
            //    "client-" + _guid));
            //this._pubSub.Reconfigure(
            //    "http://melbggj16.info/jamcast/gettoken/api",
            //    "melbourne-global-game-jam-16");
            //this._pubSub.MessageRecieved += PubSubOnMessageRecieved;

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
