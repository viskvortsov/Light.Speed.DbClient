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
            foreach (var column in table.TableReflection().PartsOfOwnerKey())
            {
                string? relation = column.Relation();
                if (relation == null)
                    throw new ModelSetupException("Relation not found");
                _ = GetPrimaryKeyPart(relation);
            }
        }
        
    }
    
    public IColumnReflection GetColumnReflection(string name)
    {
        return _mainTableReflection.Columns().SingleOrDefault(x => x.Name() == name.ToLower()) 
               ?? throw new ReflectionException($"Column {name} not found");
    }
    
    public IConnectedTable GetTableReflection(string name)
    {
        return _connectedTables.SingleOrDefault(x => x.Name() == name.ToLower())
               ?? throw new ReflectionException($"Table {name} not found");
    }

    private IColumnReflection GetPrimaryKeyPart(string name)
    {
        return _mainTableReflection.PartsOfPrimaryKey().SingleOrDefault(x => x.Name() == name.ToLower())
               ?? throw new ReflectionException($"Primary key {name} not found");
    }
    
    public IConnectedTables ConnectedTables()
    {
        return _connectedTables;
    }

}