using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.UserCourseDetails
{
    [Table("PerformanceAssessment")]
    public class PerformanceAssessment
    {
        [PrimaryKey, AutoIncrement]
        public int PerformanceAssessmentId { get; set; }
        [Indexed]
        public int CourseId { get; set; }
        public string PerformanceAssessmentName { get; set; }
        public DateTime paStartDate { get; set; }
        public DateTime paEndDate { get; set; }
    }
}
