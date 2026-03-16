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

            // Configure Services - HttpClient otimizado para ARM32 com 2GB RAM
            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        MaxConnectionsPerServer = 2,  // ✅ CORRIGIDO: padrão .NET
                        AllowAutoRedirect = false,
                        UseProxy = false,
                        AutomaticDecompression = System.Net.DecompressionMethods.None
                    };
                    return handler;
                });
            });

            builder.Services.AddHttpClient("TelegramClient", client =>
            {
                client.BaseAddress = new Uri("https://api.telegram.org");
                client.Timeout = TimeSpan.FromSeconds(30); // ✅ CORRIGIDO: 30s para mobile
                client.DefaultRequestHeaders.Add("User-Agent", "Telegram.Bot/1.0");
            });

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
