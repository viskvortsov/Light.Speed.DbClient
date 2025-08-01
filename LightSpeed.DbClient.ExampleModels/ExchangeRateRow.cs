using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.ExampleModels;

[Model(table: "exchange_rates")]
public class ExchangeRateRow : DatabaseObjectTableElement
{
    
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column(name: "row_number")]
    public int RowNumber { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column(name: "owner_id")]
    public Guid OwnerId { get; set; }

    public ExchangeRateRow(Guid id, int rowNumber, Guid ownerId)
    {
        Id = id;
        RowNumber = rowNumber;
        OwnerId = ownerId;
    }

    public ExchangeRateRow(ModelType modelType) : base(modelType)
    {
    }
    
}