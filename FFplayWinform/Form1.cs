using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Configuration;
using System.IO;

namespace FFplayWinform
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private static readonly string ChildApp = Path.Combine(Directory.GetCurrentDirectory(), "ffplay.exe");
        private string FFplayTitle = "";
        private Process pFFplay = null;

        public Form1()
        {
            InitializeComponent();
            Application.EnableVisualStyles();
            DoubleBuffered = true;

            foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ChildApp))) { p.Kill(); }
        }

        private void StartFFplay()
        {
            string adress = ConfigurationManager.AppSettings["adress"];
            string port = ConfigurationManager.AppSettings["port"];
            string log_level = "-loglevel warning";
            string reduce_the_delay = "-fflags nobuffer -probesize 4M -analyzeduration 0";
            int width = 720;
            int height = 405;
            int height_s = 10;
            string color = "'if(lte(VOLUME,-1),if(lte(VOLUME,-2),0xff00ff00,0xff00ffff),0xff0000ff)'"; // 緑 < -2dB < 黄 < -1dB < 赤
            string udp = $@"udp\\://{adress}\\:{port}";
            string resize = $"scale={width}:-1,crop=w={width}:h={height}:x=0:y=0";
            string showvolume = $"showvolume=w={width}:h={height_s}:f=0:c={color}:v=0:dm=.5:s=0:m=p:ds=log";
            string filter = $"movie={udp}:s=dv+da[v][a];[v]{resize}[v];[a]asplit[a][out1];[a]{showvolume}[s];[v][s]vstack";
            FFplayTitle = Guid.NewGuid().ToString();

            pFFplay = new Process();
            pFFplay.StartInfo.FileName = ChildApp;
            pFFplay.StartInfo.Arguments = $@"{log_level} {reduce_the_delay} -f lavfi -i {filter}  -window_title {FFplayTitle} -top 1080 -left 0 -noborder";
            pFFplay.StartInfo.CreateNoWindow = true;
            pFFplay.StartInfo.UseShellExecute = false;
            pFFplay.Start();

            int timeout = 0;
            while (timeout < 5000)
            {
                foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ChildApp)))
                {
                    if (p.MainWindowTitle == FFplayTitle)
                    {
                        SetParent(p.MainWindowHandle, panel1.Handle);
                        MoveWindow(p.MainWindowHandle, 0, 0, width, height + 2 * height_s, true);
                        return;
                    }
                }
                Thread.Sleep(100);
                timeout += 100;
            }
        }
        private void StopFFplay()
        {
            foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ChildApp)))
            {
                if (p.MainWindowTitle == FFplayTitle)
                {
                    SetForegroundWindow(p.MainWindowHandle);
                    SendKeys.Send("{ESC}");
                    break;
                }
            }

            if (pFFplay != null)
            {
                pFFplay.Kill();
                pFFplay.WaitForExit();
                pFFplay.Close();
                pFFplay.Dispose();
                pFFplay = null;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.Form1Location;
            StartFFplay();
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

            StopFFplay();
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
                StopFFplay();
                StartFFplay();
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDBLCLK = 0x00A3;

            if (m.Msg == WM_NCLBUTTONDBLCLK)
            {
                StopFFplay();
                StartFFplay();
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}

