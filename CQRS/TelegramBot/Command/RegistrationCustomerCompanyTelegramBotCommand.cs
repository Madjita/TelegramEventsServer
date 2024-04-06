using MediatR;
using DataBase;
using DataBase.Entities.Entities_DBContext;
using CQRS.Query.CustomerCompany;
using CQRS.Query;

namespace CQRS;

public struct RegistrationCustomerCompanyTelegramBotCommand : IRequest<(Org? customerCompany, bool created)>
{
    public Org customerCompany { get; set; }
}

public class RegistrationCustomerCompanyTelegramBotCommandHandler : MediatorInjection, IRequestHandler<RegistrationCustomerCompanyTelegramBotCommand, (Org? customerCompany, bool created)>
{
    public RegistrationCustomerCompanyTelegramBotCommandHandler(IMediator mediator) : base(mediator) { }

    public async Task<(Org? customerCompany, bool created)> Handle(RegistrationCustomerCompanyTelegramBotCommand request, CancellationToken cancellationToken)
    {
        var existCustomerCompany = await _mediator.Send(new CheckExistOrgCommand { Name = request.customerCompany.Name });
        if (!existCustomerCompany.Success)
        {
            //Обновили или записали пользователя в базу.
            var response = await _mediator.Send(new UpdateOrgCommand { newCustomerCompany = request.customerCompany });

            return (response.customerCompany, response.Success);
        }

        return (null, false);
    }
}
