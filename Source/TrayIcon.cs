using System.Diagnostics;
using System.Drawing;
using System.ServiceProcess;
using System.Windows.Forms;
using TrayToolkit.Helpers;
using TrayToolkit.UI;

namespace SQLServerSwitch
{
    public partial class TrayIcon : TrayIconBase
    {
        private const string SERVICE_NAME = "MSSQL$SQLEXPRESS";
        private readonly Bitmap toolTipIcon;


        public TrayIcon() : base ("SQL Server Switch")
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


        private void switchSqlServer(bool start)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = @"cmd",
                Arguments = $"/k net {(start ? "start" : "stop")} {SERVICE_NAME} & exit",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",

            }).WaitForExit();
        }


        private ServiceControllerStatus getSqlStatus()
        {
            return new ServiceController(SERVICE_NAME)?.Status ?? ServiceControllerStatus.Stopped;
        }


        private bool isSqlRunning()
        {
            return this.getSqlStatus() == ServiceControllerStatus.Running;

        }
    }
}
