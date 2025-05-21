namespace LightSpeedDbClient.Reflections;

public class DatabaseObjectReflection
{
    
    private readonly ITableReflection _mainTableReflection;
    public ITableReflection MainTableReflection => _mainTableReflection;
    
    public DatabaseObjectReflection(Type type)
    {
        _mainTableReflection = new TableReflection(type);
    }

}