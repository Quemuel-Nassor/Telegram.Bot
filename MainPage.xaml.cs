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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.RefreshCommand.Execute(null);
        }

        private async void OnConfigClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("config");
        }
    }
}
