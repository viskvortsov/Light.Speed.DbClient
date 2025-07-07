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