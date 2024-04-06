using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("telegram_bot_message")]
    public class TelegramBotMessage : EntityBase
    {
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("state_id")]
        public int StateId { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
