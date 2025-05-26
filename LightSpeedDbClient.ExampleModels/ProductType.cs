using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;

namespace ExampleModels;

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
    [TranslatableColumn]
    public ITranslatable Name { get; set; }

    public override void BeforeSave(){}

    public override void AfterSave(){}
    
    public enum Value
    {
        Empty = 0,
        Product = 1,
        Service = 2,
    }
    
}