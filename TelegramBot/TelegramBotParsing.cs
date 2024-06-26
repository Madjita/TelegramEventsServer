﻿using System.Globalization;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
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
using CQRS.Query.TelegramBotInChat;
using TelegramBot.TelegramBotFactory.TelegramBotDto;
using TelegramBot.TelegramBots.MainManager;

namespace TelegramBot;

public partial class TelegramBot : IAsyncDisposable
{
    protected virtual async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update,
        CancellationToken cancellationToken)
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
                if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Administrator &&
                    update.MyChatMember.Chat.Title == "PerfectoParty")
                {
                    _chanelPostId = update.MyChatMember.Chat.Id;
                }
                else if (update.MyChatMember.NewChatMember.Status == ChatMemberStatus.Left)
                {
                    _chanelPostId = null;
                }
                
                break;
            case UpdateType.ChannelPost:
                /*
              InlineKeyboardMarkup? inlineKeyboard = new(
                new[] {
                  new[] { InlineKeyboardButton.WithCallbackData("Фейк",KeyboardCommand.FaildCheckPictureFromMenager.ToString()) },
                  new[] { InlineKeyboardButton.WithCallbackData("Подтверить", KeyboardCommand.ApproveCheckPictureFromMenager.ToString()) }
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

    protected virtual async Task HandleButton(ITelegramBotClient botClient, CallbackQuery query,
        CancellationToken cancellationToken)
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

        if (Enum.TryParse<KeyboardCommand>(command, out var parsedEnum))
        {
            switch (parsedEnum)
            {
                case KeyboardCommand.StartRegistrationFriend:
                {
                    break;
                }
                case KeyboardCommand.StartRegistrationYourSelf:
                {
                    responseText = $"Танцуете ?";
                    InlineKeyboardMarkup? inlineKeyboard = new(
                        new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Не танцующий",
                                    KeyboardCommand.StartRegistrationNoDancer.ToString())
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Танцующий",
                                    KeyboardCommand.StartRegistrationDancer.ToString())
                            }
                        }
                    );

                    await botClient.SendTextMessageAsync(
                        chatId: telegramUser.TelegramChatId,
                        text: responseText,
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                    break;
                }
                case KeyboardCommand.StartRegistrationDancer:
                {
                    responseText = $"Введите Фамилию Имя Отчетсво и номер телефона через пробелы.";

                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = command,
                        State = TelegramUserState.WritedFIOandPhoneYourSelf,
                        TelegramUser = telegramUser,
                        LastMessage = command,
                    };
                    telegramUser.TelegramChatId = telegramUser.TelegramChatId;
                    _messageContexts.AddOrUpdate(telegramUser.TelegramChatId, newContext,
                        (existingKey, existingValue) => newContext);
                    
                    await botClient.SendTextMessageAsync(
                        chatId: telegramUser.TelegramChatId,
                        text: responseText,
                        cancellationToken: cancellationToken);
                    break;
                }
                case KeyboardCommand.EditRegistrationFIOandPhone:
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

                    responseText =
                        $"Отредактируйте и пришлите мне Фамилию Имя Отчетсво и номер телефона через пробелы.\n" +
                        "Вы вводили:\n" +
                        messageContext.LastMessage;

                    messageContext.State = TelegramUserState.EditRegistrationFIOandPhone;

                    await botClient.SendTextMessageAsync(
                        chatId: telegramUser.TelegramChatId,
                        text: responseText,
                        cancellationToken: cancellationToken);
                    break;
                }
                case KeyboardCommand.BreakRegistration:
                {
                    if (_messageContexts.TryGetValue(telegramUser.TelegramChatId, out var value))
                    {
                        _messageContexts.TryRemove(
                            new KeyValuePair<long, TelegramBotMessageContext>(telegramUser.TelegramChatId, value));
                        responseText = "Регистрация остановленна.";
                        InlineKeyboardMarkup? inlineKeyboard = new(
                            new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Не танцующий",
                                        KeyboardCommand.StartRegistrationNoDancer.ToString())
                                },
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Танцующий",
                                        KeyboardCommand.StartRegistrationDancer.ToString())
                                }
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
                case KeyboardCommand.ApproveRegistrationFIOandPhone:
                {
                    if (_messageContexts.TryGetValue(telegramUser.TelegramChatId, out var value))
                    {
                        value.State = TelegramUserState.SendedCheckPicture;

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
                case KeyboardCommand.FaildCheckPictureFromMenager:
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
                case KeyboardCommand.ApproveCheckPictureFromMenager:
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
                case KeyboardCommand.ExitFromBot:
                {
                    //Выйти из чата
                    var response = await _telegramBotFacade.Exit(this, telegramUser);

                    if (response.InlineKeyboard is not null)
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
                case KeyboardCommand.ExitFromBotNo:
                {
                    await botClient.EditMessageTextAsync(
                        chatId: query.Message.Chat.Id,
                        messageId: query.Message.MessageId,
                        text: "Не хотите, как хотите.");
                    break;
                }
                case KeyboardCommand.ExitFromBotYes:
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
                case KeyboardCommand.StartRegistrationCompany:
                {
                    string responceText = "Регистрация компании.\n"
                                          + "Введите название компании:";

                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = command,
                        State = TelegramUserState.RegistrationCustomerCompany,
                        TelegramUser = telegramUser,
                        LastMessage = responceText,
                    };

                    List<List<InlineKeyboardButton>> buttons = new();
                    buttons.Add(new List<InlineKeyboardButton>());
                    buttons[0].Add(
                        InlineKeyboardButton.WithCallbackData("Завершить", $"{KeyboardCommand.CanselOperation}"));
                    InlineKeyboardMarkup? inlineKeyboard = new(buttons);

                    var added = _messageContexts.AddOrUpdate(telegramUser.TelegramChatId, newContext,
                        (existingKey, existingValue) => newContext);

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: responceText,
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                    break;
                }
                case KeyboardCommand.PageCompanyPrevPage:
                case KeyboardCommand.PageCompanyNextPage:
                {
                    var userFromBD = await FindUser(telegramUser);
                    if (userFromBD is null) break;
                    var buttons =
                        await _telegramBotFacade.PageCompanyButtons(_mediator, userFromBD, parametrs[1].ToInt());
                    InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);
                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: query.Message.Text,
                        replyMarkup: inlineKeyboardMarkup,
                        cancellationToken: cancellationToken);
                    break;
                }
                case KeyboardCommand.PageTelegramBotInOrgPrevPage:
                case KeyboardCommand.PageTelegramBotInOrgNextPage:
                {
                    var userFromBD = await FindUser(telegramUser);
                    if (userFromBD is null) break;

                    var buttons = await _telegramBotFacade.PageTelegramBotInOrgButtons(_mediator, userFromBD,
                        parametrs[1].ToInt(), parametrs[2].ToInt());

                    InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: query.Message.Text,
                        replyMarkup: inlineKeyboardMarkup,
                        cancellationToken: cancellationToken);

                    break;
                }
                case KeyboardCommand.SelectTelegramBot:
                {
                    //Выбираем Телеграм бот в организации для настройки
                    //Выводим на экран кнопки которые позволяют настроить выбранного бота.

                    var commandGetSelectTelegramBot = new GetTelegramBotByIdQuery() { Id = parametrs[1].ToInt() };
                    var selectTelegramBot = await _mediator.Send(commandGetSelectTelegramBot);
                    string responceText = string.Empty;

                    if (selectTelegramBot is null)
                    {
                        responceText = $"Выбранного бота не сущесвтует в БД.";
                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            cancellationToken: cancellationToken);
                        break;
                    }
                    
                    responceText = $"Выбран бот: \"{selectTelegramBot.TelegramBotName}\" для настройки.\n";

                    //1) Добавить кнопку отображения событий.

                    List<List<InlineKeyboardButton>> buttons = new();
                    
                    buttons.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData("Настройка событий", $"{KeyboardCommand.SettingsEvents} {commandGetSelectTelegramBot.Id}"),
                    });
                    
                    buttons.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData("Завершить", $"{KeyboardCommand.CanselOperation}")
                    });

                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: $"{responceText}",
                        cancellationToken: cancellationToken,
                        replyMarkup: inlineKeyboard);

                    break;
                }
                case KeyboardCommand.SelectCompany:
                {
                    //Выбираем организацию
                    //1) Показываем кнопку Зарегестрировать телеграм бота
                    //2) Отображаем доступные телеграм боты
                    var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                    var organization = await _mediator.Send(commandGetOrg);
                    string responceText = string.Empty;

                    if (!organization.Success)
                    {
                        responceText = $"Выбранной компании не сущесвтует в БД.";
                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        responceText = $"Выбранна компания: {organization.customerCompany.Name}\n"
                                       + "Созданные боты:\n";

                        var buttons = await _telegramBotFacade.PageTelegramBotInOrgButtons(_mediator, telegramUser,
                            organization.customerCompany.Id);
                        InlineKeyboardMarkup? inlineKeyboard = new(buttons);
                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: $"{responceText}",
                            cancellationToken: cancellationToken,
                            replyMarkup: inlineKeyboard);
                    }

                    break;
                }
                case KeyboardCommand.SelectAlreadyRegistrationCompany:
                {
                    var response = await SelectCompany(telegramUser);

                    var responceText = response.Message ?? "Ошибка";

                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = command,
                        State = TelegramUserState.SelectCompany,
                        TelegramUser = telegramUser,
                        LastMessage = responceText,
                    };
                    
                    _messageContexts.AddOrUpdate(telegramUser.TelegramChatId, newContext,
                        (existingKey, existingValue) => newContext);

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: responceText,
                        replyMarkup: response.InlineKeyboard,
                        cancellationToken: cancellationToken);
                    break;
                }
                case KeyboardCommand.StartRegistrationBot:
                {
                    var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                    var organization = await _mediator.Send(commandGetOrg);
                    string responceText = string.Empty;

                    if (!organization.Success)
                    {
                        responceText = $"Выбранной компании не сущесвтует в БД.";

                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        //Получить список доступных типов ботов для создания
                        //1) Показываем кнопку Зарегестрировать телеграм бота
                        //2) Отображаем доступные типы телеграм ботов
                        var response = await SelectTypeBot(telegramUser, organization.customerCompany.Id);

                        responceText = response.Message ?? "Ошибка";

                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            replyMarkup: response.InlineKeyboard,
                            cancellationToken: cancellationToken);
                    }

                    break;
                }
                case KeyboardCommand.SettingsAdministratorsInOrg:
                {
                    // Реализовать отображение кнопок настроек администаторов в текущей организации
                    // 1) Текущие администраторы (список)
                    // 2) Кнопка Добавить администратора по вводу имени пользователя телеграмма.
                    string responceText = string.Empty;
                    var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                    var organization = await _mediator.Send(commandGetOrg);

                    //Получить список текущих администраторов.
                    responceText = $"Организация: {organization.customerCompany?.Name}\n"
                                   + "Администраторы:\n";

                    var buttons = await _telegramBotFacade.PageAdminUsersInOrgButtons(_mediator,
                        telegramUser,
                        organization.customerCompany.Id);

                    InlineKeyboardMarkup? inlineKeyboard = new(buttons);
                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: $"{responceText}",
                        cancellationToken: cancellationToken,
                        replyMarkup: inlineKeyboard);

                    break;
                }
                case KeyboardCommand.PageAdministratorInOrgPrevPage:
                case KeyboardCommand.PageAdministratorInOrgNextPage:
                {
                    var userFromBD = await FindUser(telegramUser);
                    if (userFromBD is null) break;

                    var buttons = await _telegramBotFacade.PageAdminUsersInOrgButtons(_mediator, userFromBD,
                        parametrs[1].ToInt(), parametrs[2].ToInt());

                    InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: query.Message.Text,
                        replyMarkup: inlineKeyboardMarkup,
                        cancellationToken: cancellationToken);

                    break;
                }
                case KeyboardCommand.AddAdministratorInOrg:
                {
                    //Реализовать логику вписывания Ник нейма пользователя, чтоб потом найти его в базе и добавить к организации.
                    var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                    var responceText =
                        "Реализовать логику вписывания Ник нейма пользователя, чтоб потом найти его в базе и добавить к организации.";

                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = command,
                        State = TelegramUserState.WriteAdministratorNikNameForAddInOrg,
                        TelegramUser = telegramUser,
                        LastMessage = command,
                        Org = new Org()
                        {
                            Id = commandGetOrg.Id
                        }
                    };
                    _messageContexts.AddOrUpdate(telegramUser.TelegramChatId, newContext,
                        (existingKey, existingValue) => newContext);

                    //1) Добавить кнопку отображения событий.
                    List<List<InlineKeyboardButton>> buttons = new();
                    AddButtonCansel(buttons);

                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: $"{responceText}",
                        cancellationToken: cancellationToken,
                        replyMarkup: inlineKeyboard);
                    break;
                }
                case KeyboardCommand.SelectBotType:
                {
                    //Выбранная организация и тип создаваемого бота
                    //1) Идем создавать бота

                    var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                    var organization = await _mediator.Send(commandGetOrg);
                    string responceText = string.Empty;

                    if (!organization.Success)
                    {
                        responceText = $"Выбранной компании не сущесвтует в БД.";

                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        // var telegramBotByTypeBotId = new GetAllTelegramBotByOrgIdQuery() { OrgId = organization.customerCompany.Id };
                        // var telegramBots = await _mediator.Send(telegramBotByOrgId);
                    }

                    break;
                }
                case KeyboardCommand.PageBotTypePrevPage:
                case KeyboardCommand.PageBotTypeNextPage:
                {
                    var userFromBD = await FindUser(telegramUser);
                    if (userFromBD is null) break;
                    var buttons =
                        await _telegramBotFacade.PageBotTypeButtons(_mediator, userFromBD, parametrs[1].ToInt());
                    InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);
                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: query.Message.Text,
                        replyMarkup: inlineKeyboardMarkup,
                        cancellationToken: cancellationToken);
                    break;
                }
                case KeyboardCommand.StartCreationBot:
                {
                    //Выбранная организация и тип создаваемого бота
                    //1) Идем создавать бота

                    var commandGetOrg = new GetOrgByIdCommand() { Id = parametrs[1].ToInt() };
                    var organization = await _mediator.Send(commandGetOrg);

                    //Берем тип
                    var telegramBotTypeId = parametrs[2].ToInt();

                    //Введите информацию для регестриации бота
                    var responceText = "1) Перейдите в @BotFather:\n"
                                       + "2) Введите или выбирите из меню /newbot\n"
                                       + "3) Выбирите бота /mybots\n"
                                       + "4) Нажмите на кнпоку \"API Token\" и скопируйте токен\n"
                                       + "5) Введите информацию для регистрации вашего телеграм ботам в системе через *;*\n\n\n"
                                       + "Пример:\n"
                                       + "@TestBot; token\n\n"
                                       + "Где:\n"
                                       + "@TestBot => *название бота желательно через @*\n"
                                       + "token => *ваш скопированный из @BotFather токен*";

                    List<List<InlineKeyboardButton>> buttons = new();
                    buttons.Add(new List<InlineKeyboardButton>());
                    buttons.FirstOrDefault()
                        ?.Add(InlineKeyboardButton.WithCallbackData("Завершить", $"{KeyboardCommand.CanselOperation}"));

                    InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);

                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = command,
                        State = TelegramUserState.RegistrationTelegramBot,
                        TelegramUser = telegramUser,
                        LastMessage = command,
                        Org = organization.customerCompany,
                        TelegramBotTypeId = telegramBotTypeId
                    };

                    if (_messageContexts.ContainsKey(telegramUser.TelegramChatId))
                    {
                        _messageContexts[telegramUser.TelegramChatId] = newContext;
                    }

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: responceText,
                        cancellationToken: cancellationToken,
                        replyMarkup: inlineKeyboardMarkup,
                        parseMode: ParseMode.Markdown);

                    break;
                }
                case KeyboardCommand.EndRegistrationBot:
                {
                    break;
                }
                case KeyboardCommand.CanselOperation:
                {
                    _messageContexts.TryRemove(
                        new KeyValuePair<long, TelegramBotMessageContext>(telegramUser.TelegramChatId, null));

                    //Определить пользователь пришел первый раз или уже существует в системе ?
                    var responce = await Start(telegramUser);

                    var responceText = responce.Message ?? "Ошибка";

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: responceText,
                        cancellationToken: cancellationToken,
                        replyMarkup: responce.InlineKeyboard);

                    break;
                }
                case KeyboardCommand.SettingsEvents:
                {
                    //Добавить обраотку кнопки настройки событий
                    //1) Отобразить текущеие созданные события на выбранном боте
                    //2) Добавить кнпоку Создать событие

                    var paramsSelectBotId = parametrs[1].ToInt();
                    
                    var commandGetSelectTelegramBot = new GetTelegramBotByIdQuery() { Id = parametrs[1].ToInt() };
                    var selectTelegramBot = await _mediator.Send(commandGetSelectTelegramBot);
                    string responceText = string.Empty;

                    if (selectTelegramBot is null)
                    {
                        responceText = $"Выбранного бота не сущесвтует в БД.";
                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            cancellationToken: cancellationToken);
                        break;
                    }
                    
                    responceText = $"Выбран бот: \"{selectTelegramBot.TelegramBotName}\" для настройки событий.\n";

                    //1) Добавить кнопку отображения событий.

                    List<List<InlineKeyboardButton>> buttons = new();
                    
                    buttons.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData("Создать событие", $"{KeyboardCommand.StartCreateEvent} {commandGetSelectTelegramBot.Id}"),
                    });
                    
                    AddButtonCansel(buttons);

                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: $"{responceText}",
                        cancellationToken: cancellationToken,
                        replyMarkup: inlineKeyboard);
                    
                    break;
                }
                case KeyboardCommand.StartCreateEvent:
                {
                    var commandGetSelectTelegramBot = new GetTelegramBotByIdQuery() { Id = parametrs[1].ToInt() };
                    var selectTelegramBot = await _mediator.Send(commandGetSelectTelegramBot);
                    string responceText = string.Empty;

                    if (selectTelegramBot is null)
                    {
                        responceText = $"Выбранного бота не сущесвтует в БД.";
                        await botClient.EditMessageTextAsync(
                            chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                            messageId: query.Message.MessageId,
                            text: responceText,
                            cancellationToken: cancellationToken);
                        break;
                    }
                    
                    var newContext = new MessageContext.TelegramBotMessageContext()
                    {
                        LastCommand = command,
                        State = TelegramUserState.WriteEvent,
                        TelegramUser = telegramUser,
                        LastMessage = command,
                        SelectTelegramBot = selectTelegramBot
                    };
                    
                    _messageContexts.AddOrUpdate(telegramUser.TelegramChatId, newContext,
                        (existingKey, existingValue) => newContext);
                    
                    //Введите информацию для регестриации бота
                    responceText = $"1) Введите Название события.\n"
                                   + "2) Дата проведения события (дд.мм.гггг).\n"
                                   + "3) Стоимость (число).\n"
                                   + "4) Стоимость если есть репост (число).\n"
                                   + "5) Стоимость в день события (число).\n"
                                   + "6) Напишите \"да\", если хотите, чтоб новому клиенту событие было бесплатно.\n"
                                   + "7) Название канала куда будет приходить информация для администраторов.\n"

                                   + "Cимовл *;* обязателен в конце каждого типа данных, кроме последней строки. Дату вводить в формате *дд.мм.гггг*\n\n\n"
                                   + "Пример:\n"
                                   + "Новогодняя вечеринка;\n"
                                   + "27.05.2024;\n"
                                   + "100;\n"
                                   + "100;\n"
                                   + "100;\n"
                                   + "нет;\n"
                                   + "PerfectoParty;\n";
                    
                    List<List<InlineKeyboardButton>> buttons = new();
                    TelegramBot.AddButtonCansel(buttons);
                    var inlineKeyboard = new InlineKeyboardMarkup(buttons);
                    
                    await botClient.EditMessageTextAsync(
                        chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                        messageId: query.Message.MessageId,
                        text: responceText,
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);

                    break;
                }
            }
        }

        return;
    }

    protected virtual async Task HandleMessage(ITelegramBotClient botClient, Telegram.Bot.Types.Update update,
        CancellationToken cancellationToken)
    {
        User telegramUser = null;

        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } && message.Photo is { } && message.Photo.Length > 0)
        {
            //Получили только фотку.
            Logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info,
                $"Received only photo message from FirstName: [{message.Chat.FirstName}]. LastName: [{message.Chat.LastName}]. ChatId {message.Chat.Id}.");

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

        Logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info,
            $"Received a '{messageText}' message from FirstName: [{message.Chat.FirstName}]. LastName: [{message.Chat.LastName}]. ChatId {message.Chat.Id}.");

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
    protected virtual async void CommandParser(ITelegramBotClient botClient, Message message, string messageText,
        User telegramUser, CancellationToken cancellationToken = default, bool adminCommand = false,
        string[]? parametrs = null)
    {
        var replyKeyboardRemove = new ReplyKeyboardRemove();
        Message? sentMessage = null;
        InlineKeyboardMarkup? inlineKeyboard = null;

        _messageContexts.TryGetValue(telegramUser.TelegramChatId, out var conxtexMessage);


        if (messageText.StartsWith("/analysis"))
        {
            var text = @"📊 Аналитика посещений танцевальных занятий 📊
Период: с 01.04.2024 по 01.06.2024
Таблица посещений:
Дата       | Общее количество посещений | День недели
--------------------------------------------------------
2024-04-01 | 3                          | Понедельник
2024-04-02 | 5                          | Вторник
2024-04-03 | 5                          | Среда
2024-04-04 | 5                          | Четверг
2024-04-05 | 4                          | Пятница
2024-04-06 | 2                          | Суббота
2024-04-07 | 4                          | Воскресенье
...        | ...                        | ...
2024-06-01 | 5                          | Суббота

Среднее количество посещений по дням недели:
День недели | Среднее количество посещений
---------------------------------------------
Понедельник | 4.2
Вторник     | 4.5
Среда       | 4.7
Четверг     | 4.4
Пятница     | 4.3
Суббота     | 4.6
Воскресенье | 4.1";

            sentMessage = await botClient.SendTextMessageAsync(
                chatId: telegramUser.TelegramChatId <= 0 ? chatId : telegramUser!.TelegramChatId,
                text: text,
                cancellationToken: cancellationToken);
            return;
        }
        
        if (message.Text.StartsWith("/list"))
        {

            var text = @"Смоглюк Дмитрий Юрьевич +375292637500";
            try
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: text,
                    cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return;
        }

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
                text: "Вы отправили: " + messageText,
                replyMarkup: replyKeyboardRemove,
                cancellationToken: cancellationToken);
        }

        return;
    }

    public async Task<User?> FindUser(User requestTelegramUser, bool isDeleted = false)
    {
        //Ищем пользователя.
        var cqrsUser = new GetUserByChatIdQuery
            { TelegramChatId = requestTelegramUser.TelegramChatId, IsDeleted = isDeleted };
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

    public virtual async Task<ResponceTelegramFacade> SelectCompany(User user)
    {
        throw new NotImplementedException();
    }

    public virtual async Task<ResponceTelegramFacade> SelectTypeBot(User user, int orgId)
    {
        throw new NotImplementedException();
    }

    public static void AddButtonCansel(List<List<InlineKeyboardButton>> buttons)
    {
        if (buttons is null)
        {
            buttons = new List<List<InlineKeyboardButton>>();
        }

        buttons.Add(new List<InlineKeyboardButton>());
        buttons.FirstOrDefault()
            ?.Add(InlineKeyboardButton.WithCallbackData("Завершить", $"{KeyboardCommand.CanselOperation}"));
    }
}