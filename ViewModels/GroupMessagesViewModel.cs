using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Telegram.Bot.Services;

namespace Telegram.Bot.ViewModels
{
    public class GroupMessagesViewModel : INotifyPropertyChanged
    {
        private readonly ITelegramBotService _telegramService;
        private bool _isRefreshing;
        private ObservableCollection<GroupMessage> _messages;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<GroupMessage> Messages
        {
            get => _messages;
            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    OnPropertyChanged(nameof(Messages));
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged(nameof(IsRefreshing));
                }
            }
        }

        public ICommand RefreshCommand { get; }

        public GroupMessagesViewModel(ITelegramBotService telegramService)
        {
            _telegramService = telegramService ?? throw new ArgumentNullException(nameof(telegramService));
            _messages = new ObservableCollection<GroupMessage>();
            RefreshCommand = new AsyncCommand(RefreshMessages);
        }

        private async Task RefreshMessages()
        {
            IsRefreshing = true;
            try
            {
                var token = await SecureStorage.GetAsync("telegram_api_token");
                
                if (string.IsNullOrEmpty(token))
                {
#pragma warning disable CS0618
                    _ = Shell.Current.DisplayAlert(
                        "Aviso",
                        "Configure o token da API primeiro",
                        "OK");
#pragma warning restore CS0618
                    return;
                }

                var messages = await _telegramService.GetGroupUpdatesAsync(token);
                Messages = new ObservableCollection<GroupMessage>(messages);
            }
            catch (Exception ex)
            {
#pragma warning disable CS0618
                _ = Shell.Current.DisplayAlert(
                    "Erro",
                    $"Erro ao buscar mensagens: {ex.Message}",
                    "OK");
#pragma warning restore CS0618
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
