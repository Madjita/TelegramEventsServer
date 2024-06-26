﻿using DataBase.Entities.Entities_DBContext;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyLoggerNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.TelegramBots.MainManager;
using TelegramBot.TelegramBots.Party;

namespace TelegramBot.TelegramBotFactory;

public class TelegramBotFactory
{
    public static TelegramBot? Create(DataBase.Entities.Entities_DBContext.TelegramBots bot, IMyLogger logger,
            IMediator mediator,
            IServiceScopeFactory scopeFactory)
    {
        return bot.TelegramBotType.TelegramBotTypeName switch
        {
            "MainManagerTelgramBot" => new MainManagerTelgramBot(0, bot.TelegramBotName, bot.TelegramBotToken, logger, mediator, scopeFactory),
            "PartyTelegramBot" => new PartyTelegramBot(bot.Id, bot.TelegramBotName, bot.TelegramBotToken, logger, mediator, scopeFactory),
            _ => null
        };
    }
}