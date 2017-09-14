using System;
using System.Windows.Forms;
using AutostartManagement;
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
            private readonly AutostartManager _autostartManager = new AutostartManager(Application.ExecutablePath);
            private readonly BusylightLyncController _busylightLyncController = new BusylightLyncController();
            private readonly CatlightConnector _catlightConnector = new CatlightConnector();
            private MenuItem _alarmSoundMenuItem;
            private MenuItem _autostartMenuItem;
            private BusylightColor _currentBusylightColor = BusylightColor.Off;

            public MyCustomApplicationContext()
            {
                CreateContextMenu();

                var updateStatusTimer = new Timer {Interval = 5000};
                updateStatusTimer.Tick += (sender, e) => UpdateStatus();
                UpdateStatus();
                updateStatusTimer.Start();
            }

            private void CreateContextMenu()
            {
                var trayMenu = new ContextMenu();

                _autostartMenuItem = new MenuItem(string.Empty, OnAutostartToggle);
                trayMenu.MenuItems.Add(_autostartMenuItem);
                UpdateAutostartMenuItemText();

                _alarmSoundMenuItem = new MenuItem(string.Empty, OnAlarmSoundToggle);
                trayMenu.MenuItems.Add(_alarmSoundMenuItem);
                UpdateAlarmSoundMenuItemText();

                trayMenu.MenuItems.Add("Exit", OnExit);

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
                var isAutostartEnabled = _autostartManager.IsAutostartEnabled();

                if (isAutostartEnabled)
                    _autostartManager.DisableAutostart();
                else
                    _autostartManager.EnableAutostart();

                UpdateAutostartMenuItemText();
            }

            private void UpdateAutostartMenuItemText()
            {
                _autostartMenuItem.Text = _autostartManager.IsAutostartEnabled()
                    ? "Don't Run At System Startup"
                    : "Run At System Startup";
            }

            private void OnAlarmSoundToggle(object sender, EventArgs e)
            {
                var isAlarmSoundEnabled = Settings.Default.IsAlarmSoundEnabled;
                Settings.Default.IsAlarmSoundEnabled = !isAlarmSoundEnabled;
                Settings.Default.Save();
                UpdateAlarmSoundMenuItemText();
            }

            private void UpdateAlarmSoundMenuItemText()
            {
                _alarmSoundMenuItem.Text = Settings.Default.IsAlarmSoundEnabled
                    ? "Disable Alarmsound On Broken Build"
                    : "Enable Alarmsound On Broken Build";
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

                var playAlarmSound = Settings.Default.IsAlarmSoundEnabled && newBusylightColor == BusylightColor.Red && _currentBusylightColor != BusylightColor.Red;

                if (playAlarmSound)
                    _busylightLyncController.Alert(newBusylightColor, BusylightSoundClip.OpenOffice,
                        BusylightVolume.Middle);
                else
                    _busylightLyncController.Light(newBusylightColor);

                _currentBusylightColor = newBusylightColor;
            }

            private void OnExit(object sender, EventArgs e)
            {
                _busylightLyncController.Light(BusylightColor.Off);
                Application.Exit();
            }
        }
    }
}