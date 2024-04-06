using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("x_org_user")]
    public class XOrgUser : EntityBase
    {
        [Column("org_id")]
        public int OrgId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
