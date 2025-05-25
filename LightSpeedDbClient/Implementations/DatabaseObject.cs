using System.Reflection;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Exceptions;
using LightSpeedDbClient.Models;
using LightSpeedDbClient.Reflections;

namespace LightSpeedDbClient.Implementations;

public abstract class DatabaseObject : IDatabaseObject
{
    private readonly DatabaseObjectReflection _reflection;
    private readonly ModelType _modelType;

    protected DatabaseObject(ModelType modelType)
    {
        _reflection = ClientSettings.GetReflection(GetType());
        _modelType = modelType;
        if (_modelType == Models.ModelType.Row)
            throw new ReflectionException($"Wrong model type used {_modelType}");
    }

    public IKey Key()
    {
        IEnumerable<IColumnReflection> partsOfPrimaryKey = _reflection.MainTableReflection.PartsOfPrimaryKey();
        List<KeyElement> keyElements = new List<KeyElement>();
        foreach (IColumnReflection primaryKeyElement in partsOfPrimaryKey)
        {
            PropertyInfo property = primaryKeyElement.Property();
            if (property == null)
                throw new ReflectionException($"Property not found for column {primaryKeyElement.Name()}");
            object? value = property.GetValue(this);
            keyElements.Add(new KeyElement(primaryKeyElement, value));
        }
        return new Key(keyElements);
    }

    public IDatabaseObjectTable Table(string name)
    {
        IDatabaseObjectTable table;
        IColumnReflection columnReflection = _reflection.MainTableReflection.GetTableReflection(name);
        object? value = columnReflection.Property().GetValue(this);
        if (value == null)
        {
            ConstructorInfo? constructor = ClientSettings.GetConstructor(columnReflection.Type());
            if (constructor == null)
                throw new ReflectionException($"Constructor not found for type {columnReflection.Type()}");
        
            table = (IDatabaseObjectTable) constructor.Invoke([Models.ModelType.Row]);
            columnReflection.Property().SetValue(this, table);
        }
        else
        {
            table = (IDatabaseObjectTable)value;
        }
        return table;
    }

    public ModelType ModelType()
    {
        return _modelType;
    }

    public bool IsObject()
    {
        return _modelType == Models.ModelType.Object;
    }

    public bool IsReference()
    {
        return _modelType == Models.ModelType.Reference;
    }

    public bool IsRow()
    {
        return _modelType == Models.ModelType.Row;
    }
    public abstract void BeforeSave();
    public abstract void AfterSave();
}