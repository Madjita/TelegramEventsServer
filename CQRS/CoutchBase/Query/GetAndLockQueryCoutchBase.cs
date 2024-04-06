using Couchbase.KeyValue;
using DataBase.Contexts;
using DataBase.Coutchbase;
using MediatR;
using Microsoft.Extensions.Options;

namespace CQRS;

public struct GetAndLockQueryCoutchBase : IRequest<IGetResult>
{
    public string Key { get; set; }
    public TimeSpan lockTimeSpan { get; set; }
    public GetAndLockOptions? Options { get; set; }
}

public class GetAndLockQueryCoutchBaseHandler : CouchBaseInjection, IRequestHandler<GetAndLockQueryCoutchBase, IGetResult>
{
    public GetAndLockQueryCoutchBaseHandler(ICouchBaseAdapter couchBaseAdapter) : base(couchBaseAdapter) { }

    public async Task<IGetResult> Handle(GetAndLockQueryCoutchBase request, CancellationToken cancellationToken)
    {
        return await _couchBaseAdapter.GetAndLockAsync(request.Key, request.lockTimeSpan, request.Options);
    }
}
