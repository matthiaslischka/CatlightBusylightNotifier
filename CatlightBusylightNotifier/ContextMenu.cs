using System;
using System.Windows.Forms;
using AutostartManagement;
using Busylight;
using CatlightBusylightNotifier.Properties;

namespace CatlightBusylightNotifier
{
    public class ContextMenu : System.Windows.Forms.ContextMenu
    {
        private readonly AutostartManager _autostartManager = new AutostartManager(Application.ExecutablePath);
        private readonly MenuItem _autostartMenuItem;
        private readonly Action _playAlarm;

        private readonly MenuItem _soundMenuItem;
        private readonly MenuItem _volumeMenuItem;

        public ContextMenu(Action playAlarm)
        {
            _playAlarm = playAlarm;
            _soundMenuItem = new MenuItem("Sound On Broken Build");
            MenuItems.Add(_soundMenuItem);
            CreateSubmenuFromEnum<BusylightSoundClip>(_soundMenuItem, SoundChanged);
            UpdateSelectedSoundMenuItem();

            _volumeMenuItem = new MenuItem("Volume");
            MenuItems.Add(_volumeMenuItem);
            CreateSubmenuFromEnum<BusylightVolume>(_volumeMenuItem, VolumeChanged);
            UpdateSelectedVolumeMenuItem();

            _autostartMenuItem = new MenuItem("Run At System Startup", OnAutostartToggle);
            MenuItems.Add(_autostartMenuItem);
            UpdateSelectedAutostartMenutem();

            MenuItems.Add("Exit", OnExit);
        }

        private void CreateSubmenuFromEnum<TEnum>(MenuItem menuItem, EventHandler onClick) where TEnum : struct
        {
            foreach (Enum enumValue in Enum.GetValues(typeof(TEnum)))
                menuItem.MenuItems.Add(enumValue.ToString(), onClick);
        }

        private void SoundChanged(object sender, EventArgs e)
        {
            Enum.TryParse(((MenuItem)sender).Text, out BusylightSoundClip soundClip);
            Settings.Default.Sound = soundClip;
            Settings.Default.Save();
            _playAlarm();
            UpdateSelectedSoundMenuItem();
        }

        private void UpdateSelectedSoundMenuItem()
        {
            foreach (MenuItem menuItem in _soundMenuItem.MenuItems)
            {
                var isCurrentSound = menuItem.Text == Settings.Default.Sound.ToString();
                menuItem.Checked = isCurrentSound;
            }
        }

        private void VolumeChanged(object sender, EventArgs e)
        {
            Enum.TryParse(((MenuItem)sender).Text, out BusylightVolume volume);
            Settings.Default.Volume = volume;
            Settings.Default.Save();
            _playAlarm();
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

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}