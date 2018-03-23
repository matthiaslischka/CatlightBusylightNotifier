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
            Application.Run(new MyCustomApplicationContext());
        }

        private class MyCustomApplicationContext : ApplicationContext
        {
            private readonly BusylightLyncController _busylightLyncController = new BusylightLyncController();
            private readonly CatlightConnector _catlightConnector = new CatlightConnector();

            private BusylightColor _currentBusylightColor = BusylightColor.Off;

            public MyCustomApplicationContext()
            {
                // ReSharper disable once ObjectCreationAsStatement
                new NotifyIcon
                {
                    Text = @"Catlight Busylight Notifier",
                    Icon = Resources._1481754117_traffic,
                    ContextMenu = new ContextMenu(PlayAlarm),
                    Visible = true
                };

                var updateStatusTimer = new Timer {Interval = 5000};
                updateStatusTimer.Tick += (sender, e) => UpdateStatus();
                UpdateStatus();
                updateStatusTimer.Start();
            }

            private void UpdateStatus()
            {
                var catlightStatus = _catlightConnector.GetStatus();
                BusylightColor newBusylightColor;

                switch (catlightStatus)
                {
                    case CatlightStatus.CatlightNotFound:
                        newBusylightColor = BusylightColor.Blue;
                        break;
                    case CatlightStatus.NoProjects:
                        newBusylightColor = BusylightColor.Off;
                        break;
                    case CatlightStatus.Ok:
                    case CatlightStatus.Info:
                        newBusylightColor = BusylightColor.Green;
                        break;
                    case CatlightStatus.WarningAcknowledged:
                    case CatlightStatus.CriticalAcknowledged:
                        newBusylightColor = BusylightColor.Yellow;
                        break;
                    case CatlightStatus.Warning:
                    case CatlightStatus.Critical:
                        newBusylightColor = BusylightColor.Red;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var playAlarm = newBusylightColor == BusylightColor.Red && _currentBusylightColor != BusylightColor.Red;

                if (playAlarm)
                    PlayAlarm();
                else
                    _busylightLyncController.Light(newBusylightColor);

                _currentBusylightColor = newBusylightColor;
            }

            private void PlayAlarm()
            {
                _busylightLyncController.Alert(_currentBusylightColor, Settings.Default.Sound,
                    Settings.Default.Volume);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _busylightLyncController.Light(BusylightColor.Off);
            }
        }
    }
}