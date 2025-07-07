using ExampleModels.Currencies;
using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.ExampleModels;

[Model(table: "companies")]
public class Company : DatabaseObject
{
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }

    [Column(name: "currency_id")]
    [ForeignKey("currency_fk", model:typeof(Currency), columnName: "id")]
    public Guid CurrencyId { get; set; }
    
    [AddInfo("currency_fk", "name")]
    public string CurrencyName { get; set; }
    
    [AddInfo("currency_fk", "rate1")]
    public Decimal CurrencyRate { get; set; }
    
    public string NotInDatabase { get; set; }
    
    public Company(ModelType modelType) : base(modelType)
    {
    }
    
}