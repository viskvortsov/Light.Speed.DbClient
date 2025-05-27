using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;

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
    
    [Column(name: "product_type_id")]
    [ForeignKey("product_type_fk", model:typeof(ProductType), columnName: "id")]
    public int ProductType { get; set; }
    
    [AddInfo("product_type_fk", "name")]
    public string ProductTypeName { get; set; }
    
    [Table]
    public DatabaseObjectTable<ExchangeRateRow> Attributes { get; set; }
    
    public string SetBeforeSave { get; set; }
    
}