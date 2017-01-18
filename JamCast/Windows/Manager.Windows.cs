#if PLATFORM_WINDOWS

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JamCast
{
    partial class Manager
    {
        private NotifyIcon _notifyIcon = null;

        partial void ListenForApplicationExit(Action onExit)
        {
            Application.ApplicationExit += (sender, e) => onExit();
        }

        partial void ConfigureSystemTrayIcon()
        {
            Icon icon;
            using (var memory = new MemoryStream())
            {
                var bytes = _imageService.GetImageFaviconBytes();
                memory.Write(bytes, 0, bytes.Length);
                memory.Seek(0, SeekOrigin.Begin);
                icon = Icon.FromHandle(new Bitmap(memory).GetHicon());
            }

            // Create the container.
            IContainer container = new Container();

            // Create the notify icon.
            _notifyIcon = new NotifyIcon(container)
            {
                Icon = icon,
                Text = "JamCast - " + _userInfo.FullName,
                Visible = true
            };
            this._notifyIcon.DoubleClick += _notifyIcon_DoubleClick;
        }

        private void _notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Application.Exit();
        }

        partial void StartLoop()
        {
            Application.Run();
        }
    }
}

#endif