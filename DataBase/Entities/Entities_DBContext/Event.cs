using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("event")]
    public  class Event : EntityBase
    {
        [Column("telegram_chat_id")]
        public long TelegramChatId { get; set; }

        [Column("start_Date")]
        public long StartDate { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("price")]
        public int Price { get; set; }

        [Column("price_with_ref")]
        public int PriceWithRef { get; set; }

        [Column("price_in_day_party")]
        public int PriceInDayParty { get; set; }

        [Column("free_for_no_danser")]
        public bool FreeForNoDanser { get; set; }
        
        [Column("telegram_bot_id")]
        public int TelegramBotId { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        public TelegramBots TelegramBot { get; set; }
    }
}
