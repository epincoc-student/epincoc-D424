using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.UserCourseDetails
{
    [Table("ObjectiveAssessment")]
    public class ObjectiveAssessment
    {
        [PrimaryKey, AutoIncrement]
        public int ObjectiveAssessmentId { get; set; }
        [Indexed]
        public int CourseId { get; set; }
        public string   ObjectiveAssessmentName { get; set; }
        public DateTime oaStartDate { get; set; }
        public DateTime oaEndDate { get; set; }
    }
}
