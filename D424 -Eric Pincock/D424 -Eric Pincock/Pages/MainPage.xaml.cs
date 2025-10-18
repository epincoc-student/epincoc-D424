using Plugin.LocalNotification;
using D424__Eric_Pincock.ViewModels;
using D424__Eric_Pincock.Classes;
using SQLite;

namespace D424__Eric_Pincock.Pages
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel viewModel;
        private User loggedInUser;
        private LocalDbService _dbService;

        public MainPage(User loggedInUser)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            _dbService = new LocalDbService();
            this.loggedInUser = loggedInUser;
            viewModel = new MainPageViewModel();
            BindingContext = viewModel;

            InitializeAndLoad();
        }

        private async void InitializeAndLoad()
        {
            await _dbService.InitAsync();
            await _dbService.ClearDatabaseAsync();
            await _dbService.SeedCoursesAsync();
            await _dbService.SeedDegreesFromCatalogAsync();
            await viewModel.LoadDegreesAsync();

            BindingContext = viewModel;
        }

        // Banner Menu Buttons Methods

        private async void DetailsButton_Clicked(object sender, EventArgs e)
        {
            double screenWidth = this.Width;

            if (!DropdownPanel.IsVisible)
            {
                DropdownPanel.TranslationY = -100;
                DropdownPanel.Opacity = 0;
                DropdownPanel.IsVisible = true;

                await Task.WhenAll(
                    DropdownPanel.FadeTo(1, 250, Easing.CubicOut),
                    DropdownPanel.TranslateTo(0, 0, 250, Easing.CubicOut));
            }
            else
            {
                await Task.WhenAll(
                    DropdownPanel.FadeTo(0, 200, Easing.CubicIn),
                    DropdownPanel.TranslateTo(0, -100, 200, Easing.CubicIn));

                DropdownPanel.IsVisible = false;
            }
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            if (DropdownPanel.IsVisible)
            {
                await Task.WhenAll(
                    DropdownPanel.FadeTo(0, 200, Easing.CubicIn),
                    DropdownPanel.TranslateTo(0, -100, 200, Easing.CubicIn));

                DropdownPanel.IsVisible = false;
            }
        }

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Log Out", "Are you sure you want to log out?", "Yes", "Cancel");
            if (!confirm)
                return;

            // Clear session data
            Preferences.Remove("UserId");
            Preferences.Remove("UserName");
            Preferences.Remove("UserFirstName");
            Preferences.Remove("DegreeId");

            // Hide dropdown
            DropdownPanel.IsVisible = false;

            // Reset to login page
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }

        //Page Buttons

        private async void SelectDegreeButton_Clicked(object sender, EventArgs e)
        {
            if (viewModel.SelectedDegree == null)
            {
                await DisplayAlert("Error", "Please select a degree first.", "OK");
                return;
            }

            var selectedDegree = viewModel.ActiveDegree;
            if (selectedDegree == null)
            {
                await DisplayAlert("Error", "Degree details not loaded yet.", "OK");
                return;
            }

            var userDegree = new UserDegree
            {
                UserId = loggedInUser.UserId,
                DegreeId = selectedDegree.DegreeId
            };

            await _dbService.SaveOrUpdateUserDegreeAsync(userDegree);

            Preferences.Set("UserId", loggedInUser.UserId);
            Preferences.Set("DegreeId", selectedDegree.DegreeId);

            Application.Current.MainPage = new NavigationPage(
                new TermsPage(loggedInUser.UserId, selectedDegree.DegreeId));
        }
    }
}
