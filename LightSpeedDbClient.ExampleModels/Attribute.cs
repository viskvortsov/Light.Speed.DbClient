using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Models;

namespace ExampleModels;

[Model(table: "attributes")]
public class Attribute
{
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column(name: "name")]
    public ITranslatable Name { get; set; }
    
}