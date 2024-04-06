using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("x_event_user")]
    public class XEventUser : EntityBase
    {
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("who_reg_user_id")]
        public int WhoRegUserId { get; set; }

        [Column("is_deleted")]
        public int IsDeleted { get; set; }
    }
}
