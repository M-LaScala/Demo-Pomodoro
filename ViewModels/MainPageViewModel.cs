using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Demo_Pomodoro.Models;
using Demo_Pomodoro.Services;
using Demo_Pomodoro.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Timers;

namespace Demo_Pomodoro.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial int StudyTimer { get; set; }
        [ObservableProperty]
        public partial int RestTimer { get; set; }
        [ObservableProperty]
        public partial int CyclesCounter { get; set; }
        [ObservableProperty]
        public partial string ThemeColor { get; set; }
        [ObservableProperty]
        public partial string AlarmSelected { get; set; }
        [ObservableProperty]
        public partial int CompletedCycles { get; set; } = 0;
        [ObservableProperty]
        public partial bool IsStudyPhase { get; set; } = true;
        [ObservableProperty]
        public partial ObservableCollection<EllipseItem> EllipseItems { get; set; }
        [ObservableProperty]
        public partial string TimerDisplay { get; set; } = "25:00";
        [ObservableProperty]
        public partial bool IsPlayButtonVisible { get; set; } = true;
        [ObservableProperty]
        public partial bool IsPauseButtonVisible { get; set; } = false;
        [ObservableProperty]
        public partial bool IsCloseButtonVisible { get; set; } = false;
        [ObservableProperty]
        public partial bool IsRestartButtonVisible { get; set; } = false;

        private TimeSpan _labelTimer;
        private System.Timers.Timer? _timer;
        private bool _isFirstLoad = true;
        private readonly IAudioService _audioService;

        public MainPageViewModel(IAudioService audioService)
        {
            _audioService = audioService;

            LoadingPreferences();
            InitializeTimer();
            EllipseItems = Auxiliar.GenerateEllipseItems(CyclesCounter);
        }

        private void InitializeTimer()
        {
            _labelTimer = TimeSpan.FromMinutes(StudyTimer);
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            int totalMinutes = (int)_labelTimer.TotalMinutes;
            int seconds = _labelTimer.Seconds;
            TimerDisplay = $"{totalMinutes:D2}:{seconds:D2}";
        }

        [RelayCommand]
        private async Task NavigateToSettingsAsync()
        {
            if (!Auxiliar.IsInteracting)
            {
                Auxiliar.IsInteracting = true;
                await Shell.Current.GoToAsync("SettingsPage");
            }
        }

        [RelayCommand]
        private void Play()
        {
            if (!Auxiliar.IsInteracting)
            {
                Auxiliar.IsInteracting = true;
                SetButtonVisibility(false, true, true, true);
                _timer = new System.Timers.Timer(1000);
                _timer.Elapsed += OnTimedEvent;
                _timer.Start();
            }
        }

        [RelayCommand]
        private void Pause()
        {
            SetButtonVisibility(true, false, true, true);
            StopTimer();
            Auxiliar.IsInteracting = false;
        }

        [RelayCommand]
        private void Restart()
        {
            SetButtonVisibility(true, false, true, true);
            StopTimer();

            if (IsStudyPhase)
            {
                _labelTimer = TimeSpan.FromMinutes(StudyTimer);
            }
            else
            {
                _labelTimer = TimeSpan.FromMinutes(RestTimer);
            }
            UpdateTimerDisplay();

            Auxiliar.IsInteracting = false;
        }

        [RelayCommand]
        private void Close()
        {
            SetButtonVisibility(true, false, false, false);
            StopTimer();
            IsStudyPhase = true;
            _labelTimer = TimeSpan.FromMinutes(StudyTimer);
            UpdateTimerDisplay();
            ResetCycles();
            EllipseItems = Auxiliar.GenerateEllipseItems(CyclesCounter);
            Auxiliar.IsInteracting = false;
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimedEvent;
                _timer.Dispose();
                _timer = null;
            }
        }

        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            if (_labelTimer.TotalSeconds > 0)
            {
                _labelTimer = _labelTimer.Add(TimeSpan.FromSeconds(-1));
                MainThread.BeginInvokeOnMainThread(UpdateTimerDisplay);
            }
            // Quando o timer chega a zero
            else
            {
                _timer?.Stop();
                Auxiliar.IsInteracting = false;

                // Remove a visibilidade dos botões durante a transição
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    SetButtonVisibility(false, false, false, false);
                });

                // Fim de todos os ciclos
                if (CompletedCycles == CyclesCounter)
                {
                    StopTimer();
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        // Primeiro reseta os ciclos
                        ResetCycles();
                        EllipseItems = Auxiliar.GenerateEllipseItems(CyclesCounter);

                        // Segundo toca o alarme e mostra o anúncio
                        await _audioService.PlayAlarmAsync();

                        // Teceiro reseta o timer
                        _labelTimer = TimeSpan.FromMinutes(StudyTimer);
                        UpdateTimerDisplay();

                        // Quarto ativa a visibilidade dos botões
                        SetButtonVisibility(true, false, false, false);
                    });
                }
                else
                {
                    // Fim da fase de estudo
                    if (IsStudyPhase)
                    {
                        IsStudyPhase = false;
                        _labelTimer = TimeSpan.FromMinutes(RestTimer);
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            // Primeiro completa o ciclo atual
                            CompleteCurrentCycle();

                            // Segundo toca o alarme e mostra o anúncio
                            await _audioService.PlayAlarmAsync();

                            // Terceiro atualiza o timer e os ciclos
                            UpdateTimerDisplay();

                            // Quarto ativa a visibilidade dos botões
                            SetButtonVisibility(true, false, false, false);
                        });
                    }
                    // Fim da fase de descanso
                    else
                    {
                        IsStudyPhase = true;
                        _labelTimer = TimeSpan.FromMinutes(StudyTimer);
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            // Primeiro toca o alarme e mostra o anúncio
                            await _audioService.PlayAlarmAsync();

                            // Segundo atualiza o timer
                            UpdateTimerDisplay();

                            // Terceiro ativa a visibilidade dos botões
                            SetButtonVisibility(true, false, false, false);
                        });
                    }
                }
            }
        }

        private void CompleteCurrentCycle()
        {
            if (CompletedCycles < CyclesCounter)
            {
                CompletedCycles++;
                Auxiliar.UpdateEllipseFill(EllipseItems, CompletedCycles);
            }
        }

        private void ResetCycles()
        {
            CompletedCycles = 0;
            Auxiliar.UpdateEllipseFill(EllipseItems, CompletedCycles);
        }

        private void SetButtonVisibility(bool play, bool pause, bool close, bool restart)
        {
            IsPlayButtonVisible = play;
            IsPauseButtonVisible = pause;
            IsCloseButtonVisible = close;
            IsRestartButtonVisible = restart;
        }

        public void OnAppearing()
        {
            Auxiliar.IsInteracting = false;

            if (_isFirstLoad)
            {
                _isFirstLoad = false;
                return;
            }

            int newStudyTimer = Preferences.Default.Get("study_timer", 25);
            if (newStudyTimer != StudyTimer)
            {
                StudyTimer = newStudyTimer;
                _labelTimer = TimeSpan.FromMinutes(StudyTimer);
                UpdateTimerDisplay();
            }

            int newRestTimer = Preferences.Default.Get("rest_timer", 5);
            if (newRestTimer != RestTimer)
            {
                RestTimer = newRestTimer;
            }

            int newCycleCount = Preferences.Default.Get("cycles_counter", 4);
            if (newCycleCount != CyclesCounter)
            {
                CyclesCounter = newCycleCount;
                ResetCycles();
                EllipseItems = Auxiliar.GenerateEllipseItems(CyclesCounter);
            }
        }

        private void LoadingPreferences()
        {
            try
            {
                StudyTimer = Preferences.Default.Get("study_timer", 25);
                RestTimer = Preferences.Default.Get("rest_timer", 5);
                CyclesCounter = Preferences.Default.Get("cycles_counter", 4);
                ThemeColor = Preferences.Default.Get("theme_color", "#84DB87");
                AlarmSelected = Preferences.Default.Get("alarm_selected", "Alarm1");
            }
            catch
            {
                Preferences.Default.Clear();

                StudyTimer = 25;
                RestTimer = 5;
                CyclesCounter = 4;
                ThemeColor = "#84DB87";
                AlarmSelected = "Alarm1";

                Preferences.Default.Set("study_timer", StudyTimer);
                Preferences.Default.Set("rest_timer", RestTimer);
                Preferences.Default.Set("cycles_counter", CyclesCounter);
                Preferences.Default.Set("theme_color", ThemeColor);
                Preferences.Default.Set("alarm_selected", AlarmSelected);
            }
        }
    }
}
