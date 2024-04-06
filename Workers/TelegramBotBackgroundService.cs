using CQRS;
using DataBase;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLoggerNamespace;
using MyLoggerNamespace.Enums;
using System.Collections.Concurrent;
using TelegramBot;
using TelegramBot.TelegramBotFactory;
using TelegramBot.TelegramBots.MainManager;
using TelegramBot.TelegramBots.Party;
using Utils;
using Utils.Managers;

namespace Workers
{
    public class TelegramBotBackgroundService : IHostedService, IDisposable
    {
        private readonly IMyLogger _logger;
        private readonly IMediator _mediator;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITelegramBotWorkerManager _telegramBotWorkerManager;

        private TelegramBot.TelegramBot _telegramBot;

        public static ConcurrentDictionary<string,TelegramBot.TelegramBot> _telegramBots = new();
        public TelegramBotBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _logger = MyLoggerNamespace.Logger.InitLogger(nameof(TelegramBotBackgroundService));
            _mediator = _scopeFactory.CreateScope().ServiceProvider.GetService<IMediator>()!;
            _telegramBotWorkerManager = _scopeFactory.CreateScope().ServiceProvider.GetService<ITelegramBotWorkerManager>()!;
        }

        private void Start(TelegramBotEventArgs e)
        {
            TelegramBot.TelegramBot? telegramBot = null;

            switch (e.Type)
            {
                case "PartyTelegramBot":
                    telegramBot = new PartyTelegramBot(e.TelegramBot.Id, e.TelegramBot.TelegramBotName, e.TelegramBot.TelegramBotToken, _logger, _mediator, _scopeFactory);
                    _ = telegramBot.SendMessage($"Application [MailParserMicroService] is starting.\nIP Addresses:\n{Diagnostic.ConcatenatedIPs}");
                    break;
            };

            if (telegramBot is not null)
                _telegramBots.TryAdd(e.TelegramBot.TelegramBotToken, telegramBot);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.WriteLine(MessageType.Info,"MyBackgroundService is starting.");

            _telegramBotWorkerManager.AddEvent(new TelegramBotWorkerManager.StartTelegramBotEventHandler(Start));

           //запустить телеграм ботов
           try
           {
               var bots = await _mediator.Send(new GetAllTelegramBotsQuery());

               foreach (var bot in bots)
               {
                   TelegramBot.TelegramBot? telegramBot = null;
                   telegramBot = TelegramBotFactory.Create(bot, _logger, _mediator, _scopeFactory);
                   if (telegramBot is not null)
                   {
                        _telegramBots.TryAdd(bot.TelegramBotToken, telegramBot);
                        await telegramBot.SendMessage($"Application [MainManagerTelgramBot][MailParserMicroService] is starting.\nIP Addresses:\n{Diagnostic.ConcatenatedIPs}");
                    }
                }
           }
           catch(Exception ex)
           {
               Console.Write(ex.ToString());
           }
           return;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.WriteLine(MessageType.Info, "MyBackgroundService is stopping.");
            foreach (var bot in _telegramBots)
            {
                await bot.Value.StopBot();
            }
            _telegramBots.Clear();

            return;
        }

        public void Dispose()
        {
            // Освобождение ресурсов, если необходимо
        }
    }
}
