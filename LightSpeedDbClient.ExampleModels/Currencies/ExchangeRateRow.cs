using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Models;

namespace ExampleModels.Currencies;

[Model(table: "exchange_rates")]
public class ExchangeRateRow : IObjectTableElement
{
    
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column]
    public int RowNumber { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column]
    public Guid OwnerId { get; set; }
    
}