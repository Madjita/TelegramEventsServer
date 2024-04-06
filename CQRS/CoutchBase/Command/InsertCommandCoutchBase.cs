using Couchbase.KeyValue;
using DataBase.Contexts;
using DataBase.Coutchbase;
using MediatR;
using Microsoft.Extensions.Options;

namespace CQRS;

public struct InsertCommandCoutchBase : IRequest<bool>
{
    public string Key { get; set; }
    public string Json { get; set; }
    public InsertOptions? Options { get; set; }
}

public class InsertCommandCoutchBaseHandler : CouchBaseInjection, IRequestHandler<InsertCommandCoutchBase, bool>
{
    public InsertCommandCoutchBaseHandler(ICouchBaseAdapter couchBaseAdapter):base(couchBaseAdapter) { }

    public async Task<bool> Handle(InsertCommandCoutchBase request, CancellationToken cancellationToken)
    {
        return await _couchBaseAdapter.InsertAsync(request.Key,request.Json, request.Options);
    }
}
