using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.Majors
{
    public class StatisticsDegree : Degree
    {
        public StatisticsDegree()
        {
            DegreeId = 2;
            DegreeName = "Statistics";
            DegreeType = "Statistics";

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


            new DegreeCourses { CourseId = 201 },
            new DegreeCourses { CourseId = 202 },
            new DegreeCourses { CourseId = 203 },
            new DegreeCourses { CourseId = 204 },
            new DegreeCourses { CourseId = 205 },
            new DegreeCourses { CourseId = 206 },
            new DegreeCourses { CourseId = 207 },
            new DegreeCourses { CourseId = 208 },
            new DegreeCourses { CourseId = 209 },
            new DegreeCourses { CourseId = 210 }
        };
        }
    }

}
