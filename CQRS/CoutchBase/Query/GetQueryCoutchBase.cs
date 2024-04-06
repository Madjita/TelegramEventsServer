using Couchbase.KeyValue;
using DataBase.Contexts;
using DataBase.Coutchbase;
using MediatR;
using Microsoft.Extensions.Options;

namespace CQRS;

public struct GetQueryCoutchBase : IRequest<IGetResult?>
{
    public string Key { get; set; }
}

public class GetQueryCoutchBaseHandler : CouchBaseInjection, IRequestHandler<GetQueryCoutchBase, IGetResult?>
{
    public GetQueryCoutchBaseHandler(ICouchBaseAdapter couchBaseAdapter) : base(couchBaseAdapter) { }

    public async Task<IGetResult?> Handle(GetQueryCoutchBase request, CancellationToken cancellationToken)
    {
        var response = await _couchBaseAdapter.TryGetAsync(request.Key);
        return response;
    }
}
