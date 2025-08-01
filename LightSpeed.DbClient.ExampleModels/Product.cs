using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.ExampleModels;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace ExampleModels;

[Model(table: "products")]
[TranslatableTable(table: "products_translations")]
public class Product : DatabaseObject
{
    public Product(ModelType modelType) : base(modelType)
    {
    }

    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column(name: "name")]
    public ITranslatable Name { get; set; }
    
    [Column(name: "product_type")]
    [ForeignKey("product_type_fk", model:typeof(ProductType), columnName: "id")]
    public ProductType.Value ProductType { get; set; }
    
    [AddInfo("product_type_fk", "name")]
    public ITranslatable ProductTypeName { get; set; }

    [Table] public DatabaseObjectTable<AttributeRow> Attributes { get; set; }
    
    public string SetBeforeSave { get; set; }
    
}