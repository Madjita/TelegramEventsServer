using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using User = DataBase.Entities.Entities_DBContext.User;

namespace TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

public class RegistrationCustomerCompanyCommandContextStrategy : BaseCommandContextStrategy
{
    public RegistrationCustomerCompanyCommandContextStrategy(
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
        Message? sentMessage = null; 
        
        var nameCustomerCompany = messageText;
        if (string.IsNullOrEmpty(nameCustomerCompany))
        {
            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                                    chatId: telegramUser.TelegramChatId,
                                    text: "Введены не верные данные.",
                                    replyMarkup: _inlineKeyboard,
                                    cancellationToken: cancellationToken);
        }
        else
        {
            var response =
                await _telegramBotFacade.RegistrationCustomerCompany(telegramUser,
                    nameCustomerCompany);

            if (response.Success)
            {
                responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                    chatId: telegramUser.TelegramChatId ,
                    text: response.Message,
                    cancellationToken: cancellationToken);

                //TODO:CanselOperation
                responseDto.DeleteCurrentContext = true;
                //Определить пользователь пришел первый раз или уже существует в системе ?

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
                
            }
            else
            {
                responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                    chatId: telegramUser.TelegramChatId,
                    text: $"Ошибка регистрации компании",
                    cancellationToken: cancellationToken);
            }
        }

        return responseDto;
    }
}