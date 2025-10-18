using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models
{
    [Table("DegreeCourse")]
    public class DegreeCourses
    {
        public int DegreeCoursesId { get; set; }
        public int DegreeId { get; set; }
        public int CourseId { get; set; }
        [Ignore]
        public Degree Degree { get; set; }
        [Ignore]
        public Course Course { get; set; }
    }
}
