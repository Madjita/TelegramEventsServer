using System.Globalization;
using CQRS.Query;
using DataBase.Entities.Entities_DBContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using TelegramBot.TelegramBotFactory.TelegramBotDto;
using User = DataBase.Entities.Entities_DBContext.User;

namespace TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

public class WriteEventCommandContextStrategy : BaseCommandContextStrategy
{
    public WriteEventCommandContextStrategy(
        Message message,
        ITelegramBotClient botClient,
        IMediator mediator,
        TelegramBotMessageContext currentContextMessage,
        IServiceScopeFactory scopeFactory,
        ITelegramBotFacade telegramBotFacade
        ):
        base(message, botClient, mediator, currentContextMessage, scopeFactory, telegramBotFacade)
    {
        
    }
    
    public override async Task<CommandContextStrategyResponseDto> Execute(User telegramUser, string messageText, CancellationToken cancellationToken)
    {
        var responseDto = new CommandContextStrategyResponseDto();
        EventFromUserDto? eventFromUserDto = null;
        
        //Сохранить событие после того как пользователь введет данные
        // Ищем подстроки после ":"
        string[] parts = messageText.ReplaceLineEndings(string.Empty).Split(';');

        // Проверяем, что есть хотя бы две части (название бота и токен)
        if (parts.Length >= 6)
        {
             eventFromUserDto = new EventFromUserDto
             {
                 EventName = parts[0],
                 EventStartData = parts[1],
                 Price = parts[2],
                 PriceWithRef = parts[3],
                 PriceInDayParty = parts[4],
                 FreeEvent = parts[5],
                 ManagerTitleChannel = parts[6]
             };
        } 
        if(eventFromUserDto is null)
            return responseDto;
        
        var date = DateTime.ParseExact(eventFromUserDto.EventStartData, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                            
        var updateEventsCommand = new UpdateEventsCommand
        {
             newEvent = new Event()
             {
                StartDate = date.ToUniversalTime(),
                Name = eventFromUserDto.EventName,
                Price = Convert.ToInt16(eventFromUserDto.Price),
                PriceWithRef = Convert.ToInt16(eventFromUserDto.PriceWithRef),
                PriceInDayParty = Convert.ToInt16(eventFromUserDto.PriceInDayParty),
                FreeForNoDanser =
                    eventFromUserDto.FreeEvent.Equals("да", StringComparison.OrdinalIgnoreCase),
                ManagerTitleChannel = eventFromUserDto.ManagerTitleChannel,
                TelegramBotId = _currentContextMessage.SelectTelegramBot.Id 
             }
        }; 
        var checkUserAdministrator = await _mediator.Send(updateEventsCommand, cancellationToken);
        
        List<List<InlineKeyboardButton>> buttons = new();
        TelegramBot.AddButtonCansel(buttons);
        _inlineKeyboard = new InlineKeyboardMarkup(buttons);
        
        if (checkUserAdministrator.Success)
        {
            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                                 chatId: telegramUser.TelegramChatId,
                                 text: "Создание события прошло успешно.",
                                 replyMarkup: _inlineKeyboard,
                                 cancellationToken: cancellationToken);
        }
        else
        {
            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                                 chatId: telegramUser.TelegramChatId,
                                 text: "Произошла ошибка при сохранении события.",
                                 replyMarkup: _inlineKeyboard,
                                 cancellationToken: cancellationToken);
        }
         
        responseDto.UsePrevCommand = true;
        responseDto.DeleteCurrentContext = true;
        
        ResponceTelegramFacade responce;
                
        //Ищем пользователя.
        var responceUser = await MainManagerFaindUser(telegramUser);
        if (responceUser is null)
        {
            responce =  new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated);
        }
        else
        {
            var keyboard = _telegramBotFacade.StartManagerButtons(_mediator, responceUser);
        
            responce =  new ResponceTelegramFacade(TelegramUserState.StartRegistrationOrganization, true)
            {
                Message = "Здравствуйте.\n"+
                          "Я бот менеджер который поможет вам зарегестрировать вашу компанию в системе и создать события.",
                InlineKeyboard = keyboard
            };
        }
                
        var responceText = responce.Message ?? "Ошибка";

        responseDto.SentMessage = await _botClient.SendTextMessageAsync(
            chatId: telegramUser.TelegramChatId ,
            text: responceText,
            cancellationToken: cancellationToken,
            replyMarkup: responce.InlineKeyboard);
        
        return responseDto;
    }
}