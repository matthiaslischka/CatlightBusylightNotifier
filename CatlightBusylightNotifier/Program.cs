using System;
using System.Windows.Forms;
using CatlightApi;
using CatlightBusylightNotifier.Properties;
using Plenom.Components.Busylight.Sdk;

namespace CatlightBusylightNotifier
{
    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyCustomApplicationContext());
        }

        private class MyCustomApplicationContext : ApplicationContext
        {
            private readonly BusylightLyncController _busylightLyncController = new BusylightLyncController();

            private readonly Catlight _catlight = new Catlight();

            public MyCustomApplicationContext()
            {
                var trayMenu = new ContextMenu();
                trayMenu.MenuItems.Add("Exit", OnExit);

                var notifyIcon = new NotifyIcon
                {
                    Text = @"Catlight Busylight Notifier",
                    Icon = Resources._1481754117_traffic,
                    ContextMenu = trayMenu,
                    Visible = true
                };

                var updateStatusTimer = new Timer {Interval = 10000};
                updateStatusTimer.Tick += (sender, e) => UpdateStatus();
                UpdateStatus();
                updateStatusTimer.Start();
            }

            private void UpdateStatus()
            {
                var doesNeedAttention = _catlight.DoesNeedAttention();
                switch (doesNeedAttention)
                {
                    case true:
                        _busylightLyncController.Light(BusylightColor.Red);
                        break;
                    case false:
                        _busylightLyncController.Light(BusylightColor.Green);
                        break;
                    case null:
                        _busylightLyncController.Light(BusylightColor.Off);
                        break;
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