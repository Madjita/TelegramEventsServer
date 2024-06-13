using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

namespace TelegramBot.TelegramBots.MainManager;

public class ContextCommandFactory
{
    public static ICommandContextStrategy? GetBaseStrategy(
        Message message,
        ITelegramBotClient botClient, 
        IMediator mediator,
        TelegramBotMessageContext currentContextMessage, 
        IServiceScopeFactory scopeFactory,
        ITelegramBotFacade telegramBotFacade
        )
    {
        return currentContextMessage.State switch
        { 
            TelegramUserState.WritedFIOandPhoneYourSelf or TelegramUserState.EditRegistrationFIOandPhone => new EditRegistrationFIOandPhoneCommandContextStrategy(message, botClient, mediator,
                currentContextMessage, scopeFactory, telegramBotFacade),
            TelegramUserState.SendedCheckPicture => new SendedCheckPictureCommandContextStrategy(message, botClient, mediator,
                currentContextMessage, scopeFactory, telegramBotFacade), 
            TelegramUserState.RegistrationCustomerCompany => new RegistrationCustomerCompanyCommandContextStrategy(message, botClient, mediator,
                currentContextMessage, scopeFactory, telegramBotFacade), 
            TelegramUserState.RegistrationTelegramBot => new RegistrationTelegramBotCommandContextStrategy(message, botClient, mediator,
                currentContextMessage, scopeFactory, telegramBotFacade), 
            TelegramUserState.RegistrationParty => new RegistrationPartyCommandContextStrategy(message, botClient, mediator,
                currentContextMessage, scopeFactory, telegramBotFacade), 
            TelegramUserState.WriteAdministratorNikNameForAddInOrg => new WriteAdministratorNikNameForAddInOrgCommandContextStrategy(message, botClient, mediator,
                currentContextMessage, scopeFactory, telegramBotFacade), 
            TelegramUserState.WriteEvent => new WriteEventCommandContextStrategy(message, botClient, mediator,
                currentContextMessage, scopeFactory, telegramBotFacade),
            _ => null
        };
    }
}