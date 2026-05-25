namespace Demo_Pomodoro
{
    public partial class MainPage : ContentPage
    {
        private readonly ViewModels.MainPageViewModel _viewModel;

        public MainPage(ViewModels.MainPageViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            DeviceDisplay.Current.KeepScreenOn = true;
            _viewModel.OnAppearing();
        }
    }
}
