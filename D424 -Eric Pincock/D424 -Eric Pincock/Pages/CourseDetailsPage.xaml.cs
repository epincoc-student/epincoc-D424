using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.Classes.Models.UserCourseDetails;
using D424__Eric_Pincock.ViewModels;
using System.Text.RegularExpressions;
using Plugin.LocalNotification;

namespace D424__Eric_Pincock.Pages;

public partial class CourseDetailsPage : ContentPage
{
    public int _userId;
    public UserDegreeCourse _passedCourse;
    private LocalDbService _dbService;
    private CourseDetailsViewModel _viewModel;

    public CourseDetailsPage(int userId, UserDegreeCourse passedCourse)
    {
        _userId = userId;
        _passedCourse = passedCourse;
        _dbService = new LocalDbService();

        _viewModel = new CourseDetailsViewModel(userId, passedCourse);
        BindingContext = _viewModel;

        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel
            .LoadRelatedDataAsync();  // refresh instructor and assessments
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    // Banner Menu Dropdown Methods

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

    // Course Instrucror Methods

    private async void AddCourseInstructorButton_Clicked(object sender,
                                                         EventArgs e)
    {
        InstructorFlyInPanel.TranslationY = 500;
        InstructorFlyInPanel.Opacity = 0;
        InstructorFlyInPanel.IsVisible = true;

        await Task.WhenAll(
            InstructorFlyInPanel.FadeTo(1, 300, Easing.CubicOut),
            InstructorFlyInPanel.TranslateTo(0, 0, 300, Easing.CubicOut));
    }

    private async void CancelInstructorButton_Clicked(object sender,
                                                      EventArgs e)
    {
        await Task.WhenAll(
            InstructorFlyInPanel.FadeTo(0, 200, Easing.CubicIn),
            InstructorFlyInPanel.TranslateTo(0, 500, 200, Easing.CubicIn));

        InstructorFlyInPanel.IsVisible = false;
    }

    private async void EditInstructorButton_Clicked(object sender, EventArgs e)
    {
        if (_viewModel?.Instructor != null)
        {
            InstructorNameEntry.Text = _viewModel.Instructor.CourseInstructorName;
            InstructorEmailEntry.Text =
                _viewModel.Instructor.CourseInstructorEmail;
            InstructorPhoneEntry.Text =
                _viewModel.Instructor.CourseInstructorPhone;
        }

        InstructorFlyInPanel.TranslationY = 500;
        InstructorFlyInPanel.Opacity = 0;
        InstructorFlyInPanel.IsVisible = true;

        await Task.WhenAll(
            InstructorFlyInPanel.FadeTo(1, 300, Easing.CubicOut),
            InstructorFlyInPanel.TranslateTo(0, 0, 300, Easing.CubicOut));
    }

    private async void DeleteInstructorButton_Clicked(object sender,
                                                      EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Confirm Delete", "Are you sure you want to delete this instructor?",
            "Yes", "No");
        if (!confirm)
            return;

        if (_viewModel.Instructor != null)
        {
            await _dbService.DeleteCourseInstructorByIdAsync(
                _viewModel.Instructor.CourseInstructorId);
            _viewModel.Instructor = null;

            // Clear entry fields
            InstructorNameEntry.Text = null;
            InstructorEmailEntry.Text = null;
            InstructorPhoneEntry.Text = null;

            // Optionally hide the panel
            await Task.WhenAll(
                InstructorFlyInPanel.FadeTo(0, 200, Easing.CubicIn),
                InstructorFlyInPanel.TranslateTo(0, 500, 200, Easing.CubicIn));
            InstructorFlyInPanel.IsVisible = false;
        }
    }

    private async void SaveInstructorButton_Clicked(object sender, EventArgs e)
    {
        var name = InstructorNameEntry.Text?.Trim();
        var email = InstructorEmailEntry.Text?.Trim();
        var phoneText = InstructorPhoneEntry.Text?.Trim();

        if (!ValidateInstructorInput(name, email, phoneText, out string error))
        {
            await DisplayAlert("Validation Error", error, "OK");
            return;
        }

        var instructor = new CourseInstructor
        {
            CourseId = _passedCourse.CourseId,
            CourseInstructorName = name,
            CourseInstructorEmail = email,
            CourseInstructorPhone = phoneText
        };

        await _dbService.SaveOrUpdateCourseInstructorAsync(instructor);

        // Animate panel out
        await Task.WhenAll(
            InstructorFlyInPanel.FadeTo(0, 200, Easing.CubicIn),
            InstructorFlyInPanel.TranslateTo(0, 500, 200, Easing.CubicIn));
        InstructorFlyInPanel.IsVisible = false;
        _viewModel.Instructor = instructor;
    }

    private bool ValidateInstructorInput(string name, string email,
                                         string phoneText,
                                         out string errorMessage)
    {
        errorMessage = string.Empty;

        // Check name
        if (string.IsNullOrWhiteSpace(name))
        {
            errorMessage = "Instructor name is required.";
            return false;
        }

        // Check email format
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
            {
                errorMessage = "Invalid email address.";
                return false;
            }
        }
        catch
        {
            errorMessage = "Invalid email address.";
            return false;
        }

        // Check phone format: standard US formats
        var phonePattern = @"^(\+1\s?)?(\(?\d{3}\)?[\s.-]?)?\d{3}[\s.-]?\d{4}$";

        if (!Regex.IsMatch(phoneText, phonePattern))
        {
            errorMessage =
                "Phone number must be a valid US format (e.g., 123-456-7890, (123) 456-7890, or 1234567890).";
            return false;
        }

        return true;
    }

    // Objective Assessment Methods
    private async void AddObjectiveAssessmentButton_Clicked(object sender,
                                                            EventArgs e)
    {
        ObjectiveAssessmentFlyInPanel.TranslationY = 500;
        ObjectiveAssessmentFlyInPanel.Opacity = 0;
        ObjectiveAssessmentFlyInPanel.IsVisible = true;

        await Task.WhenAll(
            ObjectiveAssessmentFlyInPanel.FadeTo(1, 300, Easing.CubicOut),
            ObjectiveAssessmentFlyInPanel.TranslateTo(0, 0, 300,
                                                      Easing.CubicOut));
    }

    private async void SaveObjectiveAssessmentButton_Clicked(object sender,
                                                             EventArgs e)
    {
        var name = ObjectiveAssessmentNameEntry.Text?.Trim();
        var objectiveAssessmentStartDate =
            ObjectiveAssessmentStartDatePicker.Date;
        var objectiveAssessmentEndDate = ObjectiveAssessmentEndDatePicker.Date;

        // Validation: name must not be null or empty
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Missing Name",
                               "Please enter a name for the objective assessment.",
                               "OK");
            return;
        }

        // Validation: end date must be after start date
        if (objectiveAssessmentEndDate <= objectiveAssessmentStartDate)
        {
            await DisplayAlert("Invalid Dates",
                               "End date must be after the start date.", "OK");
            return;
        }

        var objectiveAssessment =
            new ObjectiveAssessment
            {
                CourseId = _passedCourse.CourseId,
                ObjectiveAssessmentName = name,
                oaStartDate = objectiveAssessmentStartDate,
                oaEndDate = objectiveAssessmentEndDate
            };

        await _dbService.SaveOrUpdateObjectiveAssessmentAsync(
            objectiveAssessment);

        // Animate panel out
        await Task.WhenAll(
            ObjectiveAssessmentFlyInPanel.FadeTo(0, 200, Easing.CubicIn),
            ObjectiveAssessmentFlyInPanel.TranslateTo(0, 500, 200,
                                                      Easing.CubicIn));

        ObjectiveAssessmentFlyInPanel.IsVisible = false;
        _viewModel.ObjectiveAssessment = objectiveAssessment;
    }
    private async void DeleteObjectiveAssessmentButton_Clicked(object sender,
                                                               EventArgs e)
    {
        // Confirm deletion
        bool confirm = await DisplayAlert(
            "Confirm Deletion",
            "Are you sure you want to delete this objective assessment?",
            "Delete", "Cancel");
        if (!confirm)
            return;

        // Delete from database
        if (_viewModel.ObjectiveAssessment != null)
        {
            await _dbService.DeleteObjectiveAssessmentById(
                _viewModel.ObjectiveAssessment.ObjectiveAssessmentId);
            _viewModel.ObjectiveAssessment = null;
        }

        // Reset UI fields
        ObjectiveAssessmentNameEntry.Text = null;
        ObjectiveAssessmentStartDatePicker.Date = DateTime.Today;
        ObjectiveAssessmentEndDatePicker.Date = DateTime.Today;

        // Animate panel out
        if (ObjectiveAssessmentFlyInPanel.IsVisible)
        {
            await Task.WhenAll(
                DropdownPanel.FadeTo(0, 200, Easing.CubicIn),
                DropdownPanel.TranslateTo(0, -100, 200, Easing.CubicIn));

            ObjectiveAssessmentFlyInPanel.IsVisible = false;
        }
    }

    private async void EditObjectiveAssessmentButton_Clicked(object sender,
                                                             EventArgs e)
    {
        var assessment = _viewModel.ObjectiveAssessment;
        if (assessment == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", "No objective assessment found to edit.", "OK");
            return;
        }

        ObjectiveAssessmentNameEntry.Text = assessment.ObjectiveAssessmentName;
        ObjectiveAssessmentStartDatePicker.Date = assessment.oaStartDate;
        ObjectiveAssessmentEndDatePicker.Date = assessment.oaEndDate;

        ObjectiveAssessmentFlyInPanel.TranslationY = 500;
        ObjectiveAssessmentFlyInPanel.Opacity = 0;
        ObjectiveAssessmentFlyInPanel.IsVisible = true;

        await Task.WhenAll(
            ObjectiveAssessmentFlyInPanel.FadeTo(1, 300, Easing.CubicOut),
            ObjectiveAssessmentFlyInPanel.TranslateTo(0, 0, 300,
                                                      Easing.CubicOut));
    }

    private async void CancelObjectiveAssessmentButton_Clicked(object sender,
                                                               EventArgs e)
    {
        if (ObjectiveAssessmentFlyInPanel.IsVisible)
        {
            await Task.WhenAll(
                DropdownPanel.FadeTo(0, 200, Easing.CubicIn),
                DropdownPanel.TranslateTo(0, -100, 200, Easing.CubicIn));

            ObjectiveAssessmentFlyInPanel.IsVisible = false;
        }
    }

    // Performance Assessment Methods

    private async void AddPerformanceAssessmentButton_Clicked(object sender,
                                                              EventArgs e)
    {
        PerformanceAssessmentFlyInPanel.TranslationY = 500;
        PerformanceAssessmentFlyInPanel.Opacity = 0;
        PerformanceAssessmentFlyInPanel.IsVisible = true;

        await Task.WhenAll(
            PerformanceAssessmentFlyInPanel.FadeTo(1, 300, Easing.CubicOut),
            PerformanceAssessmentFlyInPanel.TranslateTo(0, 0, 300,
                                                        Easing.CubicOut));
    }

    private async void SavePerformanceAssessmentButton_Clicked(object sender,
                                                               EventArgs e)
    {
        var name = PerformanceAssessmentNameEntry.Text?.Trim();
        var performanceAssessmentStartDate =
            PerformanceAssessmentStartDatePicker.Date;
        var performanceAssessmentEndDate =
            PerformanceAssessmentEndDatePicker.Date;

        // Validation: name must not be null or empty
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert(
                "Missing Name",
                "Please enter a name for the performance assessment.", "OK");
            return;
        }

        // Validation: end date must be after start date
        if (performanceAssessmentEndDate <= performanceAssessmentStartDate)
        {
            await DisplayAlert("Invalid Dates",
                               "End date must be after the start date.", "OK");
            return;
        }

        var performanceAssessment = new PerformanceAssessment
        {
            CourseId = _passedCourse.CourseId,
            PerformanceAssessmentName = name,
            paStartDate = performanceAssessmentStartDate,
            paEndDate = performanceAssessmentEndDate
        };

        await _dbService.SaveOrUpdatePerformanceAssessmentAsync(
            performanceAssessment);

        // Animate panel out
        await Task.WhenAll(
            PerformanceAssessmentFlyInPanel.FadeTo(0, 200, Easing.CubicIn),
            PerformanceAssessmentFlyInPanel.TranslateTo(0, 500, 200,
                                                        Easing.CubicIn));

        PerformanceAssessmentFlyInPanel.IsVisible = false;
        _viewModel.PerformanceAssessment = performanceAssessment;
    }
    private async void CancelPerformanceAssessmentButton_Clicked(object sender,
                                                                 EventArgs e)
    {
        if (PerformanceAssessmentFlyInPanel.IsVisible)
        {
            await Task.WhenAll(
                DropdownPanel.FadeTo(0, 200, Easing.CubicIn),
                DropdownPanel.TranslateTo(0, -100, 200, Easing.CubicIn));

            PerformanceAssessmentFlyInPanel.IsVisible = false;
        }
    }

    private async void DeletePerformanceAssessmentButton_Clicked(object sender,
                                                                 EventArgs e)
    {
        // Confirm deletion
        bool confirm = await DisplayAlert(
            "Confirm Deletion",
            "Are you sure you want to delete this performance assessment?",
            "Delete", "Cancel");
        if (!confirm)
            return;

        // Delete from database
        if (_viewModel.PerformanceAssessment != null)
        {
            await _dbService.DeletePerformanceAssessmentById(
                _viewModel.PerformanceAssessment.PerformanceAssessmentId);
            _viewModel.PerformanceAssessment = null;
        }

        // Reset UI fields
        PerformanceAssessmentNameEntry.Text = null;
        PerformanceAssessmentStartDatePicker.Date = DateTime.Today;
        PerformanceAssessmentEndDatePicker.Date = DateTime.Today;

        // Animate panel out
        if (PerformanceAssessmentFlyInPanel.IsVisible)
        {
            await Task.WhenAll(
                DropdownPanel.FadeTo(0, 200, Easing.CubicIn),
                DropdownPanel.TranslateTo(0, -100, 200, Easing.CubicIn));

            PerformanceAssessmentFlyInPanel.IsVisible = false;
        }
    }

    private async void EditPerformanceAssessmentButton_Clicked(object sender,
                                                               EventArgs e)
    {
        var assessment = _viewModel.PerformanceAssessment;
        if (assessment == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", "No performance assessment found to edit.", "OK");
            return;
        }

        PerformanceAssessmentNameEntry.Text =
            assessment.PerformanceAssessmentName;
        PerformanceAssessmentStartDatePicker.Date = assessment.paStartDate;
        PerformanceAssessmentEndDatePicker.Date = assessment.paEndDate;

        PerformanceAssessmentFlyInPanel.TranslationY = 500;
        PerformanceAssessmentFlyInPanel.Opacity = 0;
        PerformanceAssessmentFlyInPanel.IsVisible = true;

        await Task.WhenAll(
            PerformanceAssessmentFlyInPanel.FadeTo(1, 300, Easing.CubicOut),
            PerformanceAssessmentFlyInPanel.TranslateTo(0, 0, 300,
                                                        Easing.CubicOut));
    }

    // Notifications Methods

    private async void AddNotificationButton_Clicked(object sender,
                                                     EventArgs e)
    {
        NotificationsFlyInPanel.TranslationY = 500;
        NotificationsFlyInPanel.Opacity = 0;
        NotificationsFlyInPanel.IsVisible = true;

        await Task.WhenAll(
            NotificationsFlyInPanel.FadeTo(1, 300, Easing.CubicOut),
            NotificationsFlyInPanel.TranslateTo(0, 0, 300, Easing.CubicOut));
    }

    private async void CancelNotificationButton_Clicked(object sender,
                                                        EventArgs e)
    {
        await Task.WhenAll(
            NotificationsFlyInPanel.FadeTo(0, 200, Easing.CubicIn),
            NotificationsFlyInPanel.TranslateTo(0, 500, 200, Easing.CubicIn));

        NotificationsFlyInPanel.IsVisible = false;
    }

    private async void SaveNotificationButton_Clicked(object sender,
                                                      EventArgs e)
    {
        DateTime notifyDateTime = new DateTime(
            NotificationDatePicker.Date.Year, NotificationDatePicker.Date.Month,
            NotificationDatePicker.Date.Day, NotificationTimePicker.Time.Hours,
            NotificationTimePicker.Time.Minutes, 0);

        var request = new NotificationRequest
        {
            NotificationId = new Random().Next(1000, 9999),  // Unique ID
            Title = NotificationNameEntry.Text,
            Description = NotificationDescriptionEditor.Text,
            ReturningData = "CourseReminder",  // Optional tag
            CategoryType = NotificationCategoryType.Reminder,
            Schedule =
               new NotificationRequestSchedule
               {
                   NotifyTime = notifyDateTime,
                   NotifyRepeatInterval = TimeSpan.Zero  // No repeat
               }
        };

        LocalNotificationCenter.Current.Show(request);
        NotificationsFlyInPanel.IsVisible = false;

        await DisplayAlert("Success!", "Reminder set!", "OK");

        // Animate panel out
        await Task.WhenAll(
            NotificationsFlyInPanel.FadeTo(0, 200, Easing.CubicIn),
            NotificationsFlyInPanel.TranslateTo(0, 500, 200, Easing.CubicIn));

        NotificationNameEntry.Text = null;
        NotificationDescriptionEditor.Text = null;
        NotificationDatePicker.Date = DateTime.Today;
        NotificationTimePicker.Time = new TimeSpan(0, 0, 0);  // Midnight
        NotificationsFlyInPanel.IsVisible = false;
    }
    private async void SaveNotesButton_Clicked(object sender, EventArgs e)
    {
        var notes = NotesEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(notes))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation Error", "Notes cannot be empty.", "OK");
            return;
        }

        _passedCourse.Notes = notes;
        await _dbService.UpdateUserDegreeCourseAsync(_passedCourse,
                                                     _passedCourse.TermId);

        await Application.Current.MainPage.DisplayAlert(
            "Success", "Your notes have been saved.", "OK");
    }
}