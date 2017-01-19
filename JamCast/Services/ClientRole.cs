using System;
using System.Diagnostics;
using JamCast.Models;

namespace JamCast.Services
{
    public class ClientRole : IClientRole
    {
        private readonly IJamHostApiService _jamHostApiService;

        private DateTime _nextUpdate = DateTime.MinValue;
        private StreamInfo _streamInfo;

        private Process _ffmpegProcess;

        public ClientRole(IJamHostApiService jamHostApiService)
        {
            _jamHostApiService = jamHostApiService;

            Status = "Waiting...";
        }

        public void Update()
        {
            if (DateTime.UtcNow > _nextUpdate)
            {
                try
                {
                    _streamInfo = _jamHostApiService.ClientPing();
                }
                catch (System.Net.WebException)
                {
                    _streamInfo = null;
                }
                _nextUpdate = DateTime.UtcNow.AddMinutes(1);
            }

            if (_streamInfo?.ShouldStream ?? false)
            {
                Status = "Streaming";

                if (_ffmpegProcess == null || _ffmpegProcess.HasExited)
                {
                    var killInfo = new ProcessStartInfo(@"C:\Windows\System32\taskkill.exe", "/f /im ffmpeg.exe");
                    killInfo.CreateNoWindow = true;
                    var kill = Process.Start(killInfo);
                    kill.WaitForExit();

                    _ffmpegProcess = new Process();
                    _ffmpegProcess.StartInfo.FileName = "Content\\ffmpeg.exe";
                    _ffmpegProcess.StartInfo.Arguments = "-loglevel verbose -f gdigrab -i desktop -framerate 10 -vf scale=1280:720 -b:v 10k -f flv " + _streamInfo?.RtmpsUrl;
                    _ffmpegProcess.StartInfo.CreateNoWindow = false;
                    _ffmpegProcess.StartInfo.UseShellExecute = false;
                    _ffmpegProcess.Start();
                }
            }
            else
            {
                Status = "Not Streaming";

                if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                {
                    _ffmpegProcess.Kill();
                }
            }
        }

        public void End()
        {
            
        }

        public string Status { get; set; }
    }
}