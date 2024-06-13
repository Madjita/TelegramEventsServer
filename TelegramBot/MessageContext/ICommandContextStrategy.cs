using System.Collections.Concurrent;
using MediatR;
using Telegram.Bot.Types;
using User = DataBase.Entities.Entities_DBContext.User;

namespace TelegramBot.MessageContext;

public interface ICommandContextStrategy
{
    public Task<CommandContextStrategyResponseDto> Execute(User telegramUser, string messageText, CancellationToken cancellationToken);
}

public class CommandContextStrategyResponseDto
{
    public Message? SentMessage;
    public bool DeleteCurrentContext;
    public bool UsePrevCommand;
}