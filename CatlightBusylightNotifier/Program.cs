using System;
using System.Windows.Forms;
using CatlightApi;
using CatlightBusylightNotifier.Properties;
using Plenom.Components.Busylight.Sdk;
using Sunnerberg.Autostarter;

namespace CatlightBusylightNotifier
{
    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.Run(new MyCustomApplicationContext());
        }

        private class MyCustomApplicationContext : ApplicationContext
        {
            private readonly Autostarter _autostarter = new Autostarter(Application.ExecutablePath);
            private readonly BusylightLyncController _busylightLyncController = new BusylightLyncController();
            private readonly CatlightConnector _catlightConnector = new CatlightConnector();
            private MenuItem _autostartMenuItem;

            public MyCustomApplicationContext()
            {
                CreateContextMenu();

                var updateStatusTimer = new Timer {Interval = 10000};
                updateStatusTimer.Tick += (sender, e) => UpdateStatus();
                UpdateStatus();
                updateStatusTimer.Start();
            }

            private void CreateContextMenu()
            {
                var trayMenu = new ContextMenu();
                _autostartMenuItem = new MenuItem(string.Empty, OnAutostartToggle);
                trayMenu.MenuItems.Add(_autostartMenuItem);
                trayMenu.MenuItems.Add("Exit", OnExit);
                UpdateAutostartMenuItemText();

                var notifyIcon = new NotifyIcon
                {
                    Text = @"Catlight Busylight Notifier",
                    Icon = Resources._1481754117_traffic,
                    ContextMenu = trayMenu,
                    Visible = true
                };
            }

            private void OnAutostartToggle(object sender, EventArgs e)
            {
                var isAutostartEnabled = _autostarter.IsEnabled();

                if (isAutostartEnabled)
                    _autostarter.Disable();
                else
                    _autostarter.Enable();

                UpdateAutostartMenuItemText();
            }

            private void UpdateAutostartMenuItemText()
            {
                _autostartMenuItem.Text = _autostarter.IsEnabled()
                    ? "Don't Run At System Startup"
                    : "Run At System Startup";
            }

            private void UpdateStatus()
            {
                var catlightStatus = _catlightConnector.GetStatus();
                switch (catlightStatus)
                {
                    case CatlightStatus.CatlightNotFound:
                        _busylightLyncController.Light(BusylightColor.Blue);
                        break;
                    case CatlightStatus.NoProjects:
                        _busylightLyncController.Light(BusylightColor.Off);
                        break;
                    case CatlightStatus.Ok:
                    case CatlightStatus.Info:
                        _busylightLyncController.Light(BusylightColor.Green);
                        break;
                    case CatlightStatus.WarningAcknowledged:
                    case CatlightStatus.CriticalAcknowledged:
                        _busylightLyncController.Light(BusylightColor.Yellow);
                        break;
                    case CatlightStatus.Warning:
                    case CatlightStatus.Critical:
                        _busylightLyncController.Light(BusylightColor.Red);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void OnExit(object sender, EventArgs e)
            {
                _busylightLyncController.Light(BusylightColor.Off);
                Application.Exit();
            }
        }
    }
}