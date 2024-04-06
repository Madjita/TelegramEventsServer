using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using IronBarCode;
using TelegramBot.MessageContext;
using CQRS.Command.UserInfo;
using User = DataBase.Entities.Entities_DBContext.User;
using Message = Telegram.Bot.Types.Message;
using CQRS.Query;
using DataBase.Entities.Entities_DBContext;
using Azure;
using MediatR;
using MyLoggerNamespace.Helpers;
using TelegramBot.Facade;
using CQRS;
using CQRS.Query.CustomerCompany;
using System.Text;
using TelegramBot.TelegramBotFactory.TelegramBotDto;

namespace TelegramBot;

public partial class TelegramBot : IAsyncDisposable
{
    
    protected virtual async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
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
                if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Administrator && update.MyChatMember.Chat.Title ==  "PerfectoParty")
                {
                    _chanelPostId = update.MyChatMember.Chat.Id;
                }
                else if(update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Left)
                {
                    _chanelPostId = null;
                }
                break;
            case UpdateType.ChannelPost:
                  /*
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

    protected virtual async Task HandleButton(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken)
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

        if(Enum.TryParse<Facade.KeyboardCommand>(command, out var parsedEnum))
        {
            switch (parsedEnum)
            {
                case Facade.KeyboardCommand.StartRegistrationFriend:
                    {
                        break;
                    }
                case Facade.KeyboardCommand.StartRegistrationYourSelf:
                    {
                        responseText = $"Танцуете ?";
                        InlineKeyboardMarkup? inlineKeyboard = new (
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
                        break;
                    }
                case Facade.KeyboardCommand.StartRegistrationDancer:
                    {

                        responseText = $"Введите Фамилию Имя Отчетсво и номер телефона через пробелы.";

                        var newContext = new MessageContext.TelegramBotMessageContext()
                        {
                            LastCommand = command,
                            State = Facade.TelegramUserState.WritedFIOandPhoneYourSelf,
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
                case Facade.KeyboardCommand.EditRegistrationFIOandPhone:
                    {
                        TelegramBotMessageContext? messageContext = null;
                        if(!_messageContexts.TryGetValue(telegramUser.TelegramChatId, out messageContext))
                        {
                            await botClient.SendTextMessageAsync(
                               chatId: telegramUser.TelegramChatId,
                               text: "Ошибка",
                               cancellationToken: cancellationToken);
                            break;
                        }
                        
                        responseText = $"Отредактируйте и пришлите мне Фамилию Имя Отчетсво и номер телефона через пробелы.\n" +
                                       "Вы вводили:\n"+
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
                        if(_messageContexts.TryGetValue(telegramUser.TelegramChatId, out var value))
                        {
                            _messageContexts.TryRemove(new KeyValuePair<long, TelegramBotMessageContext>( telegramUser.TelegramChatId, value));
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

                            responseText = $"Текущая дата: {today.ToString("dd:MM:yyyy")}\r\n"+
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
                                            "Номер карты: 4011111111111112\r\n"+
                                            "\r\n"+
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
                        if(query.Message.Caption is not null)
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
                case Facade.KeyboardCommand.ExitFromBot:
                    {
                        //Выйти из чата
                        var response = await _telegramBotFacade.Exit(this, telegramUser);

                        if(response.InlineKeyboard is not null)
                        {
                            await botClient.EditMessageTextAsync(
                              chatId: query.Message.Chat.Id,
                              messageId: query.Message.MessageId,
                              text: response.Message,
                              replyMarkup: response.InlineKeyboard);
                        }
                        else
                        {
                            await botClient.EditMessageTextAsync(
                              chatId: query.Message.Chat.Id,
                              messageId: query.Message.MessageId,
                              text: $"*{response.Message}*",
                              parseMode: ParseMode.Markdown);
                        }
                       

                        break;
                    }
                case Facade.KeyboardCommand.ExitFromBotNo:
                    {
                        await botClient.EditMessageTextAsync(
                               chatId: query.Message.Chat.Id,
                               messageId: query.Message.MessageId,
                               text: "Не хотите, как хотите.");
                        break;
                    }
                case Facade.KeyboardCommand.ExitFromBotYes:
                    {
                        //Активировать пользователя.
                        var response = await _telegramBotFacade.ActivateUser(this, telegramUser);

                        await botClient.EditMessageTextAsync(
                               chatId: query.Message.Chat.Id,
                               messageId: query.Message.MessageId,
                               text: response.Message,
                               replyMarkup: response.InlineKeyboard);

                        break;
                    }
                case Facade.KeyboardCommand.StartRegistrationCompany:
                    {

                        string responceText = "Регистрация компании.\n"
                         + "Введите название компании:";

                        var newContext = new MessageContext.TelegramBotMessageContext()
                        {
                            LastCommand = command,
                            State = Facade.TelegramUserState.RegistrationCustomerCompany,
                            TelegramUser = telegramUser,
                            LastMessage = responceText,
                        };


                        var added = _messageContexts.TryAdd(telegramUser.TelegramChatId, newContext);

                        await botClient.SendTextMessageAsync(
                                chatId: telegramUser.TelegramChatId,
                                text: responceText,
                                cancellationToken: cancellationToken);
                        break;
                    }
                case Facade.KeyboardCommand.PrevPage:
                case Facade.KeyboardCommand.NextPage:
                    {
                        var userFromBD = await FindUser(telegramUser);
                        if (userFromBD is null) break;

                        var buttons = await _telegramBotFacade.PageCompanyButtons(_mediator, userFromBD, parametrs[1].ToInt());

                        InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);

                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: query.Message.Text,
                            replyMarkup: inlineKeyboardMarkup,
                            cancellationToken: cancellationToken);

                        break;
                    }
                case Facade.KeyboardCommand.SelectCompany:
                    {
                        //Выбираем организацию
                        //1) Показываем кнопку Зарегестрировать телеграм бота
                        //2) Отображаем доступные телеграм боты

                        var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                        var organization = await _mediator.Send(commandGetOrg);
                        string responceText = string.Empty;

                        if (!organization.Success)
                        {
                            responceText  = $"Выбранной компании не сущесвтует в БД.";

                            await botClient.EditMessageTextAsync(
                                  chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                  messageId: query.Message.MessageId,
                                  text: responceText,
                                  cancellationToken: cancellationToken);
                        }
                        else
                        {

                            var telegramBotByOrgId = new GetAllTelegramBotByOrgIdQuery() { OrgId = organization.customerCompany.Id };
                            var telegramBots = await _mediator.Send(telegramBotByOrgId);

                            responceText = $"Выбранна компания: *{organization.customerCompany.Name}*\n"
                                           +"Созданные боты:\n\n\n";

                            List<List<InlineKeyboardButton>> buttons = new();
                            buttons.Add(new List<InlineKeyboardButton>());
                            buttons.Add(new List<InlineKeyboardButton>());
                            buttons[0].Add(InlineKeyboardButton.WithCallbackData("Создать телеграм бота", $"{KeyboardCommand.StartRegistrationBot} {organization.customerCompany.Id}"));

                            var responseTextTelegramBots = new StringBuilder();

                            foreach(var bot in telegramBots)
                            {
                                if(bot.TelegramBotName.EndsWith("_bot") || bot.TelegramBotName.EndsWith("Bot"))
                                {
                                    responseTextTelegramBots.AppendLine($"@{bot.TelegramBotName}");
                                }
                                else
                                {
                                    responseTextTelegramBots.AppendLine($"@{bot.TelegramBotName}_bot");
                                }
                            }

                            InlineKeyboardMarkup? inlineKeyboard = new(buttons);

                            await botClient.EditMessageTextAsync(
                              chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                              messageId: query.Message.MessageId,
                              text: responceText + responseTextTelegramBots.ToString(),
                              cancellationToken: cancellationToken,
                              replyMarkup: inlineKeyboard);

                        }
                        break;
                    }
                case KeyboardCommand.StartRegistrationBot:
                    {
                        var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                        var organization = await _mediator.Send(commandGetOrg);

                        //Введите информацию для регестриации бота
                        var responceText =  "1) Перейдите в @BotFather:\n"
                                          + "2) Введите или выбирите из меню /newbot\n"
                                          + "3) Выбирите бота /mybots\n"
                                          + "4) Нажмите на кнпоку \"API Token\" и скопируйте токен\n"
                                          + "5) Введите информацию для регистрации вашего телеграм ботам в системе через *;*:\n\n\n"
                                          + "Пример:\n"
                                          + "@TestBot;*ваш скопированный из @BotFather токен*";

                        var newContext = new MessageContext.TelegramBotMessageContext()
                        {
                            LastCommand = command,
                            State = Facade.TelegramUserState.RegistrationTelegramBot,
                            TelegramUser = telegramUser,
                            LastMessage = command,
                            Org = organization.customerCompany
                        };

                        _messageContexts.TryAdd(telegramUser.TelegramChatId, newContext);

                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            cancellationToken: cancellationToken,
                            parseMode: ParseMode.Markdown);

                        break;
                    }
                case KeyboardCommand.EndRegistrationBot:
                    {

                        break;
                    }
            }

        }

        return;
    }

    protected virtual async Task HandleMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
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
                UserName = message.Chat.Username
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
            MessageId=  message.MessageId,
            FirstName = message.Chat.FirstName,
            LastName = message.Chat.LastName,
            UserName = message.Chat.Username,
        };


        CommandParser(botClient, message, messageText, telegramUser, cancellationToken);

    }

   //TODO: вынести в отдельный статически класс.
    protected virtual async void CommandParser(ITelegramBotClient botClient, Message message, string messageText, User telegramUser, CancellationToken cancellationToken = default, bool adminCommand = false, string[]? parametrs = null)
    {

        var replyKeyboardRemove = new ReplyKeyboardRemove();
        Message? sentMessage = null;
        InlineKeyboardMarkup? inlineKeyboard = null;

        _messageContexts.TryGetValue(telegramUser.TelegramChatId, out var conxtexMessage);


        switch (messageText.ToLower())
        {
            case "/start":
            {

                //Определить пользователь пришел первый раз или уже существует в системе ?
                var responce = await Start(telegramUser);

                var responceText = responce.Message ?? "Ошибка";

                sentMessage = await botClient.SendTextMessageAsync(
                     chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                     responceText,
                     replyMarkup: responce.InlineKeyboard,
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
                   text: responceText,
                   cancellationToken: cancellationToken);
                break;
            }
            case "/registrationparty":
            {
                string responceText = "Регистрация события.\n"
                       + "Введите данны:\n\n"
                       + "*Дата проведения события (дд.мм.гггг)*"
                       + "*Название события (дд.мм.гггг)*;\n"
                       + "*Стоимость (число)*;\n"
                       + "*Стоимость если есть репост (число)*;\n"
                       + "*Стоимость в день события (число)*;\n"
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
                    switch(conxtexMessage.State)
                    {
                        case TelegramUserState.WritedFIOandPhoneYourSelf or TelegramUserState.EditRegistrationFIOandPhone:
                        {
                            //ФИО и номер телефона вписали, надо проверить и сохранить.
                            var messageFromUser = messageText.ToLower();
                            var FIOandPhone = messageFromUser.Split(' ');
                            if(FIOandPhone.Length == 4)
                            {
                               conxtexMessage.LastMessage = messageText;
                               conxtexMessage.UserInfo = new MessageContext.UserInfo()
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
                                   FirstName = conxtexMessage.UserInfo.FirstName,
                                   MiddleName = conxtexMessage.UserInfo.MiddleName,
                                   LastName = conxtexMessage.UserInfo.SecondName,
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
                                            FirstName = conxtexMessage.UserInfo.FirstName,
                                            MiddleName = conxtexMessage.UserInfo.MiddleName,
                                            LastName = conxtexMessage.UserInfo.SecondName,
                                            Phone = conxtexMessage.UserInfo.Phone,
                                        }
                                    };

                                    var saveUserInfo = await _mediator.Send(command);
                                    if(saveUserInfo)
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
                               if(checkRegistrationOnEvent.Success)
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
                               inlineKeyboard = new(
                                  new[] {
                                      new[] { InlineKeyboardButton.WithCallbackData("Завершить регистрацию", Facade.KeyboardCommand.BreakRegistration.ToString()) },
                                      new[] { InlineKeyboardButton.WithCallbackData("Редактировать",Facade.KeyboardCommand.EditRegistrationFIOandPhone.ToString()) },
                                      new[] { InlineKeyboardButton.WithCallbackData("Верные", Facade.KeyboardCommand.ApproveRegistrationFIOandPhone.ToString()) }
                                  }
                               );

                               sentMessage = await botClient.SendTextMessageAsync(
                                              chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                              text: "Подтвердите корректность введенных данных:\n"+
                                                    messageText,
                                              replyMarkup: inlineKeyboard,
                                              cancellationToken: cancellationToken);
                              
                            }
                            else
                            {
                                   inlineKeyboard = new(
                                      new[] {
                                          new[] { InlineKeyboardButton.WithCallbackData("Завершить регистрацию", Facade.KeyboardCommand.BreakRegistration.ToString()) },
                                          new[] { InlineKeyboardButton.WithCallbackData("Редактировать",Facade.KeyboardCommand.EditRegistrationFIOandPhone.ToString()) },
                                       }
                                    );

                                    sentMessage = await botClient.SendTextMessageAsync(
                                       chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                       text: "Введены не верные данные.",
                                       replyMarkup: inlineKeyboard,
                                       cancellationToken: cancellationToken);
                            }

                            break;
                        }
                        case TelegramUserState.SendedCheckPicture:
                        {
                            // Текст, который вы хотите закодировать в QR-код
                            string textToEncode = conxtexMessage.LastMessage;

                            // Сохранить фотку которую нам прислали:
                            // Получаем информацию о фото
                            var photo = message.Photo[^1]; // берем последний элемент массива, который обычно является самым крупным размером
                            var fileId = photo.FileId;
                            var dir = "downloaded_photos";

                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            // Путь, куда сохранить файл
                            var savePath = $"{dir}/{textToEncode}.jpg";

                            // Получаем информацию о файле и скачиваем его
                            using (var destinationStream = new MemoryStream())
                            {
                                await botClient.GetInfoAndDownloadFileAsync(fileId, destinationStream);

                                // Копируем файл в указанный путь
                                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                                {
                                    destinationStream.Seek(0, SeekOrigin.Begin);
                                    await destinationStream.CopyToAsync(fileStream);
                                }

                                Console.WriteLine($"Фотография сохранена: {savePath}");
                            }

                            if(_chanelPostId is not null )
                            {
                                using (var imageStream = new FileStream(savePath, FileMode.Open))
                                {
                                    MemoryStream stream = new();
                                    imageStream.CopyTo(stream);
                                    stream.Position = 0;
                                    var imageInput = new InputFileStream(stream, $"{textToEncode}.jpg");

                                    inlineKeyboard = new(
                                      new[] {
                                         new[] { InlineKeyboardButton.WithCallbackData("Фейк",Facade.KeyboardCommand.FaildCheckPictureFromMenager.ToString()) },
                                         new[] { InlineKeyboardButton.WithCallbackData("Подтверить", Facade.KeyboardCommand.ApproveCheckPictureFromMenager.ToString()) }
                                      }
                                    );

                                    sentMessage = await botClient.SendPhotoAsync(
                                        chatId: _chanelPostId,
                                        photo: imageInput,
                                        replyMarkup: inlineKeyboard,
                                        caption: $"Дата: {DateTime.Now.ToString("dd:MM:yyyy HH:mm:ss")}\n"+
                                                 $"Телеграм пользователь: {telegramUser.TelegramChatId} {telegramUser.UserName.Replace("_","\\_")}\n"+
                                                 $"Чек на регистрацию: {conxtexMessage.UserInfo.FIO}\n"+
                                                 $"Телефон: {conxtexMessage.UserInfo.Phone}",
                                        parseMode: ParseMode.Markdown
                                    );
                                }
                            }
                            


                            // Создание QR-кода
                            var barcode = BarcodeWriter.CreateBarcode(textToEncode, BarcodeEncoding.QRCode);
                            var qrCodePath = $"downloaded_photos/{textToEncode}_QrCode.jpg";
                            barcode.Image.SaveAs(qrCodePath);

                            using (var imageStream = new FileStream(qrCodePath, FileMode.Open))
                            {
                                MemoryStream stream = new();
                                imageStream.CopyTo(stream);
                                stream.Position = 0;
                                var imageInput = new InputFileStream(stream, "QRCode");

                                sentMessage = await botClient.SendPhotoAsync(
                                    chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                    photo: imageInput,
                                    caption: "Ваш QR код для входа:"
                                );
                            }

                            _messageContexts.TryRemove(new KeyValuePair<long, TelegramBotMessageContext>(telegramUser.TelegramChatId, conxtexMessage));
                            break;
                        }
                        case TelegramUserState.RegistrationCustomerCompany:
                        {
                            var nameCustomerCompany = messageText;
                            if (string.IsNullOrEmpty(nameCustomerCompany))
                            {
                                sentMessage = await botClient.SendTextMessageAsync(
                                  chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                  text: "Введены не верные данные.",
                                  replyMarkup: inlineKeyboard,
                                  cancellationToken: cancellationToken);
                            }
                            else
                            {
                                var response = await _telegramBotFacade.RegistrationCustomerCompany(telegramUser, nameCustomerCompany);

                                if (response.Success)
                                {
                                    sentMessage = await botClient.SendTextMessageAsync(
                                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                        text: response.Message,
                                        cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    sentMessage = await botClient.SendTextMessageAsync(
                                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                        text: $"Ошибка регистрации компании",
                                        cancellationToken: cancellationToken);
                                }

                            }
                            break;
                        }
                        case TelegramUserState.RegistrationTelegramBot:
                        {
                            // Ищем подстроки после ":"
                            string[] parts = messageText.Split(';');

                            // Проверяем, что есть хотя бы две части (название бота и токен)
                            if (parts.Length >= 2)
                            {
                                // Извлекаем название бота
                                string botName = parts[0].Trim();

                                // Извлекаем токен
                                string token = parts[1].Trim();

                                var cehck = await _mediator.Send(new CheckExistTelegramBotByTokenQuery { TelegramBotToken = token });
                                
                                if(cehck is not null)
                                {
                                    sentMessage = await botClient.SendTextMessageAsync(
                                                  chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                                  text: $"Бот {botName} с таким токеном уже был зарегистрирован.",
                                                  cancellationToken: cancellationToken);
                                    break;
                                }

                                //Регистрация бота
                                var registrationBotCommand = new UpdateTelegramBotCommand { 
                                    newBot = new DataBase.Entities.Entities_DBContext.TelegramBots
                                    { 
                                        OrgId = conxtexMessage.Org.Id,
                                        TelegramBotName = botName.Replace("@", string.Empty),
                                        TelegramBotToken = token,
                                        TelegramBotType = "PartyTelegramBot"
                                    } 
                                };

                                var registretionBot = await _mediator.Send(registrationBotCommand);

                                if(registretionBot.Success)
                                {
                                    _telegramBotWorkerManager.Start(registretionBot.TelegramBot.TelegramBotType, registretionBot.TelegramBot);
                                    sentMessage = await botClient.SendTextMessageAsync(
                                                  chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                                  text: $"Бот {botName} успешно зарегистрирован",
                                                  cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    sentMessage = await botClient.SendTextMessageAsync(
                                                      chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                                                      text: $"Бот {botName} не зарегистрирован.",
                                                      cancellationToken: cancellationToken);;
                                }


                            }
                            else
                            {
                                Console.WriteLine("Неверный формат сообщения");
                            }

                            break;
                        }
                        case TelegramUserState.RegistrationParty:
                        {

                            break;
                        }
                    }
                }
                break;
            }
        }

        if(sentMessage is null)
        {
            await botClient.SendTextMessageAsync(
                  chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                  text: "Вы отправили: " + messageText,
                  replyMarkup: replyKeyboardRemove,
                  cancellationToken: cancellationToken);
        }
       
        return;
    }

    public async Task<User?> FindUser(User requestTelegramUser, bool isDeleted = false)
    {
        //Ищем пользователя.
        var cqrsUser = new GetUserByChatIdQuery { TelegramChatId = requestTelegramUser.TelegramChatId, IsDeleted = isDeleted };
        var responceUser = await _mediator.Send(cqrsUser);

        return responceUser;
    }
    
    public async Task<WeatherCity?> FindWeatherCity(string city)
    {
        try
        {
            var getWeatherCityQuery = new GetWeatherCityQuery() { City = city };
            var weatherCity = await _mediator.Send(getWeatherCityQuery);
            return weatherCity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    public async Task<User?> CreateUser(User? requestTelegramUser)
    {
        var responseUser = await FindUser(requestTelegramUser);
        if (responseUser is null)
        {
            //Если не нашли то регестрируем пользователя в системе как гость.
            var registerUser = new StartTelegramBotCommand { telegramUser = requestTelegramUser };
            var response = await _mediator.Send(registerUser);
            return response.telegramUser;
        }

        return responseUser;
    }

    public abstract Task<ResponceTelegramFacade> Start(User? requestTelegramUser);

}
