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
                    // HttpClientHandler é mais estável em Android
                    // Keep-Alive é automático em HttpClient (.NET 10)
                    var handler = new HttpClientHandler
                    {
                        MaxConnectionsPerServer = 4,  // ✅ Permite pool de 4 conexões
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
                client.Timeout = TimeSpan.FromSeconds(120);
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
