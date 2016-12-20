#if PLATFORM_WINDOWS

using Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jamcast
{
    partial class Manager
    {
        private TrayIcon _trayIcon;

        partial void LoginPrompt() {
        // Ask the person who they are ;)
        AuthForm way = new AuthForm();
                if (way.ShowDialog() != DialogResult.OK)
                {
                    Environment.Exit(1);
                    return;
                }
    _name = way.AuthResult.FullName;
                _email = way.AuthResult.EmailAddress;
                way.Dispose();
                GC.Collect();

                using (var writer = new StreamWriter(userPath))
                {
                    writer.WriteLine(_name);
                    writer.WriteLine(_email);
                }
        }

        partial void ListenForApplicationExit(Action onExit)
        {
            Application.ApplicationExit += (sender, e) => onExit();
        }

        partial void ConfigureSystemTrayIcon()
        {
            _trayIcon = new TrayIcon(this);
        }

        partial void StartLoop()
        {
            Application.Run();
        }
    }
}

#endif