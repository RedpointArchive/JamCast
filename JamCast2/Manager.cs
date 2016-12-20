using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamcast2
{
    public partial class Manager
    {
        private string _name = "Unknown!";
        private string _email = string.Empty;

        private string _guid;

        public string User { get { return this._name; } }


        private void LoadUsername()
        {
            var userPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast",
                "user.txt");
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

        partial void LoginPrompt();
    }
}
