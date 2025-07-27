using System.Reflection;
using LightSpeed.DbClient.Attributes;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Implementations;

public abstract class DatabaseObject : IDatabaseObject
{
    private readonly DatabaseObjectReflection _reflection;
    private readonly ModelType _modelType;
    
    [TranslationsTable]
    public DatabaseObjectTable<TranslationRow> Translations { get; set; }

    protected DatabaseObject(ModelType modelType)
    {
        _reflection = ClientSettings.GetReflection(GetType());
        Translations = new DatabaseObjectTable<TranslationRow>();
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
            if (value != null && value.GetType() == typeof(TranslationRow.TranslationKey))
            {
                TranslationRow.TranslationKey key = ((TranslationRow.TranslationKey) value);
                if (key.IsGuid)
                {
                    value = key.GuidId;
                }
                else if (key.IsInt)
                {
                    value = key.IntId;
                }
            }
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

    public virtual void BeforeSave()
    {
        // TODO current implementation only supports objects with Guid or Int as primary key
        IEnumerable<IColumnReflection> partsOfPrimaryKey = _reflection.MainTableReflection.PartsOfPrimaryKey();
        if (partsOfPrimaryKey.Count() != 1)
            throw new ReflectionException("Only objects with a single primary key column are supported");

        PropertyInfo propertyKey = _reflection.MainTableReflection.PartsOfPrimaryKey().First().Property();
        object? key = propertyKey.GetValue(this);
        if (!(key is Guid) && !(key.GetType().IsEnum))
            throw new ReflectionException("Only objects with a single primary key column of type Guid or Int are supported");
        
        if (key is Guid)
        {
            Guid guidkey = (Guid) key;
            if (guidkey == Guid.Empty)
            {
                guidkey = Guid.NewGuid();
                propertyKey.SetValue(this, guidkey);
            }
        }
        
        object? objectKey = propertyKey.GetValue(this);
        foreach (IConnectedTable connectedTable in _reflection.ConnectedTables())
        {
            IEnumerable<IColumnReflection> ownerKey = connectedTable.TableReflection().PartsOfOwnerKey();
            var columnReflections = ownerKey.ToList();
            if (columnReflections.Count() != 1)
                throw new ReflectionException("Only connected table with a single owner key column are supported");
            PropertyInfo ownerKeyProperty = columnReflections.First().Property();
            IDatabaseObjectTable table = Table(connectedTable.Name());
            foreach (var row in table)
            {
                ownerKeyProperty.SetValue(row, objectKey);
            }
        }
        
        Translations.Clear();
        SaveTranslations(this, objectKey, _reflection.MainTableReflection);
        foreach (var table in _reflection.ConnectedTables())
        {
            PropertyInfo property = table.Property();
            IDatabaseObjectTable? rows = (IDatabaseObjectTable) property.GetValue(this);
            if (rows == null) continue;
            foreach (var row in rows)
            {
                SaveTranslations((IDatabaseElement) row, objectKey, table.TableReflection());
            }
        }
    }

    public virtual void BeforeDelete()
    {
    }

    public virtual void BeforeGetReference()
    {
        FillTableTranslations(this, _reflection.MainTableReflection);
    }

    public virtual void BeforeGetObject()
    {
        FillTableTranslations(this, _reflection.MainTableReflection);
        foreach (var table in _reflection.ConnectedTables())
        {
            PropertyInfo property = table.Property();
            IDatabaseObjectTable? rows = (IDatabaseObjectTable) property.GetValue(this);
            if (rows == null) continue;
            foreach (var row in rows)
            {
                FillTableTranslations((IDatabaseElement) row, table.TableReflection());
            }
            
        }
    }

    private void SaveTranslations(IDatabaseElement element, object key, ITableReflection table)
    {
        foreach (var column in table.TranslatableColumns())
        {
            
            if (!table.Columns().Contains(column))
                continue;
                
            PropertyInfo property = column.Property();
            Type type = property.PropertyType;
            object? value = property.GetValue(element);
            if (value == null) continue;
            ITranslatable? translatable = (ITranslatable) value;
            foreach (var translation in translatable.AllTranslations())
            {
                TranslationRow row = new TranslationRow();
                row.SourceId = new TranslationRow.TranslationKey(key);
                row.LanguageId = translation.Key;
                row.ContentId = translatable.GetId();
                row.Content = translation.Value;
                Translations.Add(row);
            }
            
        }
    }
    
    private void FillTableTranslations(IDatabaseElement element, ITableReflection table)
    {
        foreach (var column in table.TranslatableColumns())
        {
            ITranslatable? translatable = (ITranslatable) column.Property().GetValue(element);
            if (translatable == null)
            {
                translatable = new Translatable();
            }
            foreach (var translation in Translations)
            {
                if (translatable.GetId().Equals(translation.ContentId))
                {
                    translatable.AddTranslation(translation.LanguageId, translation.Content);
                }
            }
        }
    }
    
}