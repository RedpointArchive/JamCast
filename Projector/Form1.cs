using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JamcastProjector
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public Form1()
        {
            InitializeComponent();
            Application.EnableVisualStyles();
            this.DoubleBuffered = true;
        }

        public Process ffplay = new Process();
        private void xxxFFplay()
        {
            ffplay.StartInfo.FileName = "ffplay.exe";
            ffplay.StartInfo.Arguments = "test.mp4";
            ffplay.StartInfo.CreateNoWindow = true;
            ffplay.StartInfo.RedirectStandardOutput = true;
            ffplay.StartInfo.UseShellExecute = false;

            ffplay.EnableRaisingEvents = true;
            ffplay.OutputDataReceived += (o, e) => Debug.WriteLine(e.Data ?? "NULL", "ffplay");
            ffplay.ErrorDataReceived += (o, e) => Debug.WriteLine(e.Data ?? "NULL", "ffplay");
            ffplay.Exited += (o, e) => Debug.WriteLine("Exited", "ffplay");
            ffplay.Start();

            while (ffplay.MainWindowHandle.ToInt32() < 1)
            {
                Thread.Sleep(100);
            }

            SetParent(ffplay.MainWindowHandle, this.panel1.Handle);

            MoveWindow(ffplay.MainWindowHandle, -5, -30, this.panel1.Width + 5, this.panel1.Height + 30, true);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            xxxFFplay();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try {
                ffplay.CloseMainWindow();
                Thread.Sleep(100);
                ffplay.Kill(); }
            catch { }
        }
    }
}
