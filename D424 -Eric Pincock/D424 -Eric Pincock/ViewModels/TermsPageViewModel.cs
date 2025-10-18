using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.Classes.Models;
using SQLite;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace D424__Eric_Pincock.ViewModels
{
    public class TermsPageViewModel : INotifyPropertyChanged
    {
        private readonly LocalDbService _dbService = new();
        private int _userId;
        private int _degreeId;

        public ObservableCollection<UserTerm> UserTerms { get; set; } = new();
        public ObservableCollection<UserDegreeCourse> UserCourses { get; set; } = new();

        public Command<UserTerm> SelectTermCommand { get; }
        public Command<UserTerm> EditTermCommand { get; }

        private int _totalCredits;
        public int TotalCredits
        {
            get => _totalCredits;
            set
            {
                if (_totalCredits != value)
                {
                    _totalCredits = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _remainingCredits;
        public int RemainingCredits
        {
            get => _remainingCredits;
            set
            {
                if (_remainingCredits != value)
                {
                    _remainingCredits = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _progressPercent;
        public int ProgressPercent
        {
            get => _progressPercent;
            set
            {
                if (_progressPercent != value)
                {
                    _progressPercent = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _progressRatio;
        public double ProgressRatio
        {
            get => _progressRatio;
            set
            {
                if (_progressRatio != value)
                {
                    _progressRatio = value;
                    OnPropertyChanged();
                }
            }
        }

        public TermsPageViewModel(int userId, int degreeId)
        {
            _userId = userId;
            _degreeId = degreeId;

            SelectTermCommand = new Command<UserTerm>(term => OnTermSelected(term));
            EditTermCommand = new Command<UserTerm>(term => OnEditTerm(term));
        }

        public UserTerm SelectedTerm { get; set; }

        private void OnTermSelected(UserTerm term)
        {
            SelectedTerm = term;
            TermSelected?.Invoke(this, term);
        }

        public event EventHandler<UserTerm> TermSelected;

        private void OnEditTerm(UserTerm term)
        {
            SelectedTerm = term;
            TermEditRequested?.Invoke(this, term);
        }

        public event EventHandler<UserTerm> TermEditRequested;

        public async Task LoadUserCoursesAsync()
        {
            var courses = await _dbService.GetUserDegreeCoursesAsync(_userId, _degreeId);
            UserCourses.Clear();
            foreach (var course in courses)
                UserCourses.Add(course);
        }

        public async Task LoadUserTermsAsync()
        {
            var terms = await _dbService.GetUserTermsAsync(_userId);
            UserTerms.Clear();

            int creditsSum = 0;

            foreach (var term in terms)
            {
                var courses = await _dbService.GetCoursesByTermAsync(_userId, term.TermId);

                foreach (var course in courses)
                {
                    course.IsFromDifferentDegree = course.DegreeId != _degreeId;
                }
                creditsSum += courses.Sum(c => c.CourseCredits);

                term.SelectedDegreeId = _degreeId;
                term.AssociatedCourses = new ObservableCollection<UserDegreeCourse>(courses);
                UserTerms.Add(term);
            }
            TotalCredits = creditsSum;
        }

        public async Task UpdateCreditProgressAsync()
        {
            var userCourses = (await _dbService.GetUserDegreeCoursesAsync(_userId, _degreeId))
                .Where(c => c.DegreeId == _degreeId)
                .ToList();

            var degreeRecords = await _dbService.GetAllDegreesAsync();
            var selectedRecord = degreeRecords.FirstOrDefault(d => d.DegreeId == _degreeId);

            if (selectedRecord == null)
            {
                RemainingCredits = 0;
                ProgressPercent = 0;
                ProgressRatio = 0;
                return;
            }

            var fullDegree = await _dbService.GetDegreeWithCoursesAsync(selectedRecord);
            int requiredCredits = fullDegree.DegreeCourses.Sum(dc => dc.Course.CourseCredits);
            int completedCredits = userCourses.Where(c => c.Status == "Completed").Sum(c => c.CourseCredits);
            int remainingCredits = userCourses.Where(c => c.Status != "Completed").Sum(c => c.CourseCredits);

            RemainingCredits = remainingCredits;
            ProgressRatio = requiredCredits == 0 ? 0 : (double)completedCredits / requiredCredits;
            ProgressPercent = (int)(ProgressRatio * 100);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}