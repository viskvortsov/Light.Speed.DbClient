using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Implementations;

namespace ExampleModels.Currencies;

[Model(table: "currency_codes")]
public class CurrencyCodeRow : DatabaseObjectTableElement
{
    
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column(name: "owner_id")]
    public Guid OwnerId { get; set; }
    
    [Column]
    public string Code { get; set; }

    public CurrencyCodeRow(Guid id, string code, Guid ownerId)
    {
        Id = id;
        Code = code;
        OwnerId = ownerId;
    }

    public CurrencyCodeRow()
    {
    }
    
}