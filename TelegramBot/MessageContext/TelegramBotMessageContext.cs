using DataBase.Entities.Entities_DBContext;
using System.Collections.Concurrent;
using TelegramBot.Facade;

namespace TelegramBot.MessageContext
{
    public class UserInfo
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string MiddleName { get; set; }
        public string Phone { get; set; }

        public string FIO => $"{FirstName} {SecondName} {MiddleName}";
    }

    public class TelegramBotMessageContext
    {
        public TelegramUserState State { get; set; }
        public TelegramUserErrorCode ErrorCode { get; set; }
        public string? LastCommand { get; set; }
        public string? LastMessage { get; set; }
        public User TelegramUser { get; set; }
        public Org Org { get; set; }
        public DataBase.Entities.Entities_DBContext.TelegramBots SelectTelegramBot { get; set; }
        public int TelegramBotTypeId  { get; set; }
        public UserInfo UserInfo { get; set; }
        
        public static MessageContext.TelegramBotMessageContext CreateOrReplaseTelegramBotMessageContext(TelegramBotMessageContext? oldContext, TelegramBotMessageContext newContext, ConcurrentDictionary<long, TelegramBotMessageContext> _messageContexts)
        {
            if (oldContext is null)
            {
                _messageContexts.TryAdd(newContext.TelegramUser.TelegramChatId, newContext);
            }

            return newContext;
        }
    }
}
