using LightSpeedDbClient.Exceptions;

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

        foreach (var table in _connectedTables)
        {
            foreach (var column in table.PartsOfOwnerKey())
            {
                if (GetPrimaryKeyPart(column.Relation()) == null)
                {
                    throw new ModelSetupException("Relation not found");
                }
            }
        }
        
    }
    
    public IColumnReflection GetColumnReflection(string name)
    {
        return _mainTableReflection.Columns().SingleOrDefault(x => x.Name() == name.ToLower());
    }
    
    public IColumnReflection GetPrimaryKeyPart(string name)
    {
        return _mainTableReflection.PartsOfPrimaryKey().SingleOrDefault(x => x.Name() == name.ToLower());
    }
    
    public IConnectedTables ConnectedTables()
    {
        return _connectedTables;
    }

}