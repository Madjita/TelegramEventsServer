using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Entities.QrCodeEntities;

[Table("check_company")]
public class CheckCompany : EntityBase
{
    [Column("name")]
    public string Name  { get; set; }
    
    [Column("inn")]
    public long Inn  { get; set; }
    
    [Column("retail_place")]
    public string RetailPlace { get; set; }
    
    [Column("retail_place_address")]
    public string RetailPlaceAddress { get; set; }
    
    [Column("seller_address")]
    public string? SellerAddress { get; set; }
    
    public ICollection<CheckParsedItems> CheckParsedItems { get; set; }
}