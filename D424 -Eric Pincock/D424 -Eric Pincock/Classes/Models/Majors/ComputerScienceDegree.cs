using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.Majors
{
    public class ComputerScienceDegree : Degree
    {
        public ComputerScienceDegree()
        {
            DegreeId = 1;
            DegreeName = "Computer Science";
            DegreeType = "CS";

            DegreeCourses = new List<DegreeCourses>
        {
            new DegreeCourses { CourseId = 601 },
            new DegreeCourses { CourseId = 602 },
            new DegreeCourses { CourseId = 603 },
            new DegreeCourses { CourseId = 604 },
            new DegreeCourses { CourseId = 605 },
            new DegreeCourses { CourseId = 606 },
            new DegreeCourses { CourseId = 607 },
            new DegreeCourses { CourseId = 608 },
            new DegreeCourses { CourseId = 609 },
            new DegreeCourses { CourseId = 610 },


            new DegreeCourses { CourseId = 101 },
            new DegreeCourses { CourseId = 102 },
            new DegreeCourses { CourseId = 103 },
            new DegreeCourses { CourseId = 104 },
            new DegreeCourses { CourseId = 105 },
            new DegreeCourses { CourseId = 106 },
            new DegreeCourses { CourseId = 107 },
            new DegreeCourses { CourseId = 108 },
            new DegreeCourses { CourseId = 109 },
            new DegreeCourses { CourseId = 110 }
        };
        }
    }

}
