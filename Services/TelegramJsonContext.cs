using System.Text.Json.Serialization;

namespace Telegram.Bot.Services
{
    /// <summary>
    /// JSON Source Generator Context para Telegram API
    /// Elimina reflexão em tempo de execução, reduzindo CPU em ~30-50% durante parse
    /// Crítico para ARM32 com L1 Cache de apenas 32KB
    /// </summary>
    [JsonSourceGenerationOptions(
        PropertyNameCaseInsensitive = false,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    )]
    [JsonSerializable(typeof(TelegramBotService.TelegramUpdatesResponse))]
    [JsonSerializable(typeof(List<TelegramBotService.TelegramUpdate>))]
    [JsonSerializable(typeof(TelegramBotService.TelegramUpdate))]
    [JsonSerializable(typeof(TelegramBotService.TelegramMessage))]
    [JsonSerializable(typeof(TelegramBotService.TelegramUser))]
    [JsonSerializable(typeof(TelegramBotService.TelegramChat))]
    internal partial class TelegramJsonContext : JsonSerializerContext { }
}
