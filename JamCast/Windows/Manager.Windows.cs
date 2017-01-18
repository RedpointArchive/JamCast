#if PLATFORM_WINDOWS

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JamCast.Models;
using JamCast.Services;
using JamCast.Windows;

namespace JamCast
{
    partial class Manager
    {
        private NotifyIcon _notifyIcon = null;

        private MenuItem _userInfoMenuItem = null;
        private MenuItem _accountTypeMenuItem = null;
        private MenuItem _roleInfoMenuItem = null;
        private MenuItem _switchRoleMenuItem = null;

        private Timer _timer = null;

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

            _userInfoMenuItem = new MenuItem();
            _userInfoMenuItem.Text = "...";
            _userInfoMenuItem.Enabled = false;
            _accountTypeMenuItem = new MenuItem();
            _accountTypeMenuItem.Text = "...";
            _accountTypeMenuItem.Enabled = false;
            _roleInfoMenuItem = new MenuItem();
            _roleInfoMenuItem.Text = "...";
            _roleInfoMenuItem.Enabled = false;
            _switchRoleMenuItem = new MenuItem();
            _switchRoleMenuItem.Text = "...";
            _switchRoleMenuItem.Enabled = false;
            _switchRoleMenuItem.Visible = false;
            _switchRoleMenuItem.Click += _switchRoleMenuItem_Click;

            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(_userInfoMenuItem);
            contextMenu.MenuItems.Add(_accountTypeMenuItem);
            contextMenu.MenuItems.Add(_roleInfoMenuItem);
            contextMenu.MenuItems.Add(_switchRoleMenuItem);
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Exit", _notifyIcon_Exit);

            contextMenu.Popup += ContextMenuOnPopup;

            var container = new Container();

            _notifyIcon = new NotifyIcon(container)
            {
                Icon = icon,
                Text = "JamCast - " + _userInfo.FullName,
                Visible = true,
                ContextMenu = contextMenu
            };

            _timer = new Timer(container);
            _timer.Interval = 100;
            _timer.Tick += _timer_Tick;
            _timer.Enabled = true;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var desiredRole = _roleInfoService.GetRoleInfo();

            if (_currentRole is IClientRole && desiredRole == RoleInfo.Projector)
            {
                _currentRole.End();
                _currentRole = null;
            }

            if (_currentRole is IProjectorRole && desiredRole == RoleInfo.Client)
            {
                _currentRole.End();
                _currentRole = null;
            }

            if (_currentRole == null)
            {
                if (desiredRole == RoleInfo.Client)
                {
                    _currentRole = _clientRole;
                }
                if (desiredRole == RoleInfo.Projector)
                {
                    _currentRole = _projectorRole;
                }
            }

            _currentRole?.Update();
        }

        private void ContextMenuOnPopup(object sender, EventArgs eventArgs)
        {
            var role = _roleInfoService.GetRoleInfo();

            _userInfoMenuItem.Text = _userInfo.FullName;
            _accountTypeMenuItem.Text = "Account Type: " + _userInfo.AccountType;
            _roleInfoMenuItem.Text = "Role: " + role;

            if (_userInfo.AccountType == "developer" || _userInfo.AccountType == "organiser")
            {
                _switchRoleMenuItem.Visible = true;
                if (role == RoleInfo.Client)
                {
                    _switchRoleMenuItem.Text = "Switch to Projector";
                }
                else
                {
                    _switchRoleMenuItem.Text = "Switch to Client";
                }
                _switchRoleMenuItem.Enabled = true;
            }
            else
            {
                _switchRoleMenuItem.Enabled = false;
                _switchRoleMenuItem.Visible = false;
            }
        }

        private void _switchRoleMenuItem_Click(object sender, EventArgs e)
        {
            _roleInfoService.SwitchRole();
        }

        private void _notifyIcon_Exit(object sender, EventArgs e)
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