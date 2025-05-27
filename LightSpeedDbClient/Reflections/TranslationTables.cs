using System.Collections;
using System.Reflection;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Exceptions;

namespace LightSpeedDbClient.Reflections;

public class TranslationTables : IConnectedTables
{
    
    private readonly Dictionary<string, IConnectedTable> _tables;

    public TranslationTables(Type type)
    {
        ModelAttribute? model = type.GetCustomAttribute<ModelAttribute>();
        if (model == null)
            throw new ClassIsNotAModelException($"Model not found for type {type.Name}");

        _tables = new Dictionary<string, IConnectedTable>();
        
        TranslatableTableAttribute? translatable = type.GetCustomAttribute<TranslatableTableAttribute>();
        if (translatable != null)
        {
            PropertyInfo? property = type.GetProperty("Translations");
            if (property == null)
                throw new ReflectionException("Translations property not found"); // TODO
            TranslationsTable connectedTable = new TranslationsTable(property, translatable.Table);
            _tables.Add(connectedTable.Name(), connectedTable);
        }
    }

    public IEnumerator<IConnectedTable> GetEnumerator()
    {
        return _tables.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}