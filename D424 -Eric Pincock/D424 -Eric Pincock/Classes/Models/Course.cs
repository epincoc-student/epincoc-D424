using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models
{
    [Table("Course")]
    public class Course
    {
        [PrimaryKey]
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int CourseCredits { get; set; }
        public bool PreReqsNeeded { get; set; }
        public int PreReqCourses { get; set; }
        [Ignore]
        public List<DegreeCourses> DegreeCourses { get; set; }
        [Ignore]
        public Course RequiredCourses { get; set; }

    }
}
