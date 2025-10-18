using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using D424__Eric_Pincock.Classes;

namespace D424__Eric_Pincock.ViewModels
{
    public class UserReportsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private ObservableCollection<UserDegreeCourse> _reportResults = new();
        public ObservableCollection<UserDegreeCourse> ReportResults
        {
            get => _reportResults;
            set
            {
                _reportResults = value;
                OnPropertyChanged();
            }
        }

        public List<string> ReportOptions
        {
            get;
        } = new() { "Current Degree", "Remaining Courses" };

        private string _selectedReportType;
        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                _selectedReportType = value;
                OnPropertyChanged();
            }
        }

        private readonly LocalDbService _dbService;
        private readonly int _userId;
        private readonly int _degreeId;

        public UserReportsPageViewModel(int userId, int degreeId)
        {
            _userId = userId;
            _degreeId = degreeId;
            _dbService = new LocalDbService();
        }

        public async Task LoadReportAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedReportType))
                return;

            var allCourses =
                await _dbService.GetUserDegreeCoursesAsync(_userId, _degreeId);
            List<UserDegreeCourse> results;

            switch (SelectedReportType)
            {
                case "Current Degree":
                    results = allCourses;
                    break;

                case "Remaining Courses":
                    results =
                        allCourses.Where(c => c.Status != "Completed").ToList();
                    break;

                default:
                    results = new List<UserDegreeCourse>();
                    break;
            }

            // Populate PreReqName for each course
            foreach (var course in results)
            {
                var prereq = allCourses.FirstOrDefault(c => c.CourseId ==
                                                            course.PreReqCourses);
                course.PreReqName = prereq?.CourseName ?? "None";
            }

            ReportResults = new ObservableCollection<UserDegreeCourse>(results);
        }
    }
}