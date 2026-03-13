using System.ComponentModel;
using System.Windows.Input;

namespace Telegram.Bot.ViewModels
{
    public class ConfigTokenViewModel : INotifyPropertyChanged
    {
        private string? _token;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Token
        {
            get => _token;
            set
            {
                if (_token != value)
                {
                    _token = value;
                    OnPropertyChanged(nameof(Token));
                }
            }
        }

        public ICommand SaveCommand { get; }
        public Func<Task>? OnSaved { get; set; }

        public ConfigTokenViewModel()
        {
            SaveCommand = new AsyncCommand(SaveToken);
            LoadToken();
        }

        private async Task SaveToken()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
#pragma warning disable CS0618
                _ = Shell.Current.DisplayAlert("Aviso", "Por favor, insira um token", "OK");
#pragma warning restore CS0618
                return;
            }

            try
            {
                await SecureStorage.SetAsync("telegram_api_token", Token);
#pragma warning disable CS0618
                _ = Shell.Current.DisplayAlert("Sucesso", "Token salvo com segurança", "OK");
#pragma warning restore CS0618
                
                if (OnSaved != null)
                    await OnSaved.Invoke();
            }
            catch (Exception ex)
            {
#pragma warning disable CS0618
                _ = Shell.Current.DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
#pragma warning restore CS0618
            }
        }

        private async void LoadToken()
        {
            try
            {
                Token = await SecureStorage.GetAsync("telegram_api_token");
            }
            catch
            {
                // Token não existe ou não pode ser carregado
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
