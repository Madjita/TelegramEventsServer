using Couchbase.KeyValue;
using DataBase.Contexts;
using DataBase.Coutchbase;
using MediatR;
using Microsoft.Extensions.Options;

namespace CQRS;

public struct UpserWithUnlockCommandCoutchBase : IRequest<bool>
{
    public string Key { get; set; }
    public string Json { get; set; }
    public IGetResult lockItem { get; set; }
    public UpsertOptions? Options { get; set; }
}

public class UpserWithUnlockCommandCoutchBaseHandler : CouchBaseInjection, IRequestHandler<UpserWithUnlockCommandCoutchBase, bool>
{
    public UpserWithUnlockCommandCoutchBaseHandler(ICouchBaseAdapter couchBaseAdapter) : base(couchBaseAdapter) { }

    public async Task<bool> Handle(UpserWithUnlockCommandCoutchBase request, CancellationToken cancellationToken)
    {
        await _couchBaseAdapter.UnlockAsync(request.Key, request.lockItem.Cas);
        return await _couchBaseAdapter.UpsertAsync(request.Key, request.Json, request.Options);
    }
}
