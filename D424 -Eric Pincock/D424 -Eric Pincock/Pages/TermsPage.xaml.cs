using D424__Eric_Pincock.Classes.Models;
using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.ViewModels;
using System.Diagnostics;

namespace D424__Eric_Pincock.Pages;

public partial class TermsPage : ContentPage
{
    private int _userId;
    private int _degreeId;
    private LocalDbService _dbService;
    private TermsPageViewModel _viewModel;
    public TermsPage(int userId, int degreeId)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);

        _userId = userId;
        _degreeId = degreeId;
        _dbService = new LocalDbService();

        _viewModel = new TermsPageViewModel(userId, _degreeId);
        _viewModel.TermSelected += OnTermSelected;
        _viewModel.TermEditRequested += OnTermEditRequested;
        BindingContext = _viewModel;

        InitializeAndLoad();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        int lastDegreeId = Preferences.Get("LastDegreeId", -1);

        if (lastDegreeId != _degreeId)
        {
            await CloneCoursesIfNeeded();
            Preferences.Set("LastDegreeId", _degreeId);
        }

        await _viewModel.LoadUserTermsAsync();
        await _viewModel.UpdateCreditProgressAsync();
        await UpdateDegreeNameAsync();
    }
    public async Task RefreshTermsAsync()
    {
        await _viewModel.LoadUserTermsAsync();
        await _viewModel.UpdateCreditProgressAsync();
        await UpdateDegreeNameAsync();
    }

    // Page initialization methods

    private async Task UpdateDegreeNameAsync()
    {
        int degreeId = Preferences.Get("DegreeId", 0);

        var allDegrees = await _dbService.GetAllDegreesAsync();
        var selectedDegree =
            allDegrees.FirstOrDefault(d => d.DegreeId == degreeId);

        if (selectedDegree != null)
        {
            DegreeNameLabel.Text = $"Current Major: {selectedDegree.DegreeName}";
        }
        else
        {
            DegreeNameLabel.Text = "Your Degree Courses";
        }
    }

    private async Task CloneCoursesIfNeeded()
    {
        var degree = DegreeCatalog.GetById(_degreeId);
        if (degree == null || degree.DegreeCourses == null)
        {
            Debug.WriteLine(
                $"No degree found for DegreeId {_degreeId} or no DegreeCourses.");
            return;
        }

        foreach (var dc in degree.DegreeCourses)
        {
            var course = await _dbService.GetCourseById(dc.CourseId);

            // Check if course exists for user in any degree
            var existingCourse =
                await _dbService.GetUserDegreeCourseByUserAndCourseId(
                    _userId, course.CourseId);

            if (existingCourse != null)
            {
                if (existingCourse.DegreeId != _degreeId)
                {
                    // Update the DegreeId to the new one
                    existingCourse.DegreeId = _degreeId;
                    await _dbService.UpdateUserDegreeCourseAsync(
                        existingCourse, existingCourse.TermId);
                }

                continue;
            }

            var userCourse =
                new UserDegreeCourse
                {
                    UserId = _userId,
                    DegreeId = _degreeId,
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseCredits = course.CourseCredits,
                    PreReqsNeeded = course.PreReqsNeeded,
                    PreReqCourses = course.PreReqCourses,
                    StartDate = DateTime.MinValue,
                    EndDate = DateTime.MinValue,
                    Status = "Upcoming"
                };

            await _dbService.InsertUserDegreeCourseAsync(userCourse);
        }
    }

    private async void InitializeAndLoad()
    {
        var service = new LocalDbService();
    }

    // Banner Menu Button Methods

    private async void OnBannerMenuClicked(object sender, EventArgs e)
    {
        double screenWidth = this.Width;

        if (!BannerMenu.IsVisible)
        {
            BannerMenu.TranslationY = -100;
            BannerMenu.Opacity = 0;
            BannerMenu.IsVisible = true;

            await Task.WhenAll(BannerMenu.TranslateTo(0, 0, 250, Easing.CubicOut),
                               BannerMenu.FadeTo(1, 250, Easing.CubicOut));
        }
        else
        {
            await Task.WhenAll(
                BannerMenu.TranslateTo(0, -100, 200, Easing.CubicIn),
                BannerMenu.FadeTo(0, 200, Easing.CubicIn));

            BannerMenu.IsVisible = false;
        }
    }

    private async void OnUserReportsButton_Clicked(object sender, EventArgs e)
    {
        var userReportsPage = new UserReportsPage(_userId, _degreeId);
        await Navigation.PushModalAsync(userReportsPage);

        if (BannerMenu.IsVisible)
        {
            await Task.WhenAll(
                BannerMenu.FadeTo(0, 200, Easing.CubicIn),
                BannerMenu.TranslateTo(0, -100, 200, Easing.CubicIn));

            BannerMenu.IsVisible = false;
        }
    }

    private async void OnChangeDegreeClicked(object sender, EventArgs e)
    {
        BannerMenu.IsVisible = false;

        // Retrieve the full user object
        var user = await _dbService.GetUserByUserId(_userId);

        if (user != null)
        {
            // Navigate to MainPage with the user object
            Application.Current.MainPage = new NavigationPage(new MainPage(user));
        }
        else
        {
            await DisplayAlert("Error", "User not found.", "OK");
        }
    }

    private async void OnLogOutClicked(object sender, EventArgs e)
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
        BannerMenu.IsVisible = false;

        // Reset to login page
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        if (BannerMenu.IsVisible)
        {
            await Task.WhenAll(
                BannerMenu.FadeTo(0, 200, Easing.CubicIn),
                BannerMenu.TranslateTo(0, -100, 200, Easing.CubicIn));

            BannerMenu.IsVisible = false;
        }
    }


    // Select term - Page Navigation Method

    private async void OnTermSelected(object sender, UserTerm term)
    {
        var termDetailsPage = new TermsDetails(_userId, term, _degreeId);
        await Navigation.PushModalAsync(termDetailsPage);
    }

    // Term Methods

    private async void OnTermEditRequested(object sender, UserTerm term)
    {
        DeleteTermButton.IsVisible = true;
        TermNameEntry.Text = term.TermName;
        TermStatusPicker.SelectedIndex =
            term.TermStatus switch
            {
                "Upcoming" => 0,
                "In Progress" => 1,
                "Completed" => 2,
                _ => 0
            };
        TermStartDatePicker.Date = term.TermStartDate;
        TermEndDatePicker.Date = term.TermEndDate;

        TermEditorPanel.IsVisible = true;
        TermEditorPanel.Opacity = 0;
        TermEditorPanel.TranslationY = 500;

        await TermEditorPanel.FadeTo(1, 250);
        await TermEditorPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
    }

    private async void AddTermButton_Clicked(object sender, EventArgs e)
    {
        TermEditorPanel.IsVisible = true;
        TermEditorPanel.Opacity = 0;
        TermEditorPanel.TranslationY = 500;

        TermNameEntry.Text = string.Empty;
        TermStartDatePicker.Date = DateTime.Today;
        TermEndDatePicker.Date = DateTime.Today.AddMonths(3);

        await TermEditorPanel.FadeTo(1, 250);
        await TermEditorPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
    }
    private async void SaveTermButton_Clicked(object sender, EventArgs e)
    {
        if (!ValidateTermForm(out string errorMessage))
        {
            await DisplayAlert("Validation Error", errorMessage, "OK");
            return;
        }

        if (_viewModel.SelectedTerm == null)
        {
            var newTerm = new UserTerm
            {
                UserId = _userId,
                TermName = TermNameEntry.Text,
                TermStartDate = TermStartDatePicker.Date,
                TermEndDate = TermEndDatePicker.Date,
                TermStatus = TermStatusPicker.SelectedItem?.ToString() ?? "Upcoming"
            };

            await _dbService.InsertUserTermAsync(newTerm);
        }
        else
        {
            _viewModel.SelectedTerm.TermName = TermNameEntry.Text;
            _viewModel.SelectedTerm.TermStartDate = TermStartDatePicker.Date;
            _viewModel.SelectedTerm.TermEndDate = TermEndDatePicker.Date;
            _viewModel.SelectedTerm.TermStatus =
                TermStatusPicker.SelectedItem?.ToString();

            await _dbService.UpdateUserTermAsync(_viewModel.SelectedTerm);
        }

        _viewModel.SelectedTerm = null;
        TermNameEntry.Text = string.Empty;
        TermStatusPicker.SelectedItem = null;
        TermStartDatePicker.Date = DateTime.Today;
        TermEndDatePicker.Date = DateTime.Today.AddMonths(3);
        DeleteTermButton.IsVisible = false;
        await _viewModel.LoadUserTermsAsync();
        await HideTermEditorAsync();
    }
    private async void DeleteTermButton_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedTerm != null)
        {
            if (_viewModel.SelectedTerm != null)
            {
                bool confirmDelete = await DisplayAlert(
                    "Confirm Deletion",
                    $"Are you sure you want to delete the term \"{_viewModel.SelectedTerm.TermName}\"? This will also unassign all associated courses.",
                    "Delete", "Cancel");

                if (!confirmDelete)
                    return;
            }

            int termIdToDelete = _viewModel.SelectedTerm.TermId;
            int userId = _viewModel.SelectedTerm.UserId;

            var associatedCourses =
                await _dbService.GetCoursesByTermAsync(userId, termIdToDelete);

            foreach (var course in associatedCourses)
            {
                course.TermId = 0;
                await _dbService.UpdateUserDegreeCourseAsync(course, 0);
            }

            await _dbService.DeleteUserTermAsync(termIdToDelete);

            _viewModel.SelectedTerm = null;
            TermNameEntry.Text = string.Empty;
            TermStatusPicker.SelectedItem = null;
            TermStartDatePicker.Date = DateTime.Today;
            TermEndDatePicker.Date = DateTime.Today.AddMonths(3);
            DeleteTermButton.IsVisible = false;

            await _viewModel.LoadUserTermsAsync();
            await HideTermEditorAsync();
        }
    }

    private async void CancelTermEdit_Clicked(object sender, EventArgs e)
    {
        _viewModel.SelectedTerm = null;
        TermNameEntry.Text = string.Empty;
        TermStatusPicker.SelectedItem = null;
        TermStartDatePicker.Date = DateTime.Today;
        TermEndDatePicker.Date = DateTime.Today.AddMonths(3);
        DeleteTermButton.IsVisible = false;
        await HideTermEditorAsync();
    }


    private bool ValidateTermForm(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(TermNameEntry.Text))
        {
            errorMessage = "Please enter a term name.";
            return false;
        }

        if (TermStatusPicker.SelectedIndex < 0)
        {
            errorMessage = "Please select a status from the picker.";
            return false;
        }

        if (TermEndDatePicker.Date <= TermStartDatePicker.Date)
        {
            errorMessage = "End date must be after start date.";
            return false;
        }

        return true;
    }


    // Panel Animation Method

    private async Task HideTermEditorAsync()
    {
        await TermEditorPanel.TranslateTo(0, 500, 300, Easing.CubicIn);
        await TermEditorPanel.FadeTo(0, 250);
        TermEditorPanel.IsVisible = false;
    }

}