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
            private MenuItem _autostartMenuItem;

            private BusylightColor _currentBusylightColor = BusylightColor.Off;

            private MenuItem _soundMenuItem;
            private MenuItem _volumeMenuItem;

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

                _soundMenuItem = new MenuItem("Sound On Broken Build");
                trayMenu.MenuItems.Add(_soundMenuItem);
                foreach (Enum soundClip in Enum.GetValues(typeof(BusylightSoundClip)))
                {
                    _soundMenuItem.MenuItems.Add(soundClip.ToString(), SoundChanged);
                    UpdateSelectedSoundMenuItemMenu();
                }

                _volumeMenuItem = new MenuItem("Volume");
                trayMenu.MenuItems.Add(_volumeMenuItem);
                foreach (Enum volume in Enum.GetValues(typeof(BusylightVolume)))
                {
                    _volumeMenuItem.MenuItems.Add(volume.ToString(), VolumeChanged);
                    UpdateSelectedVolumeMenuItem();
                }

                _autostartMenuItem = new MenuItem("Run At System Startup", OnAutostartToggle);
                trayMenu.MenuItems.Add(_autostartMenuItem);
                UpdateSelectedAutostartMenutem();

                trayMenu.MenuItems.Add("Exit", OnExit);

                var notifyIcon = new NotifyIcon
                {
                    Text = @"Catlight Busylight Notifier",
                    Icon = Resources._1481754117_traffic,
                    ContextMenu = trayMenu,
                    Visible = true
                };
            }

            private void SoundChanged(object sender, EventArgs e)
            {
                Enum.TryParse(((MenuItem) sender).Text, out BusylightSoundClip soundClip);
                Settings.Default.Sound = soundClip;
                Settings.Default.Save();
                PlayAlarm();
                UpdateSelectedSoundMenuItemMenu();
            }

            private void UpdateSelectedSoundMenuItemMenu()
            {
                foreach (MenuItem menuItem in _soundMenuItem.MenuItems)
                {
                    var isCurrentSound = menuItem.Text == Settings.Default.Sound.ToString();
                    menuItem.Checked = isCurrentSound;
                }
            }

            private void VolumeChanged(object sender, EventArgs e)
            {
                Enum.TryParse(((MenuItem) sender).Text, out BusylightVolume volume);
                Settings.Default.Volume = volume;
                Settings.Default.Save();
                PlayAlarm();
                UpdateSelectedVolumeMenuItem();
            }

            private void UpdateSelectedVolumeMenuItem()
            {
                foreach (MenuItem menuItem in _volumeMenuItem.MenuItems)
                {
                    var isCurrentVolume = menuItem.Text == Settings.Default.Volume.ToString();
                    menuItem.Checked = isCurrentVolume;
                }
            }

            private void OnAutostartToggle(object sender, EventArgs e)
            {
                var isAutostartEnabled = _autostartManager.IsAutostartEnabled();

                if (isAutostartEnabled)
                    _autostartManager.DisableAutostart();
                else
                    _autostartManager.EnableAutostart();

                UpdateSelectedAutostartMenutem();
            }

            private void UpdateSelectedAutostartMenutem()
            {
                _autostartMenuItem.Checked = _autostartManager.IsAutostartEnabled();
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

            private void OnExit(object sender, EventArgs e)
            {
                _busylightLyncController.Light(BusylightColor.Off);
                Application.Exit();
            }
        }
    }
}