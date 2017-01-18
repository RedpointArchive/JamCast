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

        partial void LoginPrompt()
        {
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