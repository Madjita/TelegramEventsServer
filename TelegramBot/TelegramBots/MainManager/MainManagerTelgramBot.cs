using DataBase.Entities.Entities_DBContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyLoggerNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Facade;

namespace TelegramBot.TelegramBots.MainManager;

public class MainManagerTelgramBot : TelegramBot
{
    public string ChanelName = "PerfectoParty";

    public MainManagerTelgramBot(
        int? id,
        string name,
        string botToken,
        IMyLogger logger,
        IMediator mediator,
        IServiceScopeFactory scopeFactory)
        : base(id, name, botToken, logger, mediator, scopeFactory)
    {

    }

    private async Task<User?> MainManagerFaindUser(User? requestTelegramUser)
    {
        var responceUser = await FindUser(requestTelegramUser, true);
        if (responceUser is null)
        {
            return await FindUser(requestTelegramUser);
        }
        return responceUser;
    }

    public override async Task<ResponceTelegramFacade> Start(User? requestTelegramUser)
    {
        //Ищем пользователя.
        var responceUser = await MainManagerFaindUser(requestTelegramUser);
        if (responceUser is null)
        {
            return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated);
        }
        
        var keyboard = _telegramBotFacade.StartManagerButtons(_mediator, responceUser);
        
        return new ResponceTelegramFacade(TelegramUserState.StartRegistrationOrganization, true)
        {
            Message = "Здравствуйте.\n"+
                      "Я бот менеджер который поможет вам зарегестрировать вашу компанию в системе и создать события.",
            InlineKeyboard = keyboard
        };
    }
    
    

    public override async Task<ResponceTelegramFacade> SelectCompany(User requestTelegramUser)
    {
        //Ищем пользователя.
        var responceUser = await MainManagerFaindUser(requestTelegramUser);
        if (responceUser is null)
        {
            return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated);
        }
        
        var buttons = await _telegramBotFacade.PageCompanyButtons(_mediator, responceUser);
        
        InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);
        
        if (buttons.Count > 0)
        {
            return new ResponceTelegramFacade(TelegramUserState.StartRegistrationOrganization, true)
            {
                Message = "Выбирите компанию: ",
                InlineKeyboard = new InlineKeyboardMarkup(buttons)
            };
        }
        else
        {
            return new ResponceTelegramFacade(TelegramUserState.StartRegistrationOrganization, true)
            {
                Message = "Хотите Зарегестировать свою компанию?",
                InlineKeyboard = new InlineKeyboardMarkup(buttons)
            };
        }
    }

    public override async Task<ResponceTelegramFacade> SelectTypeBot(User requestTelegramUser, int orgId)
    {
        //Ищем пользователя.
        var responceUser = await MainManagerFaindUser(requestTelegramUser);
        if (responceUser is null)
        {
            return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated);
        }
        
        var buttons = await _telegramBotFacade.PageBotTypeButtons(_mediator, responceUser,orgId);
        
        InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);
        
        if (buttons.Count > 0)
        {
            return new ResponceTelegramFacade(TelegramUserState.StartRegistrationOrganization, true)
            {
                Message = "Выбирите тип создаваемого бота: ",
                InlineKeyboard = new InlineKeyboardMarkup(buttons)
            };
        }
        else
        {
            return new ResponceTelegramFacade(TelegramUserState.StartRegistrationOrganization, true)
            {
                Message = "Зарегестрированных типов для создания ботов не найдено. Обратитесь к администратору.",
                InlineKeyboard = new InlineKeyboardMarkup(buttons)
            };
        }
    }
}
