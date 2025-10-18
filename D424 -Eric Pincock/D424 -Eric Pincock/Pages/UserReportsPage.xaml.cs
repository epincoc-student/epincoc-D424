namespace D424__Eric_Pincock.Pages;

using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.ViewModels;

public partial class UserReportsPage : ContentPage
{
    private readonly UserReportsPageViewModel _viewModel;

    public UserReportsPage(int userId, int degreeId)
    {
        InitializeComponent();
        _viewModel = new UserReportsPageViewModel(userId, degreeId);
        BindingContext = _viewModel;
    }
    
    // Banner Menu Button Methods

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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
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

    // Run Report Button Method
    private async void RunReportButton_Clicked(object sender, EventArgs e)
    {
        await _viewModel.LoadReportAsync();
    }
}