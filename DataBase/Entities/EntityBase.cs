using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Entities;

//dotnet ef --startup-project ../AuthDomain  --verbose migrations add test
/// <summary>
///     Base class every Database Entity must inherit
/// </summary>
public abstract class EntityBase
{
    [Key]
    [Column("id")]
    public virtual int Id { get; set; }

    //[DataType(DataType.Date)]
    //public DateTimeOffset CreatedAt { get; set; }

    //[DataType(DataType.Date)]
    //public DateTimeOffset UpdatedAt { get; set; }

    public bool IsNew()
    {
        return Id == default;
    }
}