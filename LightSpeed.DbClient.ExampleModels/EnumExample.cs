using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.ExampleModels;

[Model(table: "enum_examples")]
[TranslatableTable(table: "enum_example_translations")]
public class EnumExample : DatabaseObject
{
    [PrimaryKey]
    [Column]
    public EnumExample.Value Id { get; set; }
    
    [Column(name: "name")]
    public ITranslatable Name { get; set; }
    
    [Column(name: "type1")]
    [ForeignKey("product_type_fk_1", model:typeof(ProductType), columnName: "id")]
    public ProductType.Value type1 { get; set; }
    
    [AddInfo("product_type_fk_1", "name")]
    public ITranslatable ProductTypeName1 { get; set; }
    
    [Column(name: "type2")]
    [ForeignKey("product_type_fk_2", model:typeof(ProductType), columnName: "id")]
    public ProductType.Value type2 { get; set; }
    
    [AddInfo("product_type_fk_2", "name")]
    public ITranslatable ProductTypeName2 { get; set; }
    
    public EnumExample(ModelType modelType) : base(modelType)
    {
    }
    
    public enum Value
    {
        Empty = 0,
        First = 1,
        Second = 2,
    }
    
}