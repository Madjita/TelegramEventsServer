using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Entities.QrCodeEntities;

[Table("check_parsed_items")]
public class CheckParsedItems : EntityBase
{
    [Column("name")]
    public string Name  { get; set; }
    
    [Column("sum")]
    public int Sum  { get; set; }
    
    [Column("nds")]
    public int Nds  { get; set; }
    
    [Column("price")]
    public int Price  { get; set; }
    
    [Column("ndssum")]
    public int NdsSum  { get; set; }
    
    [Column("quantity")]
    public double Quantity  { get; set; }
    
    [Column("paymenttype")]
    public int Paymenttype  { get; set; }
    
    [Column("producttype")]
    public int Producttype  { get; set; }
    
    [Column("itemsquantitymeasure")]
    public int Itemsquantitymeasure  { get; set; }
    
    [Column("check_data_id")]
    public int CheckDataId  { get; set; }
    
    [Column("check_company_id")]
    public int CheckCompanyId  { get; set; }
    
    [ForeignKey("CheckDataId")]
    public CheckData? CheckData { get; set; }
    
    [ForeignKey("CheckCompanyId")]
    public CheckCompany? CheckCompany { get; set; }
}