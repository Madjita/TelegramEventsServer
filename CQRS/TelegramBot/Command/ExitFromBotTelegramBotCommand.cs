using CQRS.Query.CustomerCompany;
using CQRS.Query;
using DataBase;
using DataBase.Entities.Entities_DBContext;
using MediatR;

namespace CQRS;

public struct ExitFromBotTelegramBotCommand : IRequest<bool>
{
    public User telegramUser { get; set; }
}

public class ExitFromBotTelegramBotCommandHandler : MediatorInjection, IRequestHandler<ExitFromBotTelegramBotCommand, bool>
{
    public ExitFromBotTelegramBotCommandHandler(IMediator mediator) : base(mediator) { }

    public async Task<bool> Handle(ExitFromBotTelegramBotCommand request, CancellationToken cancellationToken)
    {
        request.telegramUser.IsDeleted = true;
        var existCustomerCompany = await _mediator.Send(new UpdateUserCommand { newUser = request.telegramUser });

        return existCustomerCompany;
    }
}
