using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("user")]
    public class User : EntityBase
    {
        [Column("user_id")]
        public override int Id { get => base.Id; set => base.Id = value; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("telegram_chat_id")]
        public long TelegramChatId { get; set; }

        [Column("user_name")]
        public string UserName { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("middle_name")]
        public string? MiddleName { get; set; }

        [Column("message_id")]
        public int MessageId { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("h_password")]
        public string? HPassword { get; set; }

        [Column("bot_admin")]
        public bool BotAdmin { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
        
        [Column("role")]
        public string Role { get; set; }

        public int? WeatherSubscriberId { get; set; }

        public WeatherSubscribers WeatherSubscriber { get; set; }

    }
}
