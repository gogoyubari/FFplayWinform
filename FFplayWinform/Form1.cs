using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Configuration;

namespace FFplayWinform
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
            DoubleBuffered = true;
        }

        public Process ffplay1 = new Process();
        public Process ffplay2 = new Process();
        private void FFplay()
        {
            string adress = ConfigurationManager.AppSettings["adress"];
            string port = ConfigurationManager.AppSettings["port"];

            ffplay1.StartInfo.FileName = @"ffplay.exe";
            ffplay1.StartInfo.Arguments = $@"-fflags nobuffer -probesize 32 -analyzeduration 0 -sync ext -i udp://{adress}:{port}?timeout=3000000 -top 2000 -left 0 -x 720 -noborder";
            ffplay1.StartInfo.CreateNoWindow = true;
            ffplay1.StartInfo.UseShellExecute = false;
            ffplay1.Start();

            ffplay2.StartInfo.FileName = @"ffplay.exe";
            // showvolume params:
            //  'f' Set fade, allowed range is [0, 1].
            //  'w' Set channel width, allowed range is [80, 8192].
            //  'h' Set channel height, allowed range is [1, 900].
            //  'dm' In second. If set to > 0., display a line for the max level in the previous seconds.
            //  'p' Set background opacity, allowed range is [0, 1].
            ffplay2.StartInfo.Arguments = $@"-fflags nobuffer -probesize 32 -analyzeduration 0 -sync ext -f lavfi -i amovie=udp\\://{adress}\\:{port}?timeout=3000000,showvolume=f=0:w=720:h=10:dm=1:p=1 -top 2000 -left 0 -noborder";
            ffplay2.StartInfo.CreateNoWindow = true;
            ffplay2.StartInfo.UseShellExecute = false;
            ffplay2.Start();

            while ((ffplay1.MainWindowHandle == IntPtr.Zero && ffplay1.HasExited == false) ||
                (ffplay2.MainWindowHandle == IntPtr.Zero && ffplay2.HasExited == false))
            {
                Thread.Sleep(100);
                ffplay1.Refresh();
                ffplay2.Refresh();
            }

            SetParent(ffplay1.MainWindowHandle, panel1.Handle);
            MoveWindow(ffplay1.MainWindowHandle, 0, 0, 720, 405, true);

            SetParent(ffplay2.MainWindowHandle, panel2.Handle);
            MoveWindow(ffplay2.MainWindowHandle, 0, 0, 720, 20, true);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.Form1Location;
            //Text = $"udp://{ConfigurationManager.AppSettings["adress"]}:{ConfigurationManager.AppSettings["port"]}";

            FFplay();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Form1Location = Location;
            }
            else
            {
                Properties.Settings.Default.Form1Location = RestoreBounds.Location;
            }
            Properties.Settings.Default.Save();

            try
            {
                ffplay1.CloseMainWindow();
                ffplay2.CloseMainWindow();
                ffplay1.WaitForExit(5000);
                ffplay2.WaitForExit(5000);
            }
            catch { }
        }

        private const int SnapDist = 50;
        private bool DoSnap(int pos, int edge)
        {
            int delta = Math.Abs(pos - edge);
            return delta > 0 && delta <= SnapDist;
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Screen scn = Screen.FromPoint(Location);
            if (DoSnap(Left, scn.WorkingArea.Left)) Left = scn.WorkingArea.Left - 10;
            if (DoSnap(Top, scn.WorkingArea.Top)) Top = scn.WorkingArea.Top;
            if (DoSnap(scn.WorkingArea.Right, Right)) Left = scn.WorkingArea.Right + 10 - Width;
            if (DoSnap(scn.WorkingArea.Bottom, Bottom)) Top = scn.WorkingArea.Bottom + 10 - Height;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.ToString("mmss") == "0000")
            {
                try
                {
                    ffplay1.CloseMainWindow();
                    ffplay2.CloseMainWindow();
                    ffplay1.WaitForExit(5000);
                    ffplay2.WaitForExit(5000);
                }
                catch { }
                finally
                {
                    FFplay();
                }
            }
        }
    }
}

