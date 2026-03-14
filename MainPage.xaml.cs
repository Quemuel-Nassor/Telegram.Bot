using Telegram.Bot.ViewModels;

namespace Telegram.Bot
{
    public partial class MainPage : ContentPage
    {
        private readonly GroupMessagesViewModel _viewModel;

        public MainPage(GroupMessagesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            BindingContext = _viewModel;
        }
    }
}
