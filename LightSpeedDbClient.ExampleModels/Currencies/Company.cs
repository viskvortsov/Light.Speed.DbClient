using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;

namespace ExampleModels.Currencies;

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
    public int CurrencyRate { get; set; }
    
    public string NotInDatabase { get; set; }
    
    public Company(ModelType modelType) : base(modelType)
    {
    }
    
}