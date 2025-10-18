using SQLite;
using System;

namespace D424__Eric_Pincock.Classes
{
    [Table("UserDegreeCourse")]
    public class UserDegreeCourse
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int UserId { get; set; } // Foreign Key to User
        [Indexed]
        public int DegreeId { get; set; } = 0;
        public int TermId { get; set; } = 0; 
        public int CourseId { get; set; } 
        public string CourseName { get; set; }
        public int CourseCredits { get; set; }
        public bool PreReqsNeeded { get; set; }
        public int PreReqCourses { get; set; }
        [Ignore]
        public string PreReqName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        [Ignore]
        public string DisplayCourseWithCredits => $"{CourseName} - Credits: {CourseCredits}";
        [Ignore]
        public bool IsFromDifferentDegree { get; set; }
        [Ignore]
        public string DisplayStartDate =>
            StartDate > DateTime.MinValue ? $"Start: {StartDate:MMM dd, yyyy}" : string.Empty;
        [Ignore]
        public string DisplayEndDate =>
            EndDate > DateTime.MinValue ? $"End: {EndDate:MMM dd, yyyy}" : string.Empty;
    }
}