namespace D424__Eric_Pincock.Pages;
using D424__Eric_Pincock.Classes;
using Microsoft.Maui.Storage;
using Plugin.LocalNotification;
using SQLite;

public partial class LoginPage : ContentPage
{
    private LocalDbService _dbService;

    public LoginPage()
    {
        InitializeComponent();
        _dbService = new LocalDbService();
        OnStart();
    }

    // Populate User Tables
    private async void OnStart()
    {
        await _dbService.SeedUsersAsync();
    }

    // Login button and user validation
    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        await _dbService.UserTableCreation();

        string username = UserNameEntry.Text?.Trim();
        string enteredPassword = UserPasswordEntry.Text;

        var user = await _dbService.GetUserByUsernameAsync(username);

        if (user == null)
        {
            await DisplayAlert("Login Failed", "User not found", "OK");
            return;
        }

        string hashedInput =
            SecurityHelper.HashPassword(enteredPassword, user.PasswordSalt);

        if (hashedInput == user.PasswordHash)
        {
            await DisplayAlert("Login Successful",
                               $"Welcome {user.UserFirstName}!", "OK");

            // Store session data
            Preferences.Set("UserId", user.UserId);
            Preferences.Set("UserName", user.UserName);
            Preferences.Set("UserFirstName", user.UserFirstName);

            // Check for associated UserDegree
            var userDegree =
                await _dbService.GetUserDegreeByUserIdAsync(user.UserId);

            if (userDegree != null)
            {
                Preferences.Set("DegreeId", userDegree.DegreeId);
                Application.Current.MainPage = new NavigationPage(
                    new TermsPage(user.UserId, userDegree.DegreeId));
            }
            else
            {
                Application.Current.MainPage =
                    new NavigationPage(new MainPage(user));
            }
        }
        else
        {
            await DisplayAlert("Login Failed", "Incorrect password", "OK");
        }
    }
}