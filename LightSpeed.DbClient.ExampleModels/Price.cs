using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.ExampleModels;

[Model(table: "prices")]
public class Price : RecordObject
{
    [PrimaryKey]
    [Column]
    public Guid Product { get; set; }
    
    [PrimaryKey]
    [Column]
    public Guid Variant { get; set; }

    [Column]
    public double ListPrice { get; set; }
    
    [Column]
    public double SalePrice { get; set; }
    
    public Price(ModelType modelType) : base(modelType)
    {
    }
    
}