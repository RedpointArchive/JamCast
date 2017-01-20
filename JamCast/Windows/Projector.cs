using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace JamCast.Windows
{
    public partial class Projector : Form
    {
        private string _rtmpsUrl;

        private Process _ffplay;
        private string _ffplayRtmpsUrl;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int nWidth, int nHeight, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

        private Dictionary<IntPtr, bool> _parentTracker = new Dictionary<IntPtr, bool>();
        private List<IntPtr> _allKnownWindowHandles = new List<IntPtr>();

        private Random _random = new Random();
        

        public Projector()
        {
            InitializeComponent();
        }

        private void Projector_Load(object sender, EventArgs e)
        {
            // Set it up so that all drawing is done through our code.
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        public void SetRtmpsUrl(string streamInfoRtmpsUrl)
        {
            _rtmpsUrl = streamInfoRtmpsUrl;
        }

        private void _timer_Tick(object sender, EventArgs ee)
        {
            if (_ffplay == null || _ffplay.HasExited)
            {
                _ffplay = null;
            }

            if (_ffplayRtmpsUrl != _rtmpsUrl)
            {
                _ffplay?.Kill();
                _ffplay = null;
            }

            if (_ffplay == null)
            {
                _ffmpegStatusLabel.Text = "Launching FFPlay";
                _ffplayRtmpsUrl = _rtmpsUrl;

                var killInfo = new ProcessStartInfo(@"C:\Windows\System32\taskkill.exe", "/f /im ffplay.exe");
                killInfo.CreateNoWindow = true;
                var kill = Process.Start(killInfo);
                kill?.WaitForExit();
                killInfo = new ProcessStartInfo(@"C:\Windows\System32\taskkill.exe", "/f /im ffmpeg.exe");
                killInfo.CreateNoWindow = true;
                kill = Process.Start(killInfo);
                kill?.WaitForExit();

                _ffplay = new Process();
                _ffplay.StartInfo.FileName = "Content\\ffplay.exe";
                _ffplay.StartInfo.Arguments = "-loglevel verbose -stats -probesize 32 -sync ext " + _rtmpsUrl;
                _ffplay.StartInfo.CreateNoWindow = false;
                //_ffplay.StartInfo.RedirectStandardOutput = true;
                //_ffplay.StartInfo.RedirectStandardError = true;
                _ffplay.StartInfo.UseShellExecute = false;

                _ffplay.EnableRaisingEvents = true;
                //_ffplay.OutputDataReceived += FfplayOnOutputDataReceived;
                //_ffplay.ErrorDataReceived += FfplayOnOutputDataReceived;
                _ffplay.Exited += (o, e) => Debug.WriteLine("Exited", "ffplay");
                _ffplay.Start();
                //_ffplay.BeginOutputReadLine();
                //_ffplay.BeginErrorReadLine();

                _parentTracker.Clear();
                _allKnownWindowHandles.Clear();

                _ffmpegStatusLabel.Text = "Waiting for FFPlay (PID: " + _ffplay.Id + ")";

                return;
            }

            _allKnownWindowHandles.AddRange(GetRootWindowsOfProcess(_ffplay.Id));
            var children = _allKnownWindowHandles.SelectMany(GetChildWindowsRecursive).ToArray();

            var hasActualWindow = false;

            foreach (var windowHandle in children)
            {
                const int GWL_STYLE = (-16);
                const UInt32 WS_VISIBLE = 0x10000000;

                if (!_parentTracker.ContainsKey(windowHandle))
                {
                    SetWindowLong(windowHandle, GWL_STYLE, (WS_VISIBLE));
                    SetParent(windowHandle, _displayPanel.Handle);

                    _parentTracker.Add(windowHandle, true);
                }

                SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, _displayPanel.Width, _displayPanel.Height, 0x0001 | 0x0004 | 0x0200);
                SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, _displayPanel.Width, _displayPanel.Height, 0x0002 | 0x0004 | 0x0200);

                int capacity = GetWindowTextLength(new HandleRef(this, windowHandle)) * 2;
                StringBuilder stringBuilder = new StringBuilder(capacity);
                GetWindowText(new HandleRef(this, windowHandle), stringBuilder, stringBuilder.Capacity);

                if (stringBuilder.ToString() == _rtmpsUrl)
                {
                    // We have the actual window now, we will hide all other windows in the next step.
                    SetWindowPos(windowHandle, new IntPtr(1), _displayPanel.Left, _displayPanel.Top, _displayPanel.Width, _displayPanel.Height, 0x0001 | 0x0002 /* | 0x0004 | 0x0080*/);
                    hasActualWindow = true;
                }

                _ffmpegStatusLabel.Text = "Connected!";
            }

            if (hasActualWindow)
            {
                foreach (var windowHandle in children)
                {
                    int capacity = GetWindowTextLength(new HandleRef(this, windowHandle)) * 2;
                    StringBuilder stringBuilder = new StringBuilder(capacity);
                    GetWindowText(new HandleRef(this, windowHandle), stringBuilder, stringBuilder.Capacity);

                    if (stringBuilder.ToString() != _rtmpsUrl)
                    {
                        // Hide all other windows.
                        SetWindowPos(windowHandle, IntPtr.Zero, _displayPanel.Left, _displayPanel.Top, _displayPanel.Width, _displayPanel.Height, 0x0001 | 0x0002 | 0x0004 | 0x0080);
                    }
                }
            }
        }

        private int w = 20;

        private void FfplayOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            Console.WriteLine(dataReceivedEventArgs.Data ?? "NULL");
            if (!string.IsNullOrWhiteSpace(dataReceivedEventArgs.Data))
            {
                Invoke(new Action(() => {
                    _ffmpegStatusLabel.Text = dataReceivedEventArgs.Data;
                }));
            }
        }

        private void TerminateFFPlay()
        {
            if (_ffplay != null && !_ffplay.HasExited)
            {
                _ffplay.CloseMainWindow();
                _ffplay.Kill();
            }
        }
        
        private void Projector_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                TerminateFFPlay();
            }
            catch
            {
            }
        }

        List<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            List<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            List<IntPtr> dsProcRootWindows = new List<IntPtr>();
            foreach (IntPtr hWnd in rootWindows)
            {
                uint lpdwProcessId;
                GetWindowThreadProcessId(hWnd, out lpdwProcessId);
                if (lpdwProcessId == pid)
                {
                    dsProcRootWindows.Add(hWnd);
                }
            }
            return dsProcRootWindows;
        }

        private IEnumerable<IntPtr> GetChildWindowsRecursive(IntPtr hWnd)
        {
            yield return hWnd;
            foreach (var child in GetChildWindows(hWnd))
            foreach (var subchild in GetChildWindowsRecursive(child))
                yield return subchild;
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                Win32Callback childProc = new Win32Callback(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        private void Projector_Paint(object sender, PaintEventArgs e)
        {
            /*
            Graphics g;
            g.

            try
            {
                // Hide mouse cursor.
                Cursor.Hide();

                // Clear our window first.
                e.Graphics.Clear(Color.Black);

                // Get a center string style.
                StringFormat left = new StringFormat();
                left.Alignment = StringAlignment.Near;
                left.LineAlignment = StringAlignment.Center;
                StringFormat right = new StringFormat();
                right.Alignment = StringAlignment.Far;
                right.LineAlignment = StringAlignment.Center;
                StringFormat center = new StringFormat();
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;
                StringFormat tleft = new StringFormat();
                tleft.Alignment = StringAlignment.Near;
                tleft.LineAlignment = StringAlignment.Near;

                if (m_Manager._ffmpegProcessController != null &&
                    m_Manager._ffmpegProcessController.FfplayProcess != null
                        && !m_Manager._ffmpegProcessController.FfplayProcess.HasExited)
                {
                    FfmpegStreamAPI.AlignToFormBounds(
                        m_Manager._ffmpegProcessController.FfplayProcess,
                        this,
                        new Rectangle(
                            0,
                            64,
                            this.ClientSize.Width - (AppSettings.SlackEnabled ? 256 : 0),
                            this.ClientSize.Height - 128));
                }
                else if (m_Manager._ffmpegProcessController.WaitingOn != null)
                {
                    e.Graphics.DrawString(
                        "Waiting on " + m_Manager._ffmpegProcessController.WaitingOn + "'s computer to stream...",
                        new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.White),
                        new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                        center);
                }
                else
                {
                    e.Graphics.DrawString(
                        "Finding a computer to stream from...",
                        new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.White),
                        new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height),
                        center);
                }

                // Draw connection status.
                if (m_Manager._pubSubController != null)
                {
                    string connectionStatus = m_Manager._pubSubController.Status.ToString();
                    e.Graphics.DrawString(
                        connectionStatus,
                        new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular, GraphicsUnit.Pixel),
                        new SolidBrush(Color.Gray),
                        new Rectangle(this.ClientSize.Width / 2 - 64, 0, 128, 64),
                        center);
                }

                // Draw the COUNTDOWN! (top-right)
                TimeSpan span = new TimeSpan(AppSettings.EndTime.Ticks - DateTime.Now.Ticks);
                string ms = span.Milliseconds.ToString().PadLeft(4, '0').Substring(1, 3);
                string hrs = (span.Hours + (span.Days * 24)).ToString();
                string sstr = hrs + " hours " + span.Minutes + " minutes " + span.Seconds + "." + ms + " seconds ";
                if (span.Ticks <= 0) sstr = "GAME JAM OVER!";
                e.Graphics.DrawString(
                    sstr,
                    new Font(FontFamily.GenericSansSerif, 24, FontStyle.Regular, GraphicsUnit.Pixel),
                    new SolidBrush(Color.Red),
                    new Rectangle(0, 0, this.ClientSize.Width - 32, 64),
                    right);

                if (span.TotalHours < 2)
                {
                    string str = "";
                    if (span.Ticks < 0) str = "GAME JAM OVER!";
                    else str = hrs + " HOURS\n" + span.Minutes + " MINUTES\n" + span.Seconds + "." + ms + " SECS ";

                    if (span.TotalHours > 1 || Math.Floor((double)span.Milliseconds / 500) % 2 == 0 || span.Seconds < 0)
                    {
                        // Draw the COUNTDOWN! (center)
                        e.Graphics.DrawString(
                            str,
                            new Font(FontFamily.GenericSansSerif, 128, FontStyle.Regular, GraphicsUnit.Pixel),
                            new SolidBrush(Color.FromArgb(200 + this.m_Random.Next(32), 0, 0)),
                            new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                            center);
                    }
                    else
                    {
                        // Draw the COUNTDOWN! (center)
                        e.Graphics.DrawString(
                            str,
                            new Font(FontFamily.GenericSansSerif, 128, FontStyle.Regular, GraphicsUnit.Pixel),
                            new SolidBrush(Color.White),
                            new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                            center);
                    }

                    // Draw border.
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddString(
                        str,
                        FontFamily.GenericSansSerif,
                        (int)FontStyle.Regular,
                        128,
                        new PointF(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                        center);
                    e.Graphics.DrawPath(new Pen(Brushes.Black, 2), gp);
                }

                // Draw the bottom overlay.
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.Black),
                    0,
                    this.ClientSize.Height - 64,
                    this.ClientSize.Width,
                    64);

                if (AppSettings.TwitterEnabled)
                {
                    // Draw the TWEETS! ~.o
                    /* string st = this.m_Manager.GetTweetStream();
                SizeF size = e.Graphics.MeasureString(st,
                    new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold, GraphicsUnit.Pixel));

                if (this.m_StreamX < -size.Width + this.ClientSize.Width - 32)
                    this.m_StreamX = 0;
                else
                    this.m_StreamX -= 2;

                e.Graphics.DrawString(
                    (st + st).Replace("\n", "").Replace("\r", ""),
                    new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold, GraphicsUnit.Pixel),
                    new SolidBrush(Color.White),
                    //new Point(this.m_StreamX + 32, this.ClientSize.Height - 64),
                    new Rectangle(this.m_StreamX + 32, this.ClientSize.Height - 64,
                        (int) size.Width*9 + this.ClientSize.Width, 64),
                    left
                    );*
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }*/
        }
    }
}
