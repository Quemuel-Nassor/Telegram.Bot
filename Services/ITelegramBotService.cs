namespace Telegram.Bot.Services
{
    public interface ITelegramBotService
    {
        Task<List<GroupMessage>> GetGroupUpdatesAsync(string apiToken, long? offset = null);
    }

    /// <summary>
    /// Representa uma mensagem do grupo
    /// </summary>
    public class GroupMessage
    {
        public long UpdateId { get; set; }
        public long MessageId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderUsername { get; set; }
        public string? Text { get; set; }
        public string? GroupName { get; set; }
        public DateTime Date { get; set; }
    }
}
