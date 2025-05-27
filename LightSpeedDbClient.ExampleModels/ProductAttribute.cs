using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;

namespace ExampleModels;

[Model(table: "attributes")]
[TranslatableTable(table: "attributes_translations")]
public class ProductAttribute : DatabaseObject
{
    public ProductAttribute(ModelType modelType) : base(modelType)
    {
    }

    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column(name: "name")]
    public ITranslatable Name { get; set; }
    
}