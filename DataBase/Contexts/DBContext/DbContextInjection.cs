namespace DataBase.Contexts.DBContext;

/// <summary>
/// Класс для DI Database Context в CQRS комманды.
/// Приветствуется вынесение общих методов для переиспользования в других CQRS.
/// </summary>
public abstract class DbContextInjection
{
    protected IDBContext db;

    public DbContextInjection(IDBContext dbContext)
    {
        db = dbContext;
    }
}