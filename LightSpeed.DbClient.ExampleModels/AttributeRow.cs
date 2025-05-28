using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace ExampleModels;

[Model(table: "product_attributes")]
public class AttributeRow : DatabaseObjectTableElement
{
    
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [OwnerKey(relation: "id")]
    [Column(name: "owner_id")]
    public Guid OwnerId { get; set; }
    
    [Column(name: "attribute")]
    [ForeignKey("attribute_fk", model:typeof(ProductAttribute), columnName: "id")]
    public Guid Attribute { get; set; }
    
    [AddInfo("attribute_fk", "name")]
    public ITranslatable AttributeName { get; set; }
    
    [Column(name: "value")]
    public ITranslatable Value { get; set; }

    public AttributeRow()
    {
    }

    public AttributeRow(ModelType modelType) : base(modelType)
    {
    }
    
}