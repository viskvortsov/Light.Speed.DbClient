using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace ExampleModels.Currencies;

[Model(table: "currencies")]
public class Currency : DatabaseObject
{
    public Currency(ModelType modelType) : base(modelType)
    {
    }

    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column]
    public String Name { get; set; }
    
    [Column]
    public String Deleted { get; set; }
    
    [Column(name: "deleted_at")]
    public DateTime? DeletedAt { get; set; }
    
    [Column]
    public bool Upload { get; set; }
    
    [Column]
    public Decimal rate1 { get; set; }
    
    [Column]
    public double rate2 { get; set; }
    
    [Column]
    public int rate3 { get; set; }
    
    [Column]
    public decimal rate4 { get; set; }
    
    [Column]
    public double rate5 { get; set; }
    
    [Table]
    public DatabaseObjectTable<ExchangeRateRow> ExchangeRates { get; set; }
    
    [Table]
    public DatabaseObjectTable<CurrencyCodeRow> Codes { get; set; }

}