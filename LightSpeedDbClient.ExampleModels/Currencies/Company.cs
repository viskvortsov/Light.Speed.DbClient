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
    [ForeignKey(tableName:"currencies", columnName: "id")]
    [AddInfo(["name", "rate1"])]
    public Guid CurrencyId { get; set; }
    
    public Company(ModelType modelType) : base(modelType)
    {
    }
    
}