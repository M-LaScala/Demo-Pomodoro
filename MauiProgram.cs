using Demo_Pomodoro.Services.Implementations;
using Demo_Pomodoro.Services.Interfaces;
using Demo_Pomodoro.ViewModels;
using Plugin.Maui.Audio;
using Microsoft.Extensions.Logging;

namespace Demo_Pomodoro
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddTransient<IAudioService, AudioService>();
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
