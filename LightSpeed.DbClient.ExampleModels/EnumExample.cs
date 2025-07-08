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
    
    [Column(name: "self_ref1")]
    [ForeignKey("self_ref_fk_1", model:typeof(SelfReference), columnName: "id")]
    public Guid SelfRef1 { get; set; }
    
    [AddInfo("self_ref_fk_1", "self_id")]
    [AddInfoForeignKey("self_ref_fk_2", model: typeof(SelfReference), columnName: "id")]
    public Guid SelfRef2 { get; set; }
    
    [AddInfoAdditional("self_ref_fk_2", "name")]
    public String SelfRef2Name { get; set; }
    
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