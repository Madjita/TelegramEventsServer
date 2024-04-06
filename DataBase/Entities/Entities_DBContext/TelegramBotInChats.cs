using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("telegram_bot_in_chat")]
    public class TelegramBotInChats : EntityBase
    {
        [Column("telegram_chat_id")]
        public long TelegramChatId { get; set; }

        [Column("chat_title")]
        public string ChatTitle { get; set; } = string.Empty;

        [Column("chat_type")]
        public string ChatType { get; set; } = string.Empty;

        [Column("chat_status")]
        public string ChatStatus { get; set; } = string.Empty;

        [Column("telegram_bot_id")]
        public int? TelegramBotId { get; set; }

        [Column("event_id")]
        public int? EventId { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
