using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.ExampleModels;

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