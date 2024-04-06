using Couchbase.Core.IO.Operations.Errors;
using DataBase.Entities.Entities_DBContext;
using TelegramBot.Facade;

namespace TelegramBot.TelegramBotFactory.TelegramBotDto;

public class UserTBDto
{
    public readonly User? User;
    public readonly TelegramUserErrorCode ErrorCode;
    
    public UserTBDto(User? user,TelegramUserErrorCode errorCode)
    {
        User = user;
        ErrorCode = errorCode;
    }

}