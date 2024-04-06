using DataBase.Entities.Entities_DBContext;
using DataBase;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.Query;

namespace CQRS.TelegramBot.KeyBoardCommand;

public struct RegistrationDancerTelegramBotKeyBoardCommand : IRequest<bool>
{
    public enum RegistrationDancerState
    {
        WaitPhoneAndFIO,
        WaitConfermPersonalInformation,
        SendedBnkInformation,
        WaitCheck,
        GenerateQRCode
    }
    public RegistrationDancerState registrationDancerState { get; set; }
    public User telegramUser { get; set; }
}

public class RegistrationDancerTelegramBotKeyBoardCommandHandler : MediatorInjection, IRequestHandler<RegistrationDancerTelegramBotKeyBoardCommand, bool>
{
    public RegistrationDancerTelegramBotKeyBoardCommandHandler(IMediator mediator) : base(mediator) { }

    public async Task<bool> Handle(RegistrationDancerTelegramBotKeyBoardCommand request, CancellationToken cancellationToken)
    {
        var existTelegramUser = await _mediator.Send(new GetUserByChatIdQuery { TelegramChatId = request.telegramUser.TelegramChatId });
        if (existTelegramUser is not null)
        {
            //TODO: Реализовать логику Регистрации Танцующего клиента.s
            //1) Показать файл с разрешением сохрянять личную информацию
            //2) Запросить его Телефон и ФИО в формате: телефон, ФИО
            //3) Показать кнопку подтверждения введенных данных, либо редактировать.
            //4) Выдать реквизиты, куда переводить.
            //5) Попросить прислать чек.
            //6) Сгенерировать QR код + приветственное сообщение об успешной регистрации.

            //Обновили или записали пользователя в базу.
            return true;
        }

        return false;
    }
}

