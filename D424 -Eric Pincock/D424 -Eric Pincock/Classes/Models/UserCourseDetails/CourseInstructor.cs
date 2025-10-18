using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.UserCourseDetails
{
    [Table("CourseInstructor")]
    public class CourseInstructor
    {
        [PrimaryKey, AutoIncrement]
        public int CourseInstructorId { get; set; }
        [Indexed]
        public int CourseId { get; set; }
        public string CourseInstructorName { get; set; }

        public string CourseInstructorEmail { get; set; }
        public string CourseInstructorPhone { get; set; }

    }
}
