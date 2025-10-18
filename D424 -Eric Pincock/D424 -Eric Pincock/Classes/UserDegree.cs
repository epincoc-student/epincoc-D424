using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D424__Eric_Pincock.Classes
{
    [Table("UserDegree")]
    public class UserDegree
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int UserId { get; set; } // Foreign Key to User
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
    }
}
