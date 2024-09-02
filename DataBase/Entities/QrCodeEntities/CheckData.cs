using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DataBase.Entities.QrCodeEntities;

[Table("check_data")]
public class CheckData : EntityBase
{
    /// <summary>
    /// Время добавления в базу
    /// </summary>
    [Column("date")]
    public DateTime Date { get; set; } = DateTime.Now.ToUniversalTime();
    
    /// <summary>
    /// t - Дата и время
    /// </summary>
    /// <returns></returns>
    [Column("t")]
    public string T  { get; set; }
    
    /// <summary>
    /// s - Сумма чека
    /// </summary>
    /// <returns></returns>
    [Column("s")]
    public string S  { get; set; }
    
    /// <summary>
    /// fn - Номер фискального накопителя
    /// </summary>
    /// <returns></returns>
    [Column("fn")]
    public string Fn  { get; set; }
    
    /// <summary>
    /// i - Номер фискального документа
    /// </summary>
    [Column("i")]
    public string I  { get; set; }
    
    /// <summary>
    /// fp - Фискальный признак документа
    /// </summary>
    /// <returns></returns>
    [Column("fp")]
    public string Fp  { get; set; }

    /// <summary>
    /// n - Тип операции (Приход/Возврат прихода/Расход/Возврат расхода)
    /// </summary>
    /// <returns></returns>
    [Column("n")]
    public string N  { get; set; }
    
    /// <summary>
    /// true - чек был успешно обработан, по умолчанию false
    /// </summary>
    /// <returns></returns>
    [Column("processed")]
    public CheckProcessed Processed  { get; set; }
    
    public ICollection<CheckParsedItems> CheckParsedItems { get; set; }

    public string GetRawStr()
    {
        return $"t={T}&s={S}&fn={Fn}&i={I}&fp={Fp}&n={N}";
    }

    public static CheckData FromString(string data)
    {
        CheckData task = JsonConvert.DeserializeObject<CheckData>(data);
        return task;
    }
}

public enum CheckProcessed
{
    InProcess,
    Success,
    IsError,
}