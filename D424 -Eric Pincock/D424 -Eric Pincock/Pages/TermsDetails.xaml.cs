using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.LocalNotification;

namespace D424__Eric_Pincock.Pages;

public partial class TermsDetails : ContentPage
{
    private UserTerm _term;
    private List<UserDegreeCourse> _allUserCourses = new();
    private int _userId;
    private int _degreeId;
    private int _termId;
    private LocalDbService _dbService;
    private UserDegreeCourse _selectedCourse;
    private TermsDetailsViewModel _viewModel;

    public TermsDetails(int UserId, UserTerm passedTerm, int degreeId)
    {
        InitializeComponent();

        _term = passedTerm;
        _termId = passedTerm.TermId;
        _userId = UserId;
        _degreeId = degreeId;

        _dbService = new LocalDbService();

        TermNameLabel.Text = passedTerm.TermName;
        TermStatusLabel.Text = $"Term Status: {passedTerm.TermStatus}";
        TermDatesLabel.Text =
            $"Start: {passedTerm.TermStartDate:MMM dd, yyyy} - End: {passedTerm.TermEndDate:MMM dd, yyyy}";

        _viewModel = new TermsDetailsViewModel(_userId, _degreeId, _termId);
        _viewModel.CourseTapped += OnCourseTapped;
        _viewModel.SearchResultTapped += OnSearchResultsTapped;
        _viewModel.CourseEditRequested += OnCourseEditRequested;

        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    // Course Methods

    private async void OnCourseEditRequested(object sender,
                                             UserDegreeCourse course)
    {
        _selectedCourse = course;
        SelectedCourseName.Text = $"Selected Course: {course.CourseName}";
        CourseStartDatePicker.Date = course.StartDate;
        SelectedCourseCredits.Text = $"Credits: {_selectedCourse.CourseCredits}";
        CourseEndDatePicker.Date = course.EndDate;
        CourseStatusPicker.SelectedItem = course.Status;
        RemoveCourseButton.IsVisible = true;

        AddCourseButton_Clicked(this, EventArgs.Empty);
    }

    private async void AddCourseButton_Clicked(object sender, EventArgs e)
    {
        CourseEditorPanel.IsVisible = true;
        CourseEditorPanel.Opacity = 0;
        CourseEditorPanel.TranslationY = 500;

        CourseStartDatePicker.Date = DateTime.Today;
        CourseEndDatePicker.Date = DateTime.Today.AddMonths(3);

        await CourseEditorPanel.FadeTo(1, 250);
        await CourseEditorPanel.TranslateTo(0, 0, 300, Easing.CubicOut);
    }


    private async void RemoveCourseButton_Clicked(object sender, EventArgs e)
    {
        if (_selectedCourse == null)
        {
            await DisplayAlert("No Course Selected",
                               "Please select a course to remove from the term.",
                               "OK");
            return;
        }

        bool confirm = await DisplayAlert(
            "Confirm Removal",
            $"Are you sure you want to remove the course \"{_selectedCourse.CourseName}\" from the term? This will reset its status to 'Upcoming' and remove its progress from your degree tracker.",
            "Remove", "Cancel");

        if (!confirm)
            return;

        var updatedCourse =
            new UserDegreeCourse
            {
                Id = _selectedCourse.Id,
                Status = "Upcoming",
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MinValue,
                TermId = 0
            };

        await _dbService.UpdateUserDegreeCourseAsync(updatedCourse, 0);

        // Clear UI selections
        _selectedCourse = null;
        SelectedCourseName.Text = "Please Select a Course";
        CourseStatusPicker.SelectedItem = null;
        CourseStartDatePicker.Date = DateTime.Today;
        CourseEndDatePicker.Date = DateTime.Today.AddMonths(3);
        RemoveCourseButton.IsVisible = false;

        await HideCourseEditorAsync();
        _viewModel.RefreshTermCourses();
    }

    private async void SaveCourseButton_Clicked(object sender, EventArgs e)
    {
        // Validation: course selected
        if (_selectedCourse == null)
        {
            await DisplayAlert("Missing Course",
                               "Please select a course before saving.", "OK");
            return;
        }

        // Validation: status selected
        if (CourseStatusPicker.SelectedItem == null)
        {
            await DisplayAlert("Missing Status", "Please select a course status.",
                               "OK");
            return;
        }

        // Validation: end date after start date
        if (CourseEndDatePicker.Date <= CourseStartDatePicker.Date)
        {
            await DisplayAlert("Invalid Dates",
                               "End date must be after start date.", "OK");
            return;
        }

        // Check for existing term id
        if (_selectedCourse.TermId != 0 &&
            _selectedCourse.TermId != _term.TermId)
        {
            var dbService = new LocalDbService();
            var assignedTerm =
                await dbService.GetTermByIdAsync(_selectedCourse.TermId);

            string termName = assignedTerm?.TermName ?? "Unknown";

            bool reassign = await DisplayAlert(
                "Course Already Assigned",
                $"This course is already listed under term: {termName}.\nDo you want to reassign it to this term?",
                "Reassign Anyway", "Cancel");

            if (!reassign)
                return;
        }

        // Update course info
        var updatedCourse = new UserDegreeCourse
        {
            Id = _selectedCourse.Id,
            DegreeId = _degreeId,
            TermId = _termId,
            StartDate = CourseStartDatePicker.Date,
            EndDate = CourseEndDatePicker.Date,
            Status = CourseStatusPicker.SelectedItem.ToString()
        };

        await _dbService.UpdateUserDegreeCourseAsync(updatedCourse, _term.TermId);

        // Clear selections/entries
        _selectedCourse = null;
        SelectedCourseName.Text = "Please select a course.";
        SelectedCoursePreReq.Text = null;
        CourseStatusPicker.SelectedItem = null;
        CourseStartDatePicker.Date = DateTime.Today;
        CourseEndDatePicker.Date = DateTime.Today.AddMonths(3);
        RemoveCourseButton.IsVisible = false;

        await HideCourseEditorAsync();
        _viewModel.RefreshTermCourses();
    }

    private async void CancelCourseEdit_Clicked(object sender, EventArgs e)
    {
        CourseStatusPicker.SelectedItem = null;
        CourseStartDatePicker.Date = DateTime.Today;
        CourseEndDatePicker.Date = DateTime.Today.AddMonths(3);
        _selectedCourse = null;
        SelectedCourseName.Text = "Please Select a Course";
        RemoveCourseButton.IsVisible = false;
        await HideCourseEditorAsync();
    }

    // Course selected - Navigation Method

    private async void OnCourseTapped(object sender, UserDegreeCourse course)
    {
        var courseDetailsPage = new CourseDetailsPage(_userId, course);
        await Navigation.PushModalAsync(courseDetailsPage);
    }

    // Course Search Methods

    private void OnCourseSearchTextChanged(object sender,
                                           TextChangedEventArgs e)
    {
        _viewModel.FilterCourses(e.NewTextValue);

        CourseSearchResults.ItemsSource = _viewModel.FilteredCourses;
        CourseSearchResults.IsVisible = _viewModel.FilteredCourses.Any();
    }

    private void OnCourseSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
                is UserDegreeCourse selectedCourse)
        {
            // Populate fields with selected course
            _selectedCourse = selectedCourse;
            SelectedCourseName.Text =
                $"Course Selected: {selectedCourse.CourseName}";
            SelectedCourseCredits.Text =
                $"Credits: {selectedCourse.CourseCredits}";
            SelectedCoursePreReq.Text =
                $"Prerequisite Course: {selectedCourse.PreReqName}";

            CourseSearchBar.TextChanged -= OnCourseSearchTextChanged;

            // Collapse search results and clear search bar
            CourseSearchResults.IsVisible = false;
            CourseSearchBar.Text = string.Empty;
            CourseSearchResults.SelectedItem = null;

            CourseSearchBar.TextChanged += OnCourseSearchTextChanged;
        }
    }

    private void OnSearchResultsTapped(object sender, UserDegreeCourse course)
    {
        CourseStartDatePicker.Date = course.StartDate;
        CourseEndDatePicker.Date = course.EndDate;
        CourseStatusPicker.SelectedItem = course.Status;

        _selectedCourse = course;
        SelectedCourseName.Text = $"Course Selected: {course.CourseName}";
        SelectedCourseCredits.Text = $"Credits: {course.CourseCredits}";
        SelectedCoursePreReq.Text = $"Prerequisite Course: {course.PreReqName}";

        CourseSearchBar.TextChanged -= OnCourseSearchTextChanged;
        CourseSearchResults.IsVisible = false;
        CourseSearchBar.Text = string.Empty;
        CourseSearchBar.TextChanged += OnCourseSearchTextChanged;
    }

    

    // Panel Animation Method

    private async Task HideCourseEditorAsync()
    {
        await CourseEditorPanel.TranslateTo(0, 500, 300, Easing.CubicIn);
        await CourseEditorPanel.FadeTo(0, 250);
        CourseEditorPanel.IsVisible = false;
    }
}