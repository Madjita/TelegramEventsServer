using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using User = DataBase.Entities.Entities_DBContext.User;

namespace TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

public class RegistrationPartyCommandContextStrategy : BaseCommandContextStrategy
{
    public RegistrationPartyCommandContextStrategy(
        Message message,
        ITelegramBotClient botClient, 
        IMediator mediator,
        TelegramBotMessageContext currentContextMessage,
        IServiceScopeFactory scopeFactory,
        ITelegramBotFacade telegramBotFacade
        ) :
        base(message, botClient, mediator, currentContextMessage, scopeFactory, telegramBotFacade)
    {

    }

    public override async Task<CommandContextStrategyResponseDto> Execute(User telegramUser, string messageText,
        CancellationToken cancellationToken)
    {
        var responseDto = new CommandContextStrategyResponseDto();
        
        return responseDto;
    }
}