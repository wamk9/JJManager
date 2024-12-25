using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager
{
    internal static class Program
    {
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static Mutex mutex = new Mutex(true, "{71fcb366-7349-40b3-835a-943c012e1283}");


        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        //[MTAThread]
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
            else
            {
                // Bring the existing instance to the foreground
                BringToForeground();
            }
        }

        private static void BringToForeground()
        {
            // Find the window handle of the existing instance
            IntPtr hWnd = FindWindow(null, "JJManager");

            // Bring the existing instance to the foreground
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, 1);
                SetForegroundWindow(hWnd);
            }
        }
    }
}
