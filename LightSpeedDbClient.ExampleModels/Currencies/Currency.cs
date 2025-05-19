using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Implementations;

namespace ExampleModels.Currencies;

[Model(table: "currencies")]
public class Currency : DatabaseObject
{
    
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column]
    public String Name { get; set; }
    
    [Column]
    public String Deleted { get; set; }
    
    [Column]
    public DateTime DeletedAt { get; set; }
    
    [Column]
    public bool Upload { get; set; }
    
    [Column]
    public Decimal rate1 { get; set; }
    
    [Column]
    public float rate2 { get; set; }
    
    [Column]
    public int rate3 { get; set; }
    
    [Column]
    public double rate4 { get; set; }
    
    [Column]
    public float rate5 { get; set; }
    
}