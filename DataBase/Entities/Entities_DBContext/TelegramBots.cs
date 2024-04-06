using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext;

[Table("telegram_bots")]
public class TelegramBots : EntityBase
{
    [Column("telegram_bot_name")]
    public string TelegramBotName { get; set; } = string.Empty;

    [Column("telegram_bot_token")]
    public string TelegramBotToken { get; set; } = string.Empty;

    [Column("telegram_bot_type")]
    public string TelegramBotType { get; set; }

    [Column("org_id")]
    public int OrgId { get; set; }
}
