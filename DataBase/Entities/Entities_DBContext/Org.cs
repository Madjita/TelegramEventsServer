using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("org")]
    public class Org : EntityBase
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("rights")]
        public string? Rights { get; set; }

        [Column("who_registerd_user_id")]
        public int WhoRegisterdUsserId { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
