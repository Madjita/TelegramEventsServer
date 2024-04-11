using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Entities.Entities_DBContext;

[Table("telegram_bot_types")]
public class TelegramBotTypes : EntityBase
{
    [Column("telegram_bot_type_name")]
    public string TelegramBotTypeName { get; set; } = string.Empty;
    
    [Column("telegram_bot_type_issystem")]
    public bool TelegramBotTypeIsSystem { get; set; }
    
    public ICollection<TelegramBots> TelegramBots { get; set; }
}