using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using DataBase.Entities.Entities_DBContext;
using Telegram.Bot.Types;
using TelegramBot.MessageContext;
using User = DataBase.Entities.Entities_DBContext.User;
using CQRS.Query;
using CQRS.Query.TelegramBotInChat;
using CQRS.Command.UserInfo;
using TelegramBot.Facade;
using CQRS;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Exceptions;
using System.Drawing;
using System.Text;
using TelegramBot.TelegramBots.MainManager;

namespace TelegramBot.TelegramBots.Party;

public partial class PartyTelegramBot : TelegramBot
{
    protected override async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                await HandleMessage(botClient, update, cancellationToken);
                break;

            // A button was pressed
            case UpdateType.CallbackQuery:
                await HandleButton(botClient, update.CallbackQuery!, cancellationToken);
                break;

            case UpdateType.MyChatMember:

                //1) Ищем телеграм пользователя который создал компанию и выводим список компаний если их несколько.
                //2) Если компания одна, то выводим список вечерениок, на которую должен работать этот канал.
                //3) Если у данного пользователя нет компании, то ищем является ли он менеджером.
                //4) Если является то какой компании и выводим список компаний на выбор
                //5) Выводим список Вечеринок (событий) которую нужно выбрать для этого канала.
                //6) Если все выбранно, то регестрируем чат.

                if(update.MyChatMember!.Chat is not null) // && update.MyChatMember.Chat.Title == ChanelName
                {
                    if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Administrator)
                    {

                        var result = await _mediator.Send(new CheckExistXOrgUserCommand
                        {
                            telegramUser = new User
                            {
                                UserName = update.MyChatMember.From.Username,
                                TelegramChatId = update.MyChatMember.From.Id
                            },
                            BotId = this.TelegramBotId
                        });

                        if (result.Success)
                        {
                            var response = _mediator.Send(new UpdateTelegramBotInChatCommand
                            {
                                newTelegramBotInChat = new TelegramBotInChats()
                                {
                                    TelegramChatId = update.MyChatMember.Chat.Id,
                                    ChatStatus = update.MyChatMember.NewChatMember.Status.ToString(),
                                    ChatTitle = update.MyChatMember.Chat.Title,
                                    ChatType = update.MyChatMember.Chat.Type.ToString(),
                                    TelegramBotId = _id,
                                }
                            });

                            _chanelPostId = update.MyChatMember.Chat.Id;
                            
                            await botClient.SendTextMessageAsync(
                                chatId: update.MyChatMember.Chat.Id,
                                text: "Успешно добавлен.",
                                cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: update.MyChatMember.Chat.Id,
                                text: "Только зарегестрированные пользователи могут добавлять меня в канал. Простите, я ливаю.",
                                cancellationToken: cancellationToken);
                            
                            try
                            {
                                await botClient.LeaveChatAsync(
                                    chatId: update.MyChatMember.Chat.Id,
                                    cancellationToken: cancellationToken);
                            }
                            catch (ApiRequestException apiEx)
                            {
                                Console.WriteLine($"Telegram API Ошибка: {apiEx.Message}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка: {ex.Message}");
                            }
                        }
                    }
                    else if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Left || update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Kicked)
                    {
                        var response = _mediator.Send(new DeleteTelegramBotInChatCommand { 
                            TelegramChatId = update.MyChatMember.Chat.Id,
                            TelegramBotId = TelegramBotId
                        });
                        
                        _chanelPostId = null;
                    }
                }
                
                break;
            case UpdateType.ChannelPost:
                /*
                //Зарегестрировать данный чат под конкретную вечеринку вечеринку
                // 1 выбрать организацию
                InlineKeyboardMarkup? inlineKeyboard = new(
                  new[] {
                    new[] { InlineKeyboardButton.WithCallbackData("Фейк",Facade.KeyboardCommand.FaildCheckPictureFromMenager.ToString()) },
                    new[] { InlineKeyboardButton.WithCallbackData("Подтверить", Facade.KeyboardCommand.ApproveCheckPictureFromMenager.ToString()) }
                  }
               );

                await botClient.SendTextMessageAsync(
                                chatId: update.ChannelPost.Chat.Id,
                                text: "test response",
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationToken);*/
                break;
        }

    }

    protected override async Task HandleButton(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken)
    {
        var telegramUser = new User
        {
            TelegramChatId = query.Message!.Chat.Id,
            MessageId = query.Message.MessageId,
            FirstName = query.Message.Chat.FirstName,
            LastName = query.Message.Chat.LastName,
            UserName = query.Message.Chat.Username,
        };

        var parametrs = query.Data?.Split(" ") ?? new string[] { };
        var command = parametrs[0];
        string responseText = string.Empty;

        if (Enum.TryParse<Facade.KeyboardCommand>(command, out var parsedEnum))
        {
            switch (parsedEnum)
            {
                case Facade.KeyboardCommand.StartRegistrationFriend:
                    {
                        responseText = $"Друг зарегистрирован в телеграме?";
                        InlineKeyboardMarkup? inlineKeyboard = new(
                           new[] {
                                   new[] { InlineKeyboardButton.WithCallbackData("Нет",Facade.KeyboardCommand.StartRegistrationFriendNo.ToString()) },
                                   new[] { InlineKeyboardButton.WithCallbackData("Да", Facade.KeyboardCommand.StartRegistrationFriendYes.ToString()) }
                           }
                        );

                        await botClient.SendTextMessageAsync(
                              chatId: telegramUser.TelegramChatId,
                              text: responseText,
                              replyMarkup: inlineKeyboard,
                              cancellationToken: cancellationToken);

                        break;
                    }
                case Facade.KeyboardCommand.StartRegistrationFriendNo:
                    {
                        //responseText = $"Введите: @userName друга.\n"+
                        //               $"Например: @Sergei_Smogliuk";

                        responseText = $"Друг зарегистрирован в телеграме?";
                        InlineKeyboardMarkup? inlineKeyboard = new(
                           new[] {
                                   new[] { InlineKeyboardButton.WithCallbackData("Нет",Facade.KeyboardCommand.StartRegistrationFriendNo.ToString()) },
                                   new[] { InlineKeyboardButton.WithCallbackData("Да", Facade.KeyboardCommand.StartRegistrationFriendYes.ToString()) }
                           }
                        );
                        await botClient.SendTextMessageAsync(
                              chatId: telegramUser.TelegramChatId,
                              text: responseText,
                              replyMarkup: inlineKeyboard,
                              cancellationToken: cancellationToken);

                        break;
                    }
                case Facade.KeyboardCommand.StartRegistrationFriendYes:
                    {
                        responseText = $"Введите: @userName друга.\n"+
                                       $"Например: @Sergei_Smogliuk";

                        var newContext = new MessageContext.TelegramBotMessageContext()
                        {
                            LastCommand = command,
                            State = Facade.TelegramUserState.WriteFriendAccount,
                            TelegramUser = telegramUser,
                            LastMessage = command,
                        };

                        _messageContexts.TryAdd(telegramUser.TelegramChatId, newContext);

                        await botClient.SendTextMessageAsync(
                              chatId: telegramUser.TelegramChatId,
                              text: responseText,
                              cancellationToken: cancellationToken);

                        break;
                    }
                case Facade.KeyboardCommand.StartRegistrationYourSelf:
                    {
                        responseText = $"Танцуете?";
                        InlineKeyboardMarkup? inlineKeyboard = new(
                           new[] {
                                   new[] { InlineKeyboardButton.WithCallbackData("Не танцующий",Facade.KeyboardCommand.StartRegistrationNoDancer.ToString()) },
                                   new[] { InlineKeyboardButton.WithCallbackData("Танцующий", Facade.KeyboardCommand.StartRegistrationDancer.ToString()) }
                           }
                        );

                        var newContext = new MessageContext.TelegramBotMessageContext()
                        {
                            LastCommand = command,
                            State = Facade.TelegramUserState.StartRegistrationYourSelf,
                            TelegramUser = telegramUser,
                            LastMessage = command,
                        };

                        _messageContexts.TryAdd(telegramUser.TelegramChatId, newContext);

                        await botClient.SendTextMessageAsync(
                               chatId: telegramUser.TelegramChatId,
                               text: responseText,
                               replyMarkup: inlineKeyboard,
                               cancellationToken: cancellationToken);
                        break;
                    }  
                case Facade.KeyboardCommand.StartRegistrationDancer:
                    {
                        TelegramBotMessageContext? messageContext = null;
                        if (!_messageContexts.TryGetValue(telegramUser.TelegramChatId, out messageContext))
                        {
                            await botClient.SendTextMessageAsync(
                               chatId: telegramUser.TelegramChatId,
                               text: "Ошибка",
                               cancellationToken: cancellationToken);
                            break;
                        }

                        if(messageContext.State == Facade.TelegramUserState.StartRegistrationYourSelf)
                        {
                            responseText = $"Введите Фамилию Имя Отчетсво и номер телефона через пробелы.";

                            messageContext.State = Facade.TelegramUserState.WritedFIOandPhoneYourSelf;
        
                            await botClient.SendTextMessageAsync(
                                    chatId: telegramUser.TelegramChatId,
                                    text: responseText,
                                    cancellationToken: cancellationToken);
                        }
                        else
                        {
                            break;
                        }

                        
                        break;
                    }
                case Facade.KeyboardCommand.EditRegistrationFIOandPhone:
                    {
                        TelegramBotMessageContext? messageContext = null;
                        if (!_messageContexts.TryGetValue(telegramUser.TelegramChatId, out messageContext))
                        {
                            await botClient.SendTextMessageAsync(
                               chatId: telegramUser.TelegramChatId,
                               text: "Ошибка",
                               cancellationToken: cancellationToken);
                            break;
                        }

                        responseText = $"Отредактируйте и пришлите мне Фамилию Имя Отчетсво и номер телефона через пробелы.\n" +
                                       "Вы вводили:\n" +
                                       messageContext.LastMessage;

                        messageContext.State = Facade.TelegramUserState.EditRegistrationFIOandPhone;

                        await botClient.SendTextMessageAsync(
                                chatId: telegramUser.TelegramChatId,
                                text: responseText,
                                cancellationToken: cancellationToken);
                        break;
                    }
                case Facade.KeyboardCommand.BreakRegistration:
                    {
                        if (_messageContexts.TryGetValue(telegramUser.TelegramChatId, out var value))
                        {
                            _messageContexts.TryRemove(new KeyValuePair<long, TelegramBotMessageContext>(telegramUser.TelegramChatId, value));
                            responseText = "Регистрация остановленна.";
                            InlineKeyboardMarkup? inlineKeyboard = new(
                               new[] {
                                       new[] { InlineKeyboardButton.WithCallbackData("Не танцующий",Facade.KeyboardCommand.StartRegistrationNoDancer.ToString()) },
                                       new[] { InlineKeyboardButton.WithCallbackData("Танцующий", Facade.KeyboardCommand.StartRegistrationDancer.ToString()) }
                               }
                           );

                            await botClient.SendTextMessageAsync(
                                   chatId: telegramUser.TelegramChatId,
                                   text: responseText,
                                   replyMarkup: inlineKeyboard,
                                   cancellationToken: cancellationToken);
                        }

                        break;
                    }
                case Facade.KeyboardCommand.ApproveRegistrationFIOandPhone:
                    {
                        if (_messageContexts.TryGetValue(telegramUser.TelegramChatId, out var value))
                        {
                            value.State = Facade.TelegramUserState.SendedCheckPicture;

                            var today = DateTime.Now;
                            var PartyDate = new DateTime(DateTime.Now.Year, 12, 22);

                            responseText = $"Текущая дата: {today.ToString("dd:MM:yyyy")}\r\n" +
                                            "Стоимость входа:\r\n";

                            if (DateTime.Now >= PartyDate)
                            {
                                responseText += "800₽ - при оплате онлайн 22.12 или на входе\r\n";
                            }
                            else
                            {
                                responseText +=
                                    "600₽ - при оплате онлайн до 22.12 с репостом записи\r\n" +
                                    "700₽ - при оплате онлайн до 22.12 без репоста\r\n";
                            }

                            responseText += "Реквезиты:\r\n" +
                                            "По номеру телефона: 89832068482\r\n" +
                                            "Номер карты: 4011111111111112\r\n" +
                                            "\r\n" +
                                            "После перевода, прикрепите фотографию оплаты.";

                            await botClient.SendTextMessageAsync(
                                   chatId: telegramUser.TelegramChatId,
                                   text: responseText,
                                   cancellationToken: cancellationToken);
                        }

                        break;
                    }
                case Facade.KeyboardCommand.FaildCheckPictureFromMenager:
                    {
                        if (query.Message.Caption is not null)
                        {
                            await botClient.EditMessageCaptionAsync(
                               chatId: query.Message.Chat.Id,
                               messageId: query.Message.MessageId,
                               caption: $"{query.Message.Caption.Replace("_", "\\_")}\n *[Фейк]*",
                               parseMode: ParseMode.Markdown);
                        }
                        else if (query.Message.Text is not null)
                        {
                            await botClient.EditMessageTextAsync(
                               chatId: query.Message.Chat.Id,
                               messageId: query.Message.MessageId,
                               text: $"{query.Message.Text.Replace("_", "\\_")}\n *[Фейк]*",
                               parseMode: ParseMode.Markdown);
                        }

                        break;
                    }
                case Facade.KeyboardCommand.ApproveCheckPictureFromMenager:
                    {
                        if (query.Message.Caption is not null)
                        {
                            await botClient.EditMessageCaptionAsync(
                               chatId: query.Message.Chat.Id,
                               messageId: query.Message.MessageId,
                               caption: $"{query.Message.Caption.Replace("_", "\\_")}\n *[Подтвержден]*",
                               replyMarkup: null,
                               parseMode: ParseMode.Markdown);
                        }
                        else if (query.Message.Text is not null)
                        {
                            await botClient.EditMessageTextAsync(
                               chatId: query.Message.Chat.Id,
                               messageId: query.Message.MessageId,
                               text: $"{query.Message.Text.Replace("_", "\\_")}\n *[Подтвержден]*",
                               parseMode: ParseMode.Markdown);
                        }
                        break;
                    }
            }

        }

        return;
    }

    protected override async Task HandleMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
    {
        User telegramUser = null;

        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } && message.Photo is { } && message.Photo.Length > 0)
        {
            //Получили только фотку.
            Logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"Received only photo message from FirstName: [{message.Chat.FirstName}]. LastName: [{message.Chat.LastName}]. ChatId {message.Chat.Id}.");

            telegramUser = new User
            {
                TelegramChatId = message.Chat.Id,
                MessageId = message.MessageId,
                FirstName = message.Chat.FirstName,
                LastName = message.Chat.LastName,
                UserName = message.Chat.Username,
            };

            CommandParser(botClient, message, "TakePicture", telegramUser, cancellationToken);
            return;
        }

        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        Logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"Received a '{messageText}' message from FirstName: [{message.Chat.FirstName}]. LastName: [{message.Chat.LastName}]. ChatId {message.Chat.Id}.");

        telegramUser = new User
        {
            TelegramChatId = message.Chat.Id,
            MessageId = message.MessageId,
            FirstName = message.Chat.FirstName,
            LastName = message.Chat.LastName,
            UserName = message.Chat.Username,
        };


        CommandParser(botClient, message, messageText, telegramUser, cancellationToken);

    }


    //TODO: вынести в отдельный статически класс.
    protected override async void CommandParser(ITelegramBotClient botClient, Message message, string messageText, User telegramUser, CancellationToken cancellationToken = default, bool adminCommand = false, string[]? parametrs = null)
    {

        var replyKeyboardRemove = new ReplyKeyboardRemove();
        Message? sentMessage = null;
        InlineKeyboardMarkup? inlineKeyboard = null;

        _messageContexts.TryGetValue(telegramUser.TelegramChatId, out var conxtexMessage);

        switch (messageText?.ToLower())
        {
            case "/start":
                {
                    var responce = await _telegramBotFacade.Start(this,telegramUser);

                    var responceText = responce.Message ?? "Ошибка";

                    if (responce.State == Facade.TelegramUserState.StartRegistration)
                    {
                        //Регестрируем себя или другого чиловека.
                        inlineKeyboard = new(
                            new[] {
                               new[] { InlineKeyboardButton.WithCallbackData("Зарегистрировать друга", Facade.KeyboardCommand.StartRegistrationFriend.ToString()) },
                               new[] { InlineKeyboardButton.WithCallbackData("Зарегистрироваться самостоятельно", Facade.KeyboardCommand.StartRegistrationYourSelf.ToString()) }
                            }
                        );

                        //inlineKeyboard = new(
                        //    new[] {
                        //       new[] { InlineKeyboardButton.WithCallbackData("Не танцующий",Facade.KeyboardCommand.StartRegistrationNoDancer.ToString()) },
                        //       new[] { InlineKeyboardButton.WithCallbackData("Танцующий", Facade.KeyboardCommand.StartRegistrationDancer.ToString()) }
                        //    }
                        //);
                    }

                    sentMessage = await botClient.SendTextMessageAsync(
                              chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                              responceText,
                              replyMarkup: inlineKeyboard,
                              cancellationToken: cancellationToken);
                    break;
                }
            case "/registrationcustomercompany":
                {
                    string responceText = "Регистрация компании.\n"
                             + "Введите название компании:";

                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = messageText?.ToLower(),
                        State = Facade.TelegramUserState.RegistrationCustomerCompany,
                        TelegramUser = telegramUser,
                        LastMessage = responceText,
                    };

                    if(conxtexMessage is null)
                    {
                        _messageContexts.TryAdd(telegramUser.TelegramChatId, newContext);
                    }
                    else
                    {
                        conxtexMessage = newContext;
                    }
                   
                    sentMessage = await botClient.SendTextMessageAsync(
                       chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                       text: responceText,
                       cancellationToken: cancellationToken);
                    break;
                }
            case "/registrationparty":
                {
                    string responceText = "Регистрация Вечеринки.\n"
                           + "Введите данны:\n\n"
                           + "*Название вечеринки (дд.мм.гггг)*;\n"
                           + "*Стоимость (число)*;\n"
                           + "*Стоимость если есть репост (число)*;\n"
                           + "*Стоимость в день вечеринки (число)*;\n"
                           + "*Бесплатный вход не танцующим (да/нет)*\n\n"
                           + "симовл *;* обязателен в конце каждого типа данных, кроме последней строки. Дату вводить в формате *дд.мм.гггг*\n"
                           + "Пример:\n";

                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = messageText?.ToLower(),
                        State = Facade.TelegramUserState.RegistrationParty,
                        TelegramUser = telegramUser,
                        LastMessage = responceText,
                    };

                    if (conxtexMessage is null)
                    {
                        _messageContexts.TryAdd(telegramUser.TelegramChatId, newContext);
                    }
                    else
                    {
                        conxtexMessage = newContext;
                    }

                    sentMessage = await botClient.SendTextMessageAsync(
                       chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                       text: responceText.Replace("_", "\\_"),
                       parseMode: ParseMode.Markdown,
                       cancellationToken: cancellationToken);

                    sentMessage = await botClient.SendTextMessageAsync(
                      chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                      text: "07.12.2023;\n"
                           + "Примерное название вечеринки;\n"
                           + "300;\n"
                           + "200;\n"
                           + "400;\n"
                           + "да\n".Replace("_", "\\_"),
                      parseMode: ParseMode.Markdown,
                      cancellationToken: cancellationToken);

                    
                    break;
                }
            case "/admin_setuserrights":
                {
                    break;
                }
            default:
                {
                    if (conxtexMessage != null)
                    {
                        
                        var commandContextStrategy = ContextCommandFactory.GetBaseStrategy(message, botClient, _mediator, conxtexMessage, _scopeFactory, _telegramBotFacade);
                        if(commandContextStrategy is null)
                            break;
                    
                        var responseContextStrategy =  await commandContextStrategy.Execute(telegramUser, messageText, cancellationToken: cancellationToken);
                        if (responseContextStrategy.DeleteCurrentContext)
                        {
                            _messageContexts.TryRemove(
                                new KeyValuePair<long, TelegramBotMessageContext>(telegramUser.TelegramChatId, null));
                        }

                        sentMessage = responseContextStrategy.SentMessage;
                    }
                    break;
                }
        }

        if (sentMessage is null)
        {
            await botClient.SendTextMessageAsync(
                  chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                  "Вы отправили: " + messageText,
                  replyMarkup: replyKeyboardRemove,
                  cancellationToken: cancellationToken);
        }

        return;
    }

    public override async Task<ResponceTelegramFacade> Start(User? requestTelegramUser)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetService<IMediator>();

        if (mediator is null)
            return new ResponceTelegramFacade(TelegramUserErrorCode.MediatorNotFound);

        //Ищем пользователя.
        var responceUser = await FindUser(requestTelegramUser);

        var test = new StartTelegramBotCommand { telegramUser = requestTelegramUser };
        var responce = await mediator.Send(test);

        if (responce.telegramUser is null)
        {
            return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated);
        }

        return new ResponceTelegramFacade(TelegramUserState.StartRegistration, true)
        {
            Message = "НОВОГОДНЯЯ ВЕЧЕРИНКА!🎄🎉🎅" +
                      "\r\n\r\n\r\n" +
                      "Некоторые уже начали готовиться к празднованию Нового года и с нетерпением ждут анонсы всех вечеринок\U0001f973💃" +
                      "\r\n\r\n" +
                      "💃Вечеринка состоится 22.12 (пятница) с 21:00 до 24:00🎉" +
                      "\r\n\r\n" +
                      "У нас в программе:" +
                      "\r\n" +
                      "🎅 Тайный Санта\r\n" +
                      "💃 Много зажигательных танцев, общение \r\n" +
                      "🌟Поздравление преподавателя школы Анны \U0001f973 а также всех ноябрьских и декабрьских именинников\r\n" +
                      "🌟Розыгрыш абонементов и скидок на абонемент\r\n" +
                      "🌟Танцевальная анимация\r\n" +
                      "🌟Вкусные треки от DJ GOODVIN\r\n" +
                      "🌟Welcome drink и аппетитные фрукты\r\n" +
                      "🌟Фотограф\r\n\r\n" +
                      "Стоимость входа:\r\n" +
                      "600₽ - при оплате онлайн до 22.12 с репостом записи\r\n" +
                      "700₽ - при оплате онлайн до 22.12 без репоста\r\n" +
                      "800₽ - при оплате онлайн 22.12 или на входе\r\n" +
                      "Нетанцующему другу/подруге вход свободный🤗\r\n\r\n\r\n" +
                      "Дресс-код: праздничный💃\r\n\r\n" +
                      "Вечеринка для всех желающих, поэтому если ты хочешь повеселиться вместе с нами, бронируй свое место у @Nataschunya 🔥\U0001f973"
        };
    }
}
