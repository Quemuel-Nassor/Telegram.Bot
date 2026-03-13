using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Telegram.Bot.Services
{
    public class TelegramBotService : ITelegramBotService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TelegramBotService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<GroupMessage>> GetGroupUpdatesAsync(string apiToken, long? offset = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TelegramClient");
                
                // Endpoint: /bot{TOKEN}/getUpdates
                var url = $"bot{apiToken}/getUpdates";
                if (offset.HasValue)
                {
                    url += $"?offset={offset}";
                }

                var response = await client.GetFromJsonAsync<TelegramUpdatesResponse>(url, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (response?.Ok == true && response.Result != null && response.Result.Count > 0)
                {
                    return ParseUpdatesToMessages(response.Result);
                }

                return new List<GroupMessage>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar atualizações: {ex.Message}");
                return new List<GroupMessage>();
            }
        }

        private List<GroupMessage> ParseUpdatesToMessages(List<TelegramUpdate> updates)
        {
            var messages = new List<GroupMessage>();

            foreach (var update in updates)
            {
                if (update.Message == null)
                    continue;

                var msg = new GroupMessage
                {
                    UpdateId = update.UpdateId,
                    MessageId = update.Message.MessageId,
                    SenderName = update.Message.From?.FirstName,
                    SenderUsername = update.Message.From?.Username,
                    Text = update.Message.Text ?? "(mensagem sem texto)",
                    GroupName = update.Message.Chat?.Title ?? update.Message.Chat?.FirstName,
                    Date = UnixTimeStampToDateTime(update.Message.Date)
                };

                messages.Add(msg);
            }

            // Retornar em ordem reversa (mais recentes primeiro)
            return messages.OrderByDescending(m => m.Date).ToList();
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        #region Telegram API Models

        private class TelegramUpdatesResponse
        {
            [JsonPropertyName("ok")]
            public bool Ok { get; set; }

            [JsonPropertyName("result")]
            public List<TelegramUpdate>? Result { get; set; }

            [JsonPropertyName("error_code")]
            public int? ErrorCode { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }
        }

        private class TelegramUpdate
        {
            [JsonPropertyName("update_id")]
            public long UpdateId { get; set; }

            [JsonPropertyName("message")]
            public TelegramMessage? Message { get; set; }
        }

        private class TelegramMessage
        {
            [JsonPropertyName("message_id")]
            public long MessageId { get; set; }

            [JsonPropertyName("from")]
            public TelegramUser? From { get; set; }

            [JsonPropertyName("chat")]
            public TelegramChat? Chat { get; set; }

            [JsonPropertyName("date")]
            public long Date { get; set; }

            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

        private class TelegramUser
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("is_bot")]
            public bool? IsBot { get; set; }

            [JsonPropertyName("first_name")]
            public string? FirstName { get; set; }

            [JsonPropertyName("username")]
            public string? Username { get; set; }
        }

        private class TelegramChat
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("first_name")]
            public string? FirstName { get; set; }

            [JsonPropertyName("username")]
            public string? Username { get; set; }
        }

        #endregion
    }
}
