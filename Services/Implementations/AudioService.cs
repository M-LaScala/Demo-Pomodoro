using Demo_Pomodoro.Services.Interfaces;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo_Pomodoro.Services.Implementations
{
    public class AudioService : IAudioService, IDisposable
    {
        private readonly IAudioManager _audioManager;
        private readonly List<IAudioPlayer> _activePlayers = [];
        private bool _disposed;

        public AudioService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public async Task PlayAlarmAsync()
        {
            var alarmSelected = Preferences.Default.Get("alarm_selected", "Alarm1");
            var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync($"{alarmSelected}.mp3"));

            _activePlayers.Add(player);

            player.PlaybackEnded += (s, e) =>
            {
                player.Dispose();
                _activePlayers.Remove(player);
            };

            player.Play();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var player in _activePlayers)
            {
                player.Dispose();
            }

            _activePlayers.Clear();
            _disposed = true;
        }
    }
}
