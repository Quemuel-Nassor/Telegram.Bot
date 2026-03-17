using System.Diagnostics;
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
            var globalStart = Stopwatch.GetTimestamp();
            long t_getasync_start = 0, t_getasync_end = 0;
            long t_stream_start = 0, t_stream_end = 0;
            long t_json_start = 0, t_json_end = 0;
            long t_parse_start = 0, t_parse_end = 0;
            
            try
            {
                var client = _httpClientFactory.CreateClient("TelegramClient");

                var url = $"/bot{apiToken}/getUpdates";
                if (offset.HasValue)
                {
                    url += $"?offset={offset}";
                }

                // ResponseHeadersRead evita bufferziar toda resposta na memória
                t_getasync_start = Stopwatch.GetTimestamp();
                using var httpResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                t_getasync_end = Stopwatch.GetTimestamp();
                
                httpResponse.EnsureSuccessStatusCode();

                // Stream read
                t_stream_start = Stopwatch.GetTimestamp();
                await using var contentStream = await httpResponse.Content.ReadAsStreamAsync();
                t_stream_end = Stopwatch.GetTimestamp();

                // Usa Source Generator para eliminar reflexão (30-50% CPU reduction em ARM32)
                t_json_start = Stopwatch.GetTimestamp();
                var response = await JsonSerializer.DeserializeAsync(
                    contentStream,
                    TelegramJsonContext.Default.TelegramUpdatesResponse);
                t_json_end = Stopwatch.GetTimestamp();

                if (response?.Ok == true && response.Result != null && response.Result.Count > 0)
                {
                    t_parse_start = Stopwatch.GetTimestamp();
                    var result = ParseUpdatesToMessages(response.Result);
                    t_parse_end = Stopwatch.GetTimestamp();
                    
                    // Log uma única vez ao final (timestamp não afeta latência)
                    var total_ms = (Stopwatch.GetTimestamp() - globalStart) / (Stopwatch.Frequency / 1000.0);
                    var getasync_ms = (t_getasync_end - t_getasync_start) / (Stopwatch.Frequency / 1000.0);
                    var stream_ms = (t_stream_end - t_stream_start) / (Stopwatch.Frequency / 1000.0);
                    var json_ms = (t_json_end - t_json_start) / (Stopwatch.Frequency / 1000.0);
                    var parse_ms = (t_parse_end - t_parse_start) / (Stopwatch.Frequency / 1000.0);
                    
                    Console.WriteLine($"[TelegramBot]  GetAsync={getasync_ms:F0}ms | Stream={stream_ms:F0}ms | JSON={json_ms:F0}ms | Parse={parse_ms:F0}ms | Total={total_ms:F0}ms | Messages={result.Count}");
                    
                    return result;
                }

                return new List<GroupMessage>();
            }
            catch (Exception ex)
            {
                var total_ms = (Stopwatch.GetTimestamp() - globalStart) / (Stopwatch.Frequency / 1000.0);
                Console.WriteLine($"[TelegramBot]  ERROR: {ex.Message} (elapsed: {total_ms:F0}ms)");
                throw;
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

        public class TelegramUpdatesResponse
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

        public class TelegramUpdate
        {
            [JsonPropertyName("update_id")]
            public long UpdateId { get; set; }

            [JsonPropertyName("message")]
            public TelegramMessage? Message { get; set; }
        }

        public class TelegramMessage
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

        public class TelegramUser
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

        public class TelegramChat
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
