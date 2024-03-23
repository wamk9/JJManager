using JJManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class
{
    internal class AppModules
    {
        public AppModules()
        {

        }
    }

    internal class AppModulesNotifyIcon
    {
        System.Windows.Forms.NotifyIcon notifyIcon;

        public AppModulesNotifyIcon(System.ComponentModel.IContainer components, String message, EventHandler clickEvent)
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon(components);

            notifyIcon.Visible = false;
            notifyIcon.Click += new EventHandler(clickEvent);
            notifyIcon.BalloonTipTitle = "JJManager";
            notifyIcon.BalloonTipText = message;
            notifyIcon.Icon = JJManager.Properties.Resources.JJManagerIcon_256;
        }

        public void Show()
        {
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(3000);
        }

        public void Hide()
        {
            notifyIcon.Visible = false;
        }
    }

    internal class AppModulesTimer
    {
        System.Windows.Forms.Timer timer;

        public AppModulesTimer(System.ComponentModel.IContainer components, int time, EventHandler tickEvent)
        {
            timer = new System.Windows.Forms.Timer(components);

            timer.Interval = time;
            timer.Enabled = true;           
            timer.Tick += new EventHandler(tickEvent);
            timer.Start();
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }
    }
}
