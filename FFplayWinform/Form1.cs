using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using Microsoft.Win32;
using System.Windows.Forms.VisualStyles;

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
            this.DoubleBuffered = true;
        }

        public Process ffplay1 = new Process();
        public Process ffplay2 = new Process();
        private void FFplay()
        {
            ffplay1.StartInfo.FileName = @"ffplay.exe";
            ffplay1.StartInfo.Arguments = $@"-fflags nobuffer -analyzeduration 500000 -i udp://224.1.1.1:10001?timeout=3000000 -top 2000 -left 0 -x 720 -noborder";
            ffplay1.StartInfo.CreateNoWindow = true;
            ffplay1.StartInfo.UseShellExecute = false;
            ffplay1.Start();

            ffplay2.StartInfo.FileName = @"ffplay.exe";
            ffplay2.StartInfo.Arguments = $@"-fflags nobuffer -analyzeduration 500000 -f lavfi -i amovie=udp\\://224.1.1.1\\:10001?timeout=3000000,showvolume=f=0:w=720:h=10:dm=1:p=1 -top 2000 -left 0 -noborder";
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
            FFplay();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
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
