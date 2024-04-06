using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts;
using DataBase;
using CQRS.Query;

namespace CQRS;

public struct StartTelegramBotCommand : IRequest<(User? telegramUser,string password)>
{
    public User telegramUser { get; set; }
}

public class StartTelegramBotCommandHandler : MediatorInjection, IRequestHandler<StartTelegramBotCommand, (User? telegramUser, string password)>
{
    public StartTelegramBotCommandHandler(IMediator mediator) : base(mediator) { }

    public async Task<(User? telegramUser, string password)> Handle(StartTelegramBotCommand request, CancellationToken cancellationToken)
    {
        var existTelegramUser = await _mediator.Send(new GetUserByChatIdQuery { TelegramChatId = request.telegramUser.TelegramChatId });
        if(existTelegramUser is null)
        {
            //Обновили или записали пользователя в базу.
            request.telegramUser.HPassword = "guest";

            //var newUser = new User(request.telegramUser, password);

            //await _mediator.Send(new UpdateUserCommand { newUser = newUser });

            //request.telegramUser.UserId = newUser.Id;
            await _mediator.Send(new UpdateUserCommand { newUser = request.telegramUser });

            return (existTelegramUser , request.telegramUser.HPassword);
        }

        return (existTelegramUser,string.Empty);
    }
}
