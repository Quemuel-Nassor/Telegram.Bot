using Telegram.Bot.ViewModels;

namespace Telegram.Bot
{
    public partial class ConfigPage : ContentPage
    {
        private readonly ConfigTokenViewModel _viewModel;

        public ConfigPage(ConfigTokenViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            
            // Callback para voltar após salvar
            _viewModel.OnSaved = async () => await Shell.Current.GoToAsync("..");
            
            BindingContext = _viewModel;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
