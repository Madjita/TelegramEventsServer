using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyLoggerNamespace;

namespace TelegramBot.TelegramBots.Party;

public partial class PartyTelegramBot : TelegramBot
{
    public string ChanelName = "PerfectoParty";

    public PartyTelegramBot(
        int id,
        string name,
        string botToken,
        IMyLogger logger,
        IMediator mediator,
        IServiceScopeFactory scopeFactory)
        : base(id, name, botToken, logger, mediator, scopeFactory)
    {

    }
}
