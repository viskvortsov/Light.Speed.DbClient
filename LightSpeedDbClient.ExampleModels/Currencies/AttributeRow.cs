using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;

namespace ExampleModels.Currencies;

[Model(table: "attributes")]
public class AttributeRow : DatabaseObjectTableElement
{
    
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column(name: "owner_id")]
    public Guid OwnerId { get; set; }
    
    [Column(name: "attribute")]
    [ForeignKey("currency_fk", model:typeof(Attribute), columnName: "id")]
    public Guid Attribute { get; set; }
    
    [AddInfo("currency_fk", "name")]
    public String AttributeName { get; set; }
    
}