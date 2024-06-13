using CQRS.Query;
using DataBase.Entities.Entities_DBContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using User = DataBase.Entities.Entities_DBContext.User;

namespace TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

public class WriteAdministratorNikNameForAddInOrgCommandContextStrategy : BaseCommandContextStrategy
{
    public WriteAdministratorNikNameForAddInOrgCommandContextStrategy(
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

    public override async Task<CommandContextStrategyResponseDto> Execute(User telegramUser, string messageText,
        CancellationToken cancellationToken)
    {
        var responseDto = new CommandContextStrategyResponseDto();
        
        _inlineKeyboard = new(
            new[] {
                new[] { InlineKeyboardButton.WithCallbackData("Завершить", KeyboardCommand.CanselOperation.ToString()) },
           }
        ); 
        var messageFromUser = messageText;
        
        if (messageFromUser.StartsWith("@"))
        { 
            messageFromUser = messageFromUser.Substring(1);
        } 
        //Сделать поиск пользователя в базе.
        var command = new GetUserByEmailORLoginQuery()
        { 
            WithoutPassword = true,
            Email = messageFromUser
        }; 
        
        var userAdministrator = await _mediator.Send(command);
        if (userAdministrator is null)
        { 
            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                chatId: telegramUser.TelegramChatId,
                text: "Введены не верные данные.",
                replyMarkup: _inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        else
        { 
            //Если пользователь которого хотят добавить присутсвует, то
            //Проверяем может он уже добавлен в таблицу Администраторов
            var checkUserAdministratorCommand = new CheckExistXOrgUserCommand
            {
                telegramUser = userAdministrator
            };
                                
            var checkUserAdministrator = await _mediator.Send(checkUserAdministratorCommand);

            if (checkUserAdministrator.Success)
            {
                responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                    chatId: telegramUser.TelegramChatId,
                    text: "Данный пользователь уже был добавлен.",
                    replyMarkup: _inlineKeyboard,
                    cancellationToken: cancellationToken);
            }
            else
            {
                responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                    chatId: telegramUser.TelegramChatId,
                    text: "Сделать чтоб пользователь был успешно добавлен.",
                    replyMarkup: _inlineKeyboard,
                    cancellationToken: cancellationToken);

                var newXUser = new UpdateXOrgUserCommand
                {
                    newOrgUser = new XOrgUser()
                    {
                        UserId = userAdministrator.Id,
                        OrgId = _currentContextMessage.Org.Id,
                        RoleId = 3
                    }
                };
                                    
                var saveUserAdministrator = await _mediator.Send(newXUser);

                if (!saveUserAdministrator)
                {
                    responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                        chatId: telegramUser.TelegramChatId,
                        text: "Ошбика при сохранении администратора",
                        replyMarkup: _inlineKeyboard,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                        chatId: telegramUser.TelegramChatId,
                        text: "Администратор добавлен.",
                        replyMarkup: _inlineKeyboard,
                        cancellationToken: cancellationToken);
                }
            }
        }

        return responseDto;
    }
}