using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using TrayToolkit.Helpers;
using TrayToolkit.UI;

namespace SQLServerSwitch
{
    public partial class TrayIcon : TrayIconBase
    {
        private readonly string[] SERVICE_NAMES = new string[] { "MSSQL$SQLEXPRESS", "MSSQLSERVER" };
        private readonly Bitmap toolTipIcon;


        public TrayIcon() : base ("SQL Server Switch", "https://github.com/poulicek/SQLServerSwitch")
        {
            this.toolTipIcon = ResourceHelper.GetResourceImage(this.getIconName(false));
        }

        protected override string getIconName(bool lightMode)
        {
            return $"Icon{(this.isSqlRunning() ? "On" : "Off")}{(lightMode ? "Light" : "Dark")}.png";
        }

        private void updateStatus()
        {
            var status = this.getSqlStatus().ToString().ToLower();
            if (status.Length > 1)
                status = char.ToUpper(status[0]) + status.Substring(1);

            this.trayIcon.Text = this.Text + " - " + status;
        }


        protected override void updateLook()
        {
            base.updateLook();
            this.updateStatus();
        }


        protected override void onTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var isRunning = this.isSqlRunning();

            try
            {
                BalloonTooltip.Show(isRunning ? "Stopping the SQL Server..." : "Starting the SQL Server...", this.toolTipIcon, () => this.switchSqlServer(!isRunning));
            }
            catch { }

            this.updateLook();
        }


        private string getServiceName()
        {
            foreach (var svc in ServiceController.GetServices())
                if (SERVICE_NAMES.Contains(svc.ServiceName))
                    return svc.ServiceName;

            return null;
        }


        private void switchSqlServer(bool start)
        {
            var svcName = this.getServiceName();
            if (string.IsNullOrEmpty(svcName))
                return;

            Process.Start(new ProcessStartInfo()
            {
                FileName = @"cmd",
                Arguments = $"/k net {(start ? "start" : "stop")} {svcName} & exit",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",

            }).WaitForExit();
        }

        private ServiceControllerStatus getSqlStatus()
        {
            try
            {
                var svcName = this.getServiceName();
                if (string.IsNullOrEmpty(svcName))
                    return ServiceControllerStatus.Stopped;

                return new ServiceController(svcName)?.Status ?? ServiceControllerStatus.Stopped;
            }
            catch (InvalidOperationException) { return ServiceControllerStatus.Stopped; }
        }


        private bool isSqlRunning()
        {
            return this.getSqlStatus() == ServiceControllerStatus.Running;

        }
    }
}
