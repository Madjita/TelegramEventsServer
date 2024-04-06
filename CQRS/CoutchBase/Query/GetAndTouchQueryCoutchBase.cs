using Couchbase.KeyValue;
using DataBase.Contexts;
using DataBase.Coutchbase;
using MediatR;
using Microsoft.Extensions.Options;

namespace CQRS;

public struct GetAndTouchQueryCoutchBase : IRequest<IGetResult>
{
    public string Key { get; set; }
    public TimeSpan updateTimeLife { get; set; }
    public GetAndTouchOptions? Options { get; set; }
}

public class GetAndTouchQueryCoutchBaseHandler : CouchBaseInjection, IRequestHandler<GetAndTouchQueryCoutchBase, IGetResult>
{
    public GetAndTouchQueryCoutchBaseHandler(ICouchBaseAdapter couchBaseAdapter) : base(couchBaseAdapter) { }

    public async Task<IGetResult> Handle(GetAndTouchQueryCoutchBase request, CancellationToken cancellationToken)
    {
        //var response = await _couchBaseAdapter.GetAndTouchAsync(request.Key, request.updateTimeLife, request.Options);
        var response = await _couchBaseAdapter.GetAsync(request.Key);

        if (response is not null)
        {
            await _couchBaseAdapter.TouchAsync(request.Key, request.updateTimeLife);
        }
        
        return response;
    }
}
