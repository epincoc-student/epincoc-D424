using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.Classes.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace D424__Eric_Pincock.ViewModels
{
    public class TermsDetailsViewModel : INotifyPropertyChanged
    {
        private readonly LocalDbService _dbService = new();
        private readonly int _userId;
        private readonly int _degreeId;
        private readonly int _termId;

        public ObservableCollection<UserDegreeCourse> TermCourses
        {
            get; set;
        } = new();
        public ObservableCollection<UserDegreeCourse> AllUserCourses { get; set; } = new();
        public ObservableCollection<UserDegreeCourse> FilteredCourses { get; set; } = new();
        public Command<UserDegreeCourse> TapCourseCommand { get; }
        public Command<UserDegreeCourse> EditCourseCommand { get; }
        public Command<UserDegreeCourse> SearchResultTappedCommand { get; }

        public int TotalCredits => TermCourses?.Sum(c => c.CourseCredits) ?? 0;

        public TermsDetailsViewModel(int userId, int degreeId, int termId)
        {
            _userId = userId;
            _degreeId = degreeId;
            _termId = termId;

            TapCourseCommand = new Command<UserDegreeCourse>(OnCourseTapped);
            EditCourseCommand =
                new Command<UserDegreeCourse>(OnCourseEditRequested);
            SearchResultTappedCommand =
                new Command<UserDegreeCourse>(OnSearchResultTapped);

            _ = InitializeAsync();  // fire-and-forget async init
        }

        private async Task InitializeAsync()
        {
            await LoadAllUserCoursesAsync();
            await LoadCoursesForTermAsync();
        }
        public UserDegreeCourse SelectedCourse { get; set; }

        public event EventHandler<UserDegreeCourse> CourseTapped;

        public event EventHandler<UserDegreeCourse> SearchResultTapped;

        private void OnCourseTapped(UserDegreeCourse course)
        {
            SelectedCourse = course;
            CourseTapped?.Invoke(this, course);
        }

        private void OnSearchResultTapped(UserDegreeCourse course)
        {
            SelectedCourse = course;
            SearchResultTapped?.Invoke(this, course);
        }

        public event EventHandler<UserDegreeCourse> CourseEditRequested;

        private async Task LoadCoursesForTermAsync()
        {
            var courses = await _dbService.GetCoursesByTermAsync(_userId, _termId);

            TermCourses.Clear();

            foreach (var course in courses)
            {
                // Lookup prerequisite name from AllUserCourses
                var prereq = AllUserCourses.FirstOrDefault(
                    c => c.CourseId == course.PreReqCourses);
                course.PreReqName = prereq?.CourseName ?? "None";

                TermCourses.Add(course);
            }

            OnPropertyChanged(nameof(TotalCredits));
        }
        private async Task LoadAllUserCoursesAsync()
        {
            var allCourses =
                await _dbService.GetUserDegreeCoursesAsync(_userId, _degreeId);
            AllUserCourses.Clear();

            foreach (var course in allCourses)
            {
                // Lookup the prerequisite course name
                var prereq = allCourses.FirstOrDefault(c => c.CourseId ==
                                                            course.PreReqCourses);
                course.PreReqName = prereq?.CourseName ?? "None";

                AllUserCourses.Add(course);
            }
        }

        private void OnCourseEditRequested(UserDegreeCourse course)
        {
            SelectedCourse = course;
            CourseEditRequested?.Invoke(this, course);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void FilterCourses(string keyword)
        {
            keyword = keyword?.ToLower() ?? "";

            var filtered =
                AllUserCourses
                    .Where(c => !string.IsNullOrEmpty(c.CourseName) &&
                                c.CourseName.ToLower().Contains(keyword))
                    .ToList();

            FilteredCourses.Clear();
            foreach (var course in filtered) FilteredCourses.Add(course);
        }

        public async void RefreshTermCourses()
        {
            var courses = await _dbService.GetCoursesByTermAsync(_userId, _termId);
            TermCourses.Clear();
            foreach (var course in courses) TermCourses.Add(course);

            OnPropertyChanged(nameof(TotalCredits));
        }
    }
}