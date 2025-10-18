using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.Majors
{
    public class BiologyDegree : Degree
    {
        public BiologyDegree()
        {
            DegreeId = 5;
            DegreeName = "Biology";
            DegreeType = "Bio";

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
            new DegreeCourses { CourseId = 501 },
            new DegreeCourses { CourseId = 502 },
            new DegreeCourses { CourseId = 503 },
            new DegreeCourses { CourseId = 504 },
            new DegreeCourses { CourseId = 505 },
            new DegreeCourses { CourseId = 506 },
            new DegreeCourses { CourseId = 507 },
            new DegreeCourses { CourseId = 508 },
            new DegreeCourses { CourseId = 509 },
            new DegreeCourses { CourseId = 510 }
        };
        }
    }

}
