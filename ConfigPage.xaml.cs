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
            BindingContext = _viewModel;
        }
    }
}
