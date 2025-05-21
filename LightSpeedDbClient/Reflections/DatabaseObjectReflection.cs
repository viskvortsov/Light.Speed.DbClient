namespace LightSpeedDbClient.Reflections;

public class DatabaseObjectReflection
{
    
    private readonly ITableReflection _mainTableReflection;
    private readonly IConnectedTables _connectedTables;
    
    public ITableReflection MainTableReflection => _mainTableReflection;
    
    public DatabaseObjectReflection(Type type)
    {
        _mainTableReflection = new TableReflection(type);
        _connectedTables = new ConnectedTables(type);
    }
    
    public IColumnReflection GetColumnReflection(string name)
    {
        return _mainTableReflection.Columns().SingleOrDefault(x => x.Name() == name.ToLower());
    }

}