using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using JamCast.Models;

namespace JamCast.Services
{
    public class ClientRole : IClientRole
    {
        private readonly IJamHostApiService _jamHostApiService;

        private DateTime _nextUpdate = DateTime.MinValue;
        private StreamInfo _streamInfo;

        private Process _ffmpegProcess;
        private Thread _screenshotCaptureThread;

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

            if (_streamInfo != null)
            {
                _streamInfo.ShouldStream = true;

                if (_streamInfo?.ShouldStream ?? false)
                {
                    Status = "Streaming";

                    if (_streamInfo.OperationMode == OperationMode.Rtmp)
                    {
                        if (_ffmpegProcess == null || _ffmpegProcess.HasExited)
                        {
                            var killInfo = new ProcessStartInfo(@"C:\Windows\System32\taskkill.exe", "/f /im ffmpeg.exe");
                            killInfo.CreateNoWindow = true;
                            var kill = Process.Start(killInfo);
                            kill.WaitForExit();

                            _ffmpegProcess = new Process();
                            _ffmpegProcess.StartInfo.FileName = "Content\\ffmpeg.exe";
                            _ffmpegProcess.StartInfo.Arguments =
                                "-loglevel verbose -f gdigrab -i desktop -framerate 10 -vf scale=1280:720 -b:v 10k -f flv " +
                                _streamInfo?.RtmpsUrl;
                            _ffmpegProcess.StartInfo.CreateNoWindow = true;
                            _ffmpegProcess.StartInfo.UseShellExecute = false;
                            _ffmpegProcess.Start();
                        }
                    }
                    else
                    {
                        if (_screenshotCaptureThread == null || !_screenshotCaptureThread.IsAlive)
                        {
                            _screenshotCaptureThread = new Thread(CaptureScreenshotsAndUpload);
                            _screenshotCaptureThread.IsBackground = true;
                            _screenshotCaptureThread.Start();
                        }
                    }
                }
                else
                {
                    Status = "Not Streaming";

                    if (_streamInfo.OperationMode == OperationMode.Rtmp)
                    {
                        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
                        {
                            _ffmpegProcess.Kill();
                        }
                    }
                }
            }
        }

        private void CaptureScreenshotsAndUpload()
        {
            while (_streamInfo?.ShouldStream ?? false)
            {
                var image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                var graphics = Graphics.FromImage(image);
                graphics.CopyFromScreen(0, 0, 0, 0, image.Size);

                using (var memory = new MemoryStream())
                {
                    var encoder = ImageCodecInfo.GetImageEncoders().First(x => x.FormatID == ImageFormat.Jpeg.Guid);
                    var encoderParameter = new EncoderParameter(Encoder.Quality, 50L);
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = encoderParameter;

                    image.Save(memory, encoder, encoderParameters);
                    var len = memory.Position;
                    memory.Seek(0, SeekOrigin.Begin);

                    _jamHostApiService.UploadClientScreenshot(memory);
                }

                Thread.Sleep(500);
            }
        }

        public void End()
        {
            
        }

        public string Status { get; set; }
    }
}