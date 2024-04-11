
namespace DataBase.Coutchbase;

/// <summary>
/// Класс для DI CouchBaseAdapter в CQRS комманды.
/// Приветствуется вынесение общих методов для переиспользования в других CQRS.
/// </summary>
public abstract class CouchBaseInjection
{
    protected ICouchBaseAdapter _couchBaseAdapter;

    public CouchBaseInjection(ICouchBaseAdapter couchBaseAdapter)
    {
        _couchBaseAdapter = couchBaseAdapter;
    }
}
