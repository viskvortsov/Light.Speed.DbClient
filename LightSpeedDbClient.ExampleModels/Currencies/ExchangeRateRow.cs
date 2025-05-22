using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Models;

namespace ExampleModels.Currencies;

[Model(table: "exchange_rates")]
public class ExchangeRateRow : IObjectTableElement
{
    
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column(name: "row_number")]
    public int RowNumber { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column(name: "owner_id")]
    public Guid OwnerId { get; set; }
    
}