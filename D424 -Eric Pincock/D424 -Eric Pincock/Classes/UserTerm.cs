using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes
{
    [Table("UserTerm")]
    public class UserTerm
    {
        [PrimaryKey, AutoIncrement]
        public int TermId { get; set; }
        [Indexed]
        public string TermName { get; set; }
        public int UserId { get; set; } // Foreign Key to User
        public DateTime TermStartDate { get; set; }
        public DateTime TermEndDate { get; set; }
        public string TermStatus { get; set; }
        [Ignore]
        public int SelectedDegreeId { get; set; }
        [Ignore]
        public ObservableCollection<UserDegreeCourse> AssociatedCourses { get; set; } = new();
        [Ignore]
        public int TotalCredits => AssociatedCourses?
            .Where(c => c.DegreeId == SelectedDegreeId)
            .Sum(c => c.CourseCredits) ?? 0;
    }
}
