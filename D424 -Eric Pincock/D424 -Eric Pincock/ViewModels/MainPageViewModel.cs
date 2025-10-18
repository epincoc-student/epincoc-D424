using D424__Eric_Pincock.Classes.Models;
using D424__Eric_Pincock.Classes.Models.Majors;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace D424__Eric_Pincock.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DegreeRecord> Degrees { get; set; } = new();

        private DegreeRecord _selectedDegree;
        public DegreeRecord SelectedDegree
        {
            get => _selectedDegree;
            set
            {
                if (_selectedDegree != value)
                {
                    _selectedDegree = value;
                    OnPropertyChanged();
                    _ = LoadDegreeDetailsAsync();
                }
            }
        }

        private Degree _activeDegree;
        public Degree ActiveDegree
        {
            get => _activeDegree;
            set
            {
                _activeDegree = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadDegreesAsync()
        {
            IsLoading = true;

            var service = new LocalDbService();
            var records = await service.GetAllDegreesAsync();

            Degrees.Clear();
            foreach (var record in records)
            {
                Degrees.Add(record);

                // Preload course data for each degree
                _ = PreloadDegreeDetailsAsync(record);
            }

            IsLoading = false;
        }

        private Dictionary<int, Degree> _degreeCache = new();

        private async Task PreloadDegreeDetailsAsync(DegreeRecord record)
        {
            var service = new LocalDbService();
            var degree = await service.GetDegreeWithCoursesAsync(record);

            if (degree != null && !_degreeCache.ContainsKey(degree.DegreeId))
            {
                _degreeCache[degree.DegreeId] = degree;
            }
        }

        private async Task LoadDegreeDetailsAsync()
        {
            if (SelectedDegree == null)
                return;

            if (_degreeCache.TryGetValue(SelectedDegree.DegreeId,
                                         out var cachedDegree))
            {
                ActiveDegree = cachedDegree;
            }
            else
            {
                var service = new LocalDbService();
                var degree =
                    await service.GetDegreeWithCoursesAsync(SelectedDegree);
                ActiveDegree = degree;

                if (degree != null)
                    _degreeCache[degree.DegreeId] = degree;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}