using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;

namespace CQRS.Query;

public record CheckExistXOrgUserCommand : IRequest<(bool Success, XOrgUser? xOrgUser)>
{
    public User? telegramUser { get; set; }
    public int BotId { get; set; }
}

public class CheckExistXOrgUserCommandHandler : DbContextInjection, IRequestHandler<CheckExistXOrgUserCommand, (bool Success, XOrgUser? xOrgUser)>
{
    public CheckExistXOrgUserCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, XOrgUser? xOrgUser)> Handle(CheckExistXOrgUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            IQueryable<XOrgUser> query;
            XOrgUser? result;
            
            if (request.BotId == 0)
            {
                query = from employee in db.XOrgUser
                    join company in db.Org on employee.OrgId equals company.Id
                    join user in db.User on employee.UserId equals user.Id
                    where user.TelegramChatId == request.telegramUser!.TelegramChatId
                    select new XOrgUser
                    {
                        Id = employee.Id,
                        OrgId = company.Id,
                        UserId = user.Id,
                        RoleId = employee.RoleId,
                    };

                result = await query.FirstOrDefaultAsync();

                return (result is not null, result);
            }
            
            query = from employee in db.XOrgUser
                        join bot in db.TelegramBots on request.BotId equals bot.Id
                        join company in db.Org on employee.OrgId equals company.Id
                        join user in db.User on employee.UserId equals user.Id
                        where user.TelegramChatId == request.telegramUser!.TelegramChatId && bot.OrgId == employee.OrgId
                        select new XOrgUser
                        {
                            Id = employee.Id,
                            OrgId = company.Id,
                            UserId = user.Id,
                            RoleId = employee.RoleId,
                        };

            result = await query.FirstOrDefaultAsync();

            return (result is not null, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
