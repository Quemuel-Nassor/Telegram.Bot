using Microsoft.Extensions.Logging;
using Telegram.Bot.Services;
using Telegram.Bot.ViewModels;

namespace Telegram.Bot
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
                });

            // Configure Services
            builder.Services.AddHttpClient("TelegramClient", client =>
            {
                client.BaseAddress = new Uri("https://api.telegram.org");
                client.Timeout = TimeSpan.FromMinutes(5); // ARM32 é lento, precisa timeout maior
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(6)); // Pool de conexões

            // Background Worker para tarefas pesadas fora da UI thread
            builder.Services.AddSingleton<IBackgroundWorker>(sp =>
            {
                var worker = new BackgroundWorkerService();
                worker.Start();
                return worker;
            });

            builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
            builder.Services.AddScoped<GroupMessagesViewModel>();
            builder.Services.AddScoped<ConfigTokenViewModel>();
            builder.Services.AddScoped<MainPage>();
            builder.Services.AddScoped<ConfigPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
