using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public class GuidKey<E> : IKey where E : IDatabaseElement
{
    
    private readonly IColumnReflection _column;
    private readonly Guid _id;
    
    public GuidKey(Guid id)
    {
        DatabaseObjectReflection reflection = ClientSettings.GetReflection(typeof(E));
        if (reflection.MainTableReflection.PartsOfPrimaryKey().Count() > 1)
            throw new ModelSetupException(
                "GuidKey can only be used with a single primary key column"
            );
        _column = reflection.MainTableReflection.PartsOfPrimaryKey().First();
        _id = id;
    }
    
    public IEnumerable<IKeyElement> KeyElements()
    {
        return new List<IKeyElement>() {new KeyElement(_column, _id)};
    }

    public object GetValue(string name)
    {

        if (name != _column.Name())
        {
            throw new ReflectionException();
        }
        
        return _id;
        
    }
    
}