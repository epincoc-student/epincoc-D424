using SQLite;

namespace D424__Eric_Pincock.Classes.Models
{
    public abstract class Degree
    {
        [PrimaryKey]
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
        public string DegreeType { get; set; }
        [Ignore]
        public List<DegreeCourses> DegreeCourses { get; set; }
        [Ignore]
        public int TotalCredits => DegreeCourses?.Sum(dc => dc.Course?.CourseCredits ?? 0) ?? 0;

    }
}
