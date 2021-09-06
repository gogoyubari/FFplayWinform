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
            ffplay1.StartInfo.Arguments = $@"-fflags nobuffer -analyzeduration 500000 -i udp://{adress}:{port}?timeout=3000000 -top 2000 -left 0 -x 720 -noborder";
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
            ffplay2.StartInfo.Arguments = $@"-fflags nobuffer -analyzeduration 500000 -f lavfi -i amovie=udp\\://{adress}\\:{port}?timeout=3000000,showvolume=f=0:w=720:h=10:dm=1:p=1 -top 2000 -left 0 -noborder";
            ffplay2.StartInfo.CreateNoWindow = true;
            ffplay2.StartInfo.UseShellExecute = false;
            ffplay2.Start();

            Thread.Sleep(3000);
            
            SetParent(ffplay1.MainWindowHandle, panel1.Handle);
            MoveWindow(ffplay1.MainWindowHandle, 0, 0, 720, 405, true);

            SetParent(ffplay2.MainWindowHandle, panel2.Handle);
            MoveWindow(ffplay2.MainWindowHandle, 0, 0, 720, 20, true);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Location = Properties.Settings.Default.Form1Location;
            Text = $"udp://{ConfigurationManager.AppSettings["adress"]}:{ConfigurationManager.AppSettings["port"]}";

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
    }
}
