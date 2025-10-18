using D424__Eric_Pincock.Classes.Models.Majors;
using System.Collections.Generic;
using System.Linq;

namespace D424__Eric_Pincock.Classes.Models
{
    public static class DegreeCatalog
    {
        public static List<Degree> AllDegrees => new()
        {
            new ComputerScienceDegree(),
            new StatisticsDegree(),
            new BusinessAdministrationDegree(),
            new PsychologyDegree(),
            new BiologyDegree()
            // Add more subclasses here as needed
        };
        public static Degree GetById(int degreeId)
        {
            return AllDegrees.FirstOrDefault(d => d.DegreeId == degreeId);
        }
        public static Degree GetByType(string degreeType)
        {
            return AllDegrees.FirstOrDefault(d => d.DegreeType == degreeType);
        }
    }
}