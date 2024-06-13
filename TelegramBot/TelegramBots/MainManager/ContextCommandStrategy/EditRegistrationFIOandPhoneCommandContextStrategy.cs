using CQRS.Command.UserInfo;
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

public class EditRegistrationFIOandPhoneCommandContextStrategy : BaseCommandContextStrategy
{
    public EditRegistrationFIOandPhoneCommandContextStrategy(
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
        
        //ФИО и номер телефона вписали, надо проверить и сохранить.
        var messageFromUser = messageText.ToLower();
        var FIOandPhone = messageFromUser.Split(' ');
        if (FIOandPhone.Length == 4)
        {
            _currentContextMessage.LastMessage = messageText;
            _currentContextMessage.UserInfo = new MessageContext.UserInfo()
            {
                FirstName = FIOandPhone[0],
                SecondName = FIOandPhone[1],
                MiddleName = FIOandPhone[2],
                Phone = FIOandPhone[3],
            };

            //Если все правильно то сохранить или отредактировать данные ФИО в базу
            //Сначало смотрим присутствует ли это ФИО в базе
            var checkExistUserInfoCommand = new CheckExistUserCommand()
            {
                FirstName = _currentContextMessage.UserInfo.FirstName,
                MiddleName = _currentContextMessage.UserInfo.MiddleName,
                LastName = _currentContextMessage.UserInfo.SecondName,
            };
            var resultCheckExistUserCommand = await _mediator.Send(checkExistUserInfoCommand);

            User? userInfo = null;

            if (resultCheckExistUserCommand.Success)
            {
                //Если данные уже присутствуют в базе то человек явно уже посещал какое-то мероприятие, но может другой компании.
                userInfo = resultCheckExistUserCommand.userInfo;
            }
            else
            {
                var command = new UpdateUserCommand()
                {
                    newUser = new User
                    {
                        FirstName = _currentContextMessage.UserInfo.FirstName,
                        MiddleName = _currentContextMessage.UserInfo.MiddleName,
                        LastName = _currentContextMessage.UserInfo.SecondName,
                        Phone = _currentContextMessage.UserInfo.Phone,
                    }
                };

                var saveUserInfo = await _mediator.Send(command);
                if (saveUserInfo)
                {
                    userInfo = command.newUser;
                }
            }

            //Нужно сохранить данные в таблицу RegistrationOnEvents, чтоб привязать пользователя которого регестриуют к событию
            var checkExistRegistrationOnEventsCommand = new CheckExistRegistrationOnEventsCommand()
            {
                EventId = 1,
                WhoRegistratedUserId = telegramUser.Id,
                UserId = userInfo.Id,
                StateRegistrationOnEvent = "WaitingManagerApprove"
            };

            //Сначало проверить существует ли он в базе 
            var checkRegistrationOnEvent = await _mediator.Send(checkExistRegistrationOnEventsCommand);
            if (checkRegistrationOnEvent.Success)
            {
                //пользователь уже пытался зарегестрироваться
            }
            else
            {
                // Регестрируем пользователя
                var updateRegistrationOnEventsCommand = new UpdateRegistrationOnEventsCommand()
                {
                    newRegistrationOnEvents = new XEventUser()
                    {
                        EventId = 1,
                        UserId = userInfo.Id,
                        WhoRegUserId = telegramUser.Id,
                    }
                };
            }

            //
            _inlineKeyboard = new(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Завершить регистрацию",
                            KeyboardCommand.BreakRegistration.ToString())
                    },
                    new[]
                    { 
                        InlineKeyboardButton.WithCallbackData("Редактировать",
                            KeyboardCommand.EditRegistrationFIOandPhone.ToString())
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Верные",
                            KeyboardCommand.ApproveRegistrationFIOandPhone.ToString())
                    }
                });

            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                                    chatId: telegramUser.TelegramChatId,
                                    text: "Подтвердите корректность введенных данных:\n" +
                                          messageText,
                                    replyMarkup: _inlineKeyboard,
                                    cancellationToken: cancellationToken);
        }
        else
        {
            _inlineKeyboard = new(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Завершить регистрацию",
                                                KeyboardCommand.BreakRegistration.ToString())
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Редактировать",
                                                KeyboardCommand.EditRegistrationFIOandPhone.ToString())
                    },
                });

            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                chatId: telegramUser.TelegramChatId,
                text: "Введены не верные данные.",
                replyMarkup: _inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        
        return responseDto;
    }
}