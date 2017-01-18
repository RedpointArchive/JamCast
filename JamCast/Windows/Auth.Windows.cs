using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JamCast.Models;
using Newtonsoft.Json;

namespace Client
{
    public partial class AuthForm : Form
    {
        private readonly SiteInfo _siteInfo;

        public AuthForm(SiteInfo siteInfo)
        {
            _siteInfo = siteInfo;

            InitializeComponent();
        }

        private void AuthForm_Load(object sender, EventArgs e)
        {
            Text = _siteInfo.SiteName;
            _titleLabel.Text = "Welcome to " + _siteInfo.SiteName;
            _descriptionLabel.Text = "Sign in with the email address and password you used to buy your ticket on " +
                                     _siteInfo.Url;

            using (var client = new WebClient())
            {
                using (var memory = new MemoryStream())
                {
                    var bytes = client.DownloadData(_siteInfo.ImageCover);
                    memory.Write(bytes, 0, bytes.Length);
                    memory.Seek(0, SeekOrigin.Begin);
                    _imageBox.Image = new Bitmap(memory);
                }

                using (var memory = new MemoryStream())
                {
                    var bytes = client.DownloadData(_siteInfo.ImageFavicon);
                    memory.Write(bytes, 0, bytes.Length);
                    memory.Seek(0, SeekOrigin.Begin);
                    this.Icon = Icon.FromHandle(new Bitmap(memory).GetHicon());
                }
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

            Task.Run(async () =>
            {
                try
                {
                    var authenticated = await Authenticate(emailAddress, password);
                    if (authenticated.IsValid)
                    {
                        AuthResult = authenticated;
                        this.Invoke(new Action(() =>
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
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
                    this.Invoke(new Action(() =>
                    {
                        _emailAddress.Enabled = true;
                        _password.Enabled = true;
                        _login.Enabled = true;
                        _login.Text = "Login";
                    }));
                }
            });
        }

        public AuthInfo AuthResult { get; private set; }

        private async Task<AuthInfo> Authenticate(string emailAddress, string password)
        {
            var url = _siteInfo.Url + @"jamcast/api/authenticate";
            var client = new WebClient();

            var result = await client.UploadValuesTaskAsync(url, "POST", new NameValueCollection
            {
                {"email", emailAddress},
                {"password", password}
            });

            var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            var has_error = (bool?)resultParsed.has_error;
            if (has_error.HasValue && has_error.Value)
            {
                return new AuthInfo
                {
                    IsValid = false,
                    Error = (string) resultParsed.error
                };
            }
            else
            {
                return new AuthInfo
                {
                    IsValid = true,
                    FullName = (string) resultParsed.fullName,
                    EmailAddress = (string)resultParsed.email,
                };
            }
        }

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