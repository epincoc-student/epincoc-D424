using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.Majors
{
    public class BusinessAdministrationDegree : Degree
    {
        public BusinessAdministrationDegree()
        {
            DegreeId = 3;
            DegreeName = "Business Administration";
            DegreeType = "Business Administration";

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

            new DegreeCourses { CourseId = 301 },
            new DegreeCourses { CourseId = 302 },
            new DegreeCourses { CourseId = 303 },
            new DegreeCourses { CourseId = 304 },
            new DegreeCourses { CourseId = 305 },
            new DegreeCourses { CourseId = 306 },
            new DegreeCourses { CourseId = 307 },
            new DegreeCourses { CourseId = 308 },
            new DegreeCourses { CourseId = 309 },
            new DegreeCourses { CourseId = 310 }
        };
        }
    }

}
