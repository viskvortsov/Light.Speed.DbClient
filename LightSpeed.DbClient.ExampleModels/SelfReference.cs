using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;

namespace LightSpeed.DbClient.ExampleModels;

[Model(table: "self_references")]
public class SelfReference : DatabaseObject
{
    [PrimaryKey]
    [Column]
    public Guid Id { get; set; }
    
    [Column]
    public String Name { get; set; }

    [Column(name: "self_id")]
    [ForeignKey("local_fk", model:typeof(SelfReference), columnName: "id")]
    public Guid SefId { get; set; }
    
    [AddInfo("local_fk", field: "name")]
    public Guid SefIdName { get; set; }
    
    public SelfReference(ModelType modelType) : base(modelType)
    {
    }
    
}