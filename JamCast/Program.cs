using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Linq;
using Jamcast;

namespace Jamcast
{
    public static class Program
    {
        public static DeploymentInfo SiteInfo {get; private set;}

        private static Guid guid;

        public static string Status { get; private set; }
        public static string BasePath { get; private set; }
        public static string Host { get; private set; }

        public static Manager Manager { get; private set; }

        public static string GetLocalIPAddress()
        {
            IPHostEntry host;
            var localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static IPAddress[] GetAllKnownIPAddresses()
        {
            try
            {
                return Dns.GetHostAddresses("");
            }
            catch (Exception)
            {
                return new IPAddress[0];
            }
        }

        public static IEnumerable<PhysicalAddress> GetAllKnownHWAddresses()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    .Select(nic => nic.GetPhysicalAddress());
            }
            catch (Exception)
            {
                return new PhysicalAddress[0];
            }
        }
        
        internal static void Main(string[] args)
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast");
            Directory.CreateDirectory(path);
            BasePath = path;

            Status = "Querying Computer GUID";

            if (File.Exists(Path.Combine(BasePath, "guid.txt")))
            {
                using (var reader = new StreamReader(Path.Combine(BasePath, "guid.txt")))
                {
                    guid = Guid.Parse(reader.ReadToEnd().Trim());
                }
            }
            else
            {
                guid = Guid.NewGuid();
                using (var writer = new StreamWriter(Path.Combine(BasePath, "guid.txt")))
                {
                    writer.Write(guid.ToString());
                }
            }

            Status = "Querying Hostname";

            try
            {
                Host = Dns.GetHostEntry("").HostName;
                if (String.IsNullOrEmpty(Host)) throw new Exception();
            }
            catch
            {
                try
                {
                    Host = Environment.MachineName;
                    if (String.IsNullOrEmpty(Host)) throw new Exception();
                }
                catch
                {
                    try
                    {
                        Host = GetLocalIPAddress();
                    }
                    catch
                    {
                        Host = "Unknown - " + guid;
                    }
                }
            }

            SiteInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<DeploymentInfo>(File.ReadAllText("siteinfo.json"));

            Manager = new Manager(SiteInfo);
            Manager.Run();
        }
    }
}

