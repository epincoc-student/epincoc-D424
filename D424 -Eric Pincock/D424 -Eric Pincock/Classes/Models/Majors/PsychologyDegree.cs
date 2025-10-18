using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models.Majors
{
    public class PsychologyDegree : Degree
    {
        public PsychologyDegree()
        {
            DegreeId = 4;
            DegreeName = "Psychology";
            DegreeType = "Psychology";

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


            new DegreeCourses { CourseId = 401 },
            new DegreeCourses { CourseId = 402 },
            new DegreeCourses { CourseId = 403 },
            new DegreeCourses { CourseId = 404 },
            new DegreeCourses { CourseId = 405 },
            new DegreeCourses { CourseId = 406 },
            new DegreeCourses { CourseId = 407 },
            new DegreeCourses { CourseId = 408 },
            new DegreeCourses { CourseId = 409 },
            new DegreeCourses { CourseId = 410 }
        };
        }
    }

}
