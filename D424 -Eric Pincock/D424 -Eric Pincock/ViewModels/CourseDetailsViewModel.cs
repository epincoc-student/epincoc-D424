using D424__Eric_Pincock.Classes;
using D424__Eric_Pincock.Classes.Models;
using D424__Eric_Pincock.Classes.Models.UserCourseDetails;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace D424__Eric_Pincock.ViewModels
{
    public class CourseDetailsViewModel : INotifyPropertyChanged
    {
        private readonly LocalDbService _dbService = new();
        private CourseInstructor _instructor;
        private ObjectiveAssessment _objectiveAssesment;
        private PerformanceAssessment _performanceAssessment;
        private readonly int _userId;

        public UserDegreeCourse Course { get; set; }
        public CourseInstructor Instructor
        {
            get => _instructor;
            set
            {
                _instructor = value;
                OnPropertyChanged(nameof(Instructor));
                OnPropertyChanged(nameof(HasInstructor));
                OnPropertyChanged(nameof(ShowAddInstructorButton));
            }
        }

        public bool HasInstructor => Instructor != null;
        public bool ShowAddInstructorButton => !HasInstructor;

        public ObservableCollection<ObjectiveAssessment> ObjectiveAssessments
        {
            get; set;
        } = new();

        private ObjectiveAssessment _objectiveAssessment;
        public ObjectiveAssessment ObjectiveAssessment
        {
            get => _objectiveAssessment;
            set
            {
                _objectiveAssessment = value;
                OnPropertyChanged(nameof(ObjectiveAssessment));
                OnPropertyChanged(nameof(HasObjectiveAssessment));
                OnPropertyChanged(nameof(ShowAddObjectiveAssessmentButton));
            }
        }

        public bool HasObjectiveAssessment => ObjectiveAssessment != null;
        public bool ShowAddObjectiveAssessmentButton => !HasObjectiveAssessment;

        public ObservableCollection<PerformanceAssessment>
            PerformanceAssessments
        {
            get; set;
        } = new();

        private PerformanceAssessment _performanceassessment;

        public PerformanceAssessment PerformanceAssessment
        {
            get => _performanceAssessment;
            set
            {
                _performanceAssessment = value;
                OnPropertyChanged(nameof(PerformanceAssessment));
                OnPropertyChanged(nameof(HasPerformanceAssessment));
                OnPropertyChanged(nameof(ShowAddPerformanceAssessmentButton));
            }
        }

        public bool HasPerformanceAssessment => PerformanceAssessment != null;
        public bool ShowAddPerformanceAssessmentButton =>
            !HasPerformanceAssessment;

        public Command SaveNotesCommand { get; }

        public CourseDetailsViewModel(int userId, UserDegreeCourse course)
        {
            _userId = userId;
            Course = course;

            SaveNotesCommand = new Command(async () => await SaveNotesAsync());
            _ = LoadRelatedDataAsync();
        }

        public async Task LoadRelatedDataAsync()
        {
            Instructor =
                await _dbService.GetInstructorByCourseIdAsync(Course.CourseId);

            var performance =
                await _dbService.GetPerformanceAssessmentByCourseIdAsync(
                    Course.CourseId);
            PerformanceAssessments.Clear();
            if (performance != null)
                PerformanceAssessments.Add(performance);

            var objective = await _dbService.GetObjectiveAssessmentByCourseIdAsync(
                Course.CourseId);
            ObjectiveAssessments.Clear();
            if (objective != null)
            {
                ObjectiveAssessments.Add(objective);
                ObjectiveAssessment = objective;
            }
            else
            {
                ObjectiveAssessment = null;
            }

            OnPropertyChanged(nameof(HasObjectiveAssessment));
        }
        private async Task SaveNotesAsync()
        {
            await _dbService.UpdateUserDegreeCourseAsync(Course, Course.TermId);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
