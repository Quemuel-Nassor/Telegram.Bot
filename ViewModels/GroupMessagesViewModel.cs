using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Telegram.Bot.Services;

namespace Telegram.Bot.ViewModels
{
    public class GroupMessagesViewModel : INotifyPropertyChanged
    {
        private readonly ITelegramBotService _telegramService;
        private readonly IBackgroundWorker _backgroundWorker;
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

        public GroupMessagesViewModel(ITelegramBotService telegramService, IBackgroundWorker backgroundWorker)
        {
            _telegramService = telegramService ?? throw new ArgumentNullException(nameof(telegramService));
            _backgroundWorker = backgroundWorker ?? throw new ArgumentNullException(nameof(backgroundWorker));
            _messages = new ObservableCollection<GroupMessage>();
            RefreshCommand = new AsyncCommand(RefreshMessages);
        }

        private async Task RefreshMessages()
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

            IsRefreshing = true;

            // Delega a tarefa pesada para o background worker
            await _backgroundWorker.EnqueueAsync(async () =>
            {
                try
                {
                    var messages = await _telegramService.GetGroupUpdatesAsync(token);

                    // Atualiza a UI na thread principal
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Messages.Clear();
                        foreach (var msg in messages)
                        {
                            Messages.Add(msg);
                        }

                        IsRefreshing = false;
                    });
                }
                catch (Exception ex)
                {
#pragma warning disable CS0618
                    _ = Shell.Current.DisplayAlert(
                        "Erro",
                        $"Erro ao buscar mensagens: {ex.Message}",
                        "OK");

#pragma warning restore CS0618
                    IsRefreshing = false;
                }
            });
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
