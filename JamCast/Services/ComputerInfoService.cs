using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using JamCast.Models;
using Newtonsoft.Json;

namespace JamCast.Services
{
    public interface IComputerInfoService
    {
        PersistentComputerInfo GetPersistentComputerInfo();
        ComputerInfo GetComputerInfo();

        void SetSessionInformation(string sessionId, string secretKey);
    }

    public class ComputerInfoService : IComputerInfoService
    {
        private PersistentComputerInfo _persistentComputerInfo;
        private ComputerInfo _computerInfo;

        private string GetPersistentFilePath()
        {
            var persistence = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast");
            Directory.CreateDirectory(persistence);

            var persistenceFile = Path.Combine(persistence, "info.json");

            return persistenceFile;
        }

        public PersistentComputerInfo GetPersistentComputerInfo()
        {
            if (_persistentComputerInfo != null)
            {
                return _persistentComputerInfo;
            }
            
            var persistenceFile = GetPersistentFilePath();
            if (!File.Exists(persistenceFile))
            {
                var info = new PersistentComputerInfo
                {
                    Guid = Guid.NewGuid().ToString()
                };
                File.WriteAllText(persistenceFile, JsonConvert.SerializeObject(info));
            }

            _persistentComputerInfo =
                JsonConvert.DeserializeObject<PersistentComputerInfo>(File.ReadAllText(persistenceFile));
            return _persistentComputerInfo;
        }

        public void SetSessionInformation(string sessionId, string secretKey)
        {
            var info = GetPersistentComputerInfo();
            info.SessionId = sessionId;
            info.SecretKey = secretKey;
            File.WriteAllText(GetPersistentFilePath(), JsonConvert.SerializeObject(info));
        }

        public ComputerInfo GetComputerInfo()
        {
            if (_computerInfo != null)
            {
                return _computerInfo;
            }

            _computerInfo = new ComputerInfo
            {
                Host = GetHostname(),
                IpAddresses = GetAllKnownIPAddresses(),
                HwAddresses = GetAllKnownHWAddresses(),
                PersistentData = GetPersistentComputerInfo()
            };
            return _computerInfo;
        }

        private string GetHostname()
        {
            string host;
            try
            {
                host = Dns.GetHostEntry("").HostName;
                if (string.IsNullOrEmpty(host)) throw new Exception();
            }
            catch
            {
                try
                {
                    host = Environment.MachineName;
                    if (string.IsNullOrEmpty(host)) throw new Exception();
                }
                catch
                {
                    try
                    {
                        host = GetLocalIPAddress();
                    }
                    catch
                    {
                        host = "Unknown - " + GetPersistentComputerInfo().Guid;
                    }
                }
            }
            return host;
        }

        private string GetLocalIPAddress()
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

        private IPAddress[] GetAllKnownIPAddresses()
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

        private PhysicalAddress[] GetAllKnownHWAddresses()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    .Select(nic => nic.GetPhysicalAddress())
                    .ToArray();
            }
            catch (Exception)
            {
                return new PhysicalAddress[0];
            }
        }
    }
}
