using System.ComponentModel.DataAnnotations;
using DataBase.Entities;
using DataBase.Entities.Entities_DBContext;

namespace Authorization.Entity;

public class JsonWebToken : EntityBase
{
    public string Token { get; set; }

    [DataType(DataType.Date)]
    public DateTimeOffset ExpiresAt { get; set; }

    [DataType(DataType.Date)]
    public DateTimeOffset DeleteAfter { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; }
}