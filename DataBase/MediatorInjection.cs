
using MediatR;

namespace DataBase;

public abstract class MediatorInjection
{
    protected IMediator _mediator;

    public MediatorInjection(IMediator mediator)
    {
        _mediator = mediator;
    }
}