using System.Drawing.Imaging;
using CQRS.Query;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using User = DataBase.Entities.Entities_DBContext.User;

namespace TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

public abstract class BaseCommandContextStrategy : ICommandContextStrategy
{
    protected readonly Message _message;
    protected readonly ITelegramBotClient _botClient;
    protected readonly IMediator _mediator;
    protected readonly TelegramBotMessageContext _currentContextMessage;
    protected readonly IServiceScopeFactory _scopeFactory;
    protected readonly ITelegramBotFacade _telegramBotFacade;
    
    public InlineKeyboardMarkup? _inlineKeyboard { get; set; }
    public BaseCommandContextStrategy(
        Message message,
        ITelegramBotClient botClient, 
        IMediator mediator, 
        TelegramBotMessageContext currentContextMessage,
        IServiceScopeFactory scopeFactory,
        ITelegramBotFacade telegramBotFacade)
    {
        _message = message;
        _botClient = botClient;
        _mediator = mediator;
        _currentContextMessage = currentContextMessage;
        _scopeFactory = scopeFactory;
        _telegramBotFacade = telegramBotFacade;
    }
    
    protected async Task<User?> MainManagerFaindUser(User? requestTelegramUser)
    {
        var responceUser = await FindUser(requestTelegramUser, true);
        if (responceUser is null)
        {
            return await FindUser(requestTelegramUser);
        }
        return responceUser;
    }
    
    public async Task<User?> FindUser(User requestTelegramUser, bool isDeleted = false)
    {
        //Ищем пользователя.
        var cqrsUser = new GetUserByChatIdQuery
            { TelegramChatId = requestTelegramUser.TelegramChatId, IsDeleted = isDeleted };
        var responceUser = await _mediator.Send(cqrsUser);

        return responceUser;
    }
    
    public abstract Task<CommandContextStrategyResponseDto> Execute(User telegramUser, string messageText, CancellationToken cancellationToken);
}