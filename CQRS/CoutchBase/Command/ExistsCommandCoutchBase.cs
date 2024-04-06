using Couchbase.KeyValue;
using DataBase.Contexts;
using DataBase.Coutchbase;
using MediatR;
using Microsoft.Extensions.Options;

namespace CQRS;

public struct ExistsCommandCoutchBase : IRequest<bool>
{
    public string Key { get; set; }
    public ExistsOptions? Options { get; set; }
}

public class ExistsCommandCoutchBaseHandler : CouchBaseInjection, IRequestHandler<ExistsCommandCoutchBase, bool>
{
    public ExistsCommandCoutchBaseHandler(ICouchBaseAdapter couchBaseAdapter) : base(couchBaseAdapter) { }

    public async Task<bool> Handle(ExistsCommandCoutchBase request, CancellationToken cancellationToken)
    {
        return await _couchBaseAdapter.ExistsAsync(request.Key, request.Options);
    }
}
