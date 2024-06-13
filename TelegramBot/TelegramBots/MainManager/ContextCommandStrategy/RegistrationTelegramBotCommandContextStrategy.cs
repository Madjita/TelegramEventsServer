using CQRS;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Facade;
using TelegramBot.MessageContext;
using Utils.Managers;
using User = DataBase.Entities.Entities_DBContext.User;

namespace TelegramBot.TelegramBots.MainManager.ContextCommandStrategy;

public class RegistrationTelegramBotCommandContextStrategy : BaseCommandContextStrategy
{
    private readonly ITelegramBotWorkerManager _telegramBotWorkerManager;
    public RegistrationTelegramBotCommandContextStrategy(
        Message message,
        ITelegramBotClient botClient, 
        IMediator mediator,
        TelegramBotMessageContext currentContextMessage,
        IServiceScopeFactory scopeFactory,
        ITelegramBotFacade telegramBotFacade)
        :
        base(message, botClient, mediator, currentContextMessage, scopeFactory, telegramBotFacade)
    {
        _telegramBotWorkerManager = _scopeFactory.CreateScope().ServiceProvider.GetService<ITelegramBotWorkerManager>()!;
    }

    public override async Task<CommandContextStrategyResponseDto> Execute(User telegramUser, string messageText,
        CancellationToken cancellationToken)
    {
        var responseDto = new CommandContextStrategyResponseDto();
        // Ищем подстроки после ":"
        string[] parts = messageText.Split(';');

        // Проверяем, что есть хотя бы две части (название бота и токен)
        if (parts.Length >= 2)
        {
            // Извлекаем название бота
            string botName = parts[0].Trim();

            // Извлекаем токен
            string token = parts[1].Trim();

            var cehck = await _mediator.Send(new CheckExistTelegramBotByTokenQuery
                { TelegramBotToken = token });

            if (cehck is not null)
            {
                responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                    chatId: telegramUser.TelegramChatId,
                    text: $"Бот {botName} с таким токеном уже был зарегистрирован.",
                    cancellationToken: cancellationToken);
                
                return responseDto;
            }

            //Регистрация бота
            var registrationBotCommand = new UpdateTelegramBotCommand
            {
                newBot = new DataBase.Entities.Entities_DBContext.TelegramBots {
                    OrgId = _currentContextMessage.Org.Id,
                    TelegramBotName = botName.Replace("@", string.Empty),
                    TelegramBotToken = token,
                    TelegramBotTypeId = _currentContextMessage.TelegramBotTypeId
                }
            };

            var registretionBot = await _mediator.Send(registrationBotCommand);

            if (registretionBot.Success)
            {
                _telegramBotWorkerManager.Start(
                                        registretionBot.TelegramBot.TelegramBotType.TelegramBotTypeName,
                                        registretionBot.TelegramBot);
                responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                                        chatId: telegramUser.TelegramChatId,
                                        text: $"Бот {botName} успешно зарегистрирован",
                                        cancellationToken: cancellationToken);
            }
            else
            {
                responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                                        chatId: telegramUser.TelegramChatId,
                                        text: $"Бот {botName} не зарегистрирован.",
                                        cancellationToken: cancellationToken);
                                    ;
            }
        }
        else
        {
            responseDto.SentMessage = await _botClient.SendTextMessageAsync(
                chatId: telegramUser.TelegramChatId,
                text: $"Неверный формат сообщения",
                cancellationToken: cancellationToken);
        }

        return responseDto;
    }
}