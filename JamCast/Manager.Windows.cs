#if PLATFORM_WINDOWS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamcast2
{
    partial class Manager
    {
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
    }
}

#endif