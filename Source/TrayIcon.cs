using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SQLServerSwitch
{
    public partial class TrayIcon : Form
    {
        public TrayIcon()
        {
            this.Text = "SQL Server Switch";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Visible = false;
            this.ShowInTaskbar = false;
            this.trayIcon = this.createTrayIcon();
        }


        /// <summary>
        /// Creates a tray icon
        /// </summary>
        private NotifyIcon createTrayIcon()
        {
            var trayIcon = new NotifyIcon()
            {
                Text = "Keyboard Locker",
                Icon = this.getIcon(),
                ContextMenu = this.createContextMenu(),
                Visible = true
            };

            trayIcon.MouseUp += onTrayIconClick;
            return trayIcon;
        }


        /// <summary>
        /// Updates the look
        /// </summary>
        private void updateLook()
        {
            this.trayIcon.Icon = this.getIcon();
        }


        /// <summary>
        /// Returns the icon from the resource
        /// </summary>
        private Icon getIcon()
        {
            var lightMode = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true)?.GetValue("SystemUsesLightTheme") as int? == 1;
            var iconName = $"{this.GetType().Namespace}.Icon{(locked ? "Locked" : "Unlocked")}{(lightMode ? "Light" : "Dark")}.png";

            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconName))
            using (var bmp = new Bitmap(s))
                return Icon.FromHandle(bmp.GetHicon());
        }




        /// <summary>
        /// Creates the context menu
        /// </summary>
        private ContextMenu createContextMenu()
        {
            var trayMenu = new ContextMenu();

            trayMenu.MenuItems.Add("Start with Windows", this.onStartUp).Checked = this.startsWithWindows();
            trayMenu.MenuItems.Add("Exit", this.onMenuExit);

            return trayMenu;
        }

        async private void onDisplaySettingsChanged(object sender, EventArgs e)
        {
            await Task.Delay(500);
            this.updateLook();

            await Task.Delay(1000);
            this.updateLook();
        }


        /// <summary>
        /// Setting the startup state
        /// </summary>
        private bool startsWithWindows()
        {
            try
            {
                return Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValue(this.Text) as string == Application.ExecutablePath.ToString();
            }
            catch { return false; }
        }


        /// <summary>
        /// Setting the startup state
        /// </summary>
        private bool setStartup(bool set)
        {
            try
            {
                var rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (set)
                    rk.SetValue(this.Text, Application.ExecutablePath.ToString());
                else
                    rk.DeleteValue(this.Text, false);

                return set;
            }
            catch { return !set; }
        }
    }
}
