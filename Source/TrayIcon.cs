using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Forms;
using Common;

namespace SQLServerSwitch
{
    public partial class TrayIcon : TrayIconBase
    {
        private const string SERVICE_NAME = "MSSQL$SQLEXPRESS";


        public TrayIcon() : base ("SQL Server Switch")
        {
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
                Process.Start(new ProcessStartInfo()
                {
                    FileName = @"cmd",
                    Arguments = $"/k net {(isRunning ? "stop" : "start")} {SERVICE_NAME} & exit",
                    CreateNoWindow = true,
                    Verb = "runas",

                }).WaitForExit();
            }
            catch { }

            this.updateLook();
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
