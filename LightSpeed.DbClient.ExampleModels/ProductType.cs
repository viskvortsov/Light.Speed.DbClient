using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.ExampleModels;

[Model(table: "product_types")]
[TranslatableTable(table: "product_type_translations")]
public class ProductType : DatabaseObject
{
    public ProductType(ModelType modelType) : base(modelType)
    {
    }

    [PrimaryKey]
    [Column]
    public ProductType.Value Id { get; set; }
    
    [Column(name: "name")]
    public ITranslatable Name { get; set; }
    
    public enum Value
    {
        Empty = 0,
        Product = 1,
        Service = 2,
    }
    
}