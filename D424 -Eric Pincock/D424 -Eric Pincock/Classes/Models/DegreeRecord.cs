using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes.Models
{
    [Table("DegreeRecord")]
    public class DegreeRecord
    {
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
        public string DegreeType { get; set; }
    }
}
