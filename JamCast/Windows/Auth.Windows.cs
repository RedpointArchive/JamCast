using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using JamCast.Models;
using JamCast.Services;

namespace Client
{
    public partial class AuthForm : Form
    {
        private readonly SiteInfo _siteInfo;
        private readonly IImageService _imageService;
        private readonly IJamHostApiService _jamHostApiService;
        private readonly IComputerInfoService _computerInfoService;

        public AuthForm(SiteInfo siteInfo, IImageService imageService, IJamHostApiService jamHostApiService, IComputerInfoService computerInfoService)
        {
            _siteInfo = siteInfo;
            _imageService = imageService;
            _jamHostApiService = jamHostApiService;
            _computerInfoService = computerInfoService;

            InitializeComponent();
        }

        private void AuthForm_Load(object sender, EventArgs e)
        {
            Text = _siteInfo.SiteName;
            _titleLabel.Text = "Welcome to " + _siteInfo.SiteName;
            _descriptionLabel.Text = "Sign in with the email address and password you used to buy your ticket on " +
                                     _siteInfo.Url;
            
            using (var memory = new MemoryStream())
            {
                var bytes = _imageService.GetImageCoverBytes();
                memory.Write(bytes, 0, bytes.Length);
                memory.Seek(0, SeekOrigin.Begin);
                _imageBox.Image = new Bitmap(memory);
            }

            using (var memory = new MemoryStream())
            {
                var bytes = _imageService.GetImageFaviconBytes();
                memory.Write(bytes, 0, bytes.Length);
                memory.Seek(0, SeekOrigin.Begin);
                this.Icon = Icon.FromHandle(new Bitmap(memory).GetHicon());
            }
        }

        private void _login_Click(object sender, System.EventArgs e)
        {
            _emailAddress.Enabled = false;
            _password.Enabled = false;
            _login.Enabled = false;
            _login.Text = "Logging in...";

            var emailAddress = _emailAddress.Text;
            var password = _password.Text;

            var needsToFinalize = true;

            Task.Run(() =>
            {
                try
                {
                    var authenticated = _jamHostApiService.Authenticate(emailAddress, password, _computerInfoService.GetComputerInfo().Host);
                    if (authenticated.IsValid)
                    {
                        _computerInfoService.SetSessionInformation(authenticated.SessionId, authenticated.SecretKey); 

                        AuthResult = authenticated;
                        this.Invoke(new Action(() =>
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                            needsToFinalize = false;
                        }));
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            _emailAddress.Enabled = true;
                            _password.Enabled = true;
                            _login.Enabled = true;
                            _login.Text = "Login";
                            _error.Text = authenticated.Error;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        _error.Text = ex.Message;
                    }));
                }
                finally
                {
                    if (needsToFinalize)
                    {
                        this.Invoke(new Action(() =>
                        {
                            _emailAddress.Enabled = true;
                            _password.Enabled = true;
                            _login.Enabled = true;
                            _login.Text = "Login";
                        }));
                    }
                }
            });
        }

        public AuthInfo AuthResult { get; private set; }

        private void _emailAddress_KeyUp(object sender, KeyEventArgs e)
        {
            if (_emailAddress.Enabled)
            {
                UpdateLoginButton();
            }
        }

        private void _password_KeyUp(object sender, KeyEventArgs e)
        {
            if (_emailAddress.Enabled)
            {
                UpdateLoginButton();
            }
        }

        private void UpdateLoginButton()
        {
            _login.Enabled = (_emailAddress.Text.Length > 0 &&
                              _password.Text.Length > 0);
        }
    }
}