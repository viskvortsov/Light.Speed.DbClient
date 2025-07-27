using LightSpeed.DbClient.Database;

namespace LightSpeed.DbClient.Reflections;

public class ForeignKeyTable : IForeignKeyTable
{
    
    private readonly Type _type;
    
    public ForeignKeyTable(Type type)
    {
        _type = type;
    }
    
    public ITableReflection TableReflection()
    {
        return ClientSettings.GetReflection(_type).MainTableReflection;
    }
}