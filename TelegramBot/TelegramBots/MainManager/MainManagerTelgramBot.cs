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
        int id,
        string name,
        string botToken,
        IMyLogger logger,
        IMediator mediator,
        IServiceScopeFactory scopeFactory)
        : base(id, name, botToken, logger, mediator, scopeFactory)
    {

    }

    public override async Task<ResponceTelegramFacade> Start(User? requestTelegramUser)
    {
        //Ищем пользователя.
        var responceUser = await FindUser(requestTelegramUser, true);

        //1) Проверить существует ли у данного пользователя своя компания? 
        //2) Является ли пользователь менеджером какой либо компании? 

        if (responceUser is null)
        {
            responceUser = await FindUser(requestTelegramUser);
            if (responceUser is null)
                return new ResponceTelegramFacade(TelegramUserErrorCode.TelegramUserNotCreated);
        }

        var buttons = await _telegramBotFacade.PageCompanyButtons(_mediator, responceUser);

        InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(buttons);


        if (buttons.Count > 0)
        {
            return new ResponceTelegramFacade(TelegramUserState.StartRegistrationOrganization, true)
            {
                Message = "Выбирите компанию или зарегестрируйте новую: ",
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
}
