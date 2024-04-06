using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("role")]
    public class Role : EntityBase
    {
        [Column("role_name")]
        public string RoleName { get; set; }
    }
}
