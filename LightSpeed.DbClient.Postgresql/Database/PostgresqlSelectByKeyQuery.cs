using System.Text;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.Exceptions;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Reflections;

namespace LightSpeed.DbClient.Postgresql.Database;

public class PostgresqlSelectByKeyQuery(DatabaseObjectReflection reflection, IKey key, IMapper mapper) : IQuery
{
    private readonly Dictionary<string, string> _tableReplacements = new ();
    private readonly Dictionary<string, string> _translationsTableReplacements = new ();
    private readonly QueryParameters _parameters = new ();
    
    public string GetQueryText()
    {

        _parameters.Clear();
        StringBuilder sb = new StringBuilder();
        
        FillReplacements(reflection.MainTableReflection);
        sb.Append(MainRowSelectQuery());
        sb.Append(" ");
        
        List<IConnectedTable> connectedTables = reflection.ConnectedTables().ToList();
        foreach (var connectedTable in connectedTables)
        {
            FillReplacements(connectedTable.TableReflection());
            sb.Append(ConnectedTableSelectQuery(connectedTable));
            sb.Append(" ");
        }
        
        List<IConnectedTable> translationTables = reflection.TranslationTables().ToList();
        foreach (var connectedTable in translationTables)
        {
            FillReplacements(connectedTable.TableReflection());
            sb.Append(ConnectedTableSelectQuery(connectedTable));
            sb.Append(" ");
        }
        
        return sb.ToString();
    }

    public IQueryParameters Parameters()
    {
        return _parameters;
    }

    private string MainRowSelectQuery()
    {
        
        var additionalTables = reflection.MainTableReflection.ColumnsWithForeignKey().ToList();
        var additionalTables2 = reflection.MainTableReflection.AdditionalFieldsWithForeignKey().ToList();
        var additionalColumns = reflection.MainTableReflection.AdditionalFields().ToList();
        var additionalColumns2 = reflection.MainTableReflection.AdditionalFields2().ToList();

        List<IColumnReflection> allTranslationJoins = new List<IColumnReflection>();
        var translatableFields = reflection.MainTableReflection.TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins.Add(translatableField);
            }
        }
        
        List<IColumnReflection> allTranslationJoins2 = new List<IColumnReflection>();
        var translatableFields2 = reflection.MainTableReflection.AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins2.Add(translatableField);
            }
        }
        
        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT");
        sb.Append($" ");
        var columns = reflection.MainTableReflection.Columns().ToList();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            sb.Append($"{reflection.MainTableReflection.QueryName()}.{column.QueryName()} as {column.QueryName()}");
            if (i < columns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }

        if (additionalColumns.Count > 0)
            sb.Append(",");

        int index0 = 0;
        foreach (var column in additionalColumns)
        {
            ITableReflection? foreignKeyTable = column.ForeignKeyTable();
            if (foreignKeyTable == null)
                throw new ReflectionException($"Foreign key table not found for {column.Name()}");
            sb.Append($"{GetTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (index0 < additionalColumns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index0++;
        }
        
        // additional fields 2
        if (additionalColumns2.Count > 0)
            sb.Append(",");
        
        int index1 = 0;
        foreach (var column in additionalColumns2)
        {
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (index1 < additionalColumns2.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index1++;
        }

        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index = 0;
        foreach (var translatableField in translatableFields)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index++;
                continue;
            }
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{translatableField.TranslationsQueryName()}");
            if (index < translatableFields.Count - 1)
                sb.Append(",");
            index++;
        }
        
        sb.Append(" ");
        
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index03 = 0;
        foreach (var translatableField in translatableFields2)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index03++;
                continue;
            }
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{translatableField.TranslationsQueryName()}");
            if (index03 < translatableFields2.Count - 1)
                sb.Append(",");
            index03++;
        }
        sb.Append(" ");
        
        // Main table
        sb.Append($"FROM {reflection.MainTableReflection.QueryName()} as {reflection.MainTableReflection.QueryName()}");
        
        // Additional tables
        foreach (var column in additionalTables)
        {
            ITableReflection? tableReflection = column.ForeignKeyTable();
            if (tableReflection == null)
                throw new ReflectionException($"Foreign key table not found for {column.ForeignKeyName()}");
            
            IColumnReflection? columnReflection = column.ForeignKeyColumn();
            if (columnReflection == null)
                throw new ReflectionException($"Foreign key column not found for {column.ForeignKeyName()}");

            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{tableReflection.QueryName()} as {GetTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{columnReflection.QueryName()}");
        }
        
        sb.Append($" ");
        
        // Additional tables 2
        foreach (var column in additionalTables2)
        {
            // LEFT JOIN attribute_types as attribute_types_3 ON product_families_attributes.attribute_type = attributes_1.attribute
            // LEFT JOIN attribute_types as attribute_types_3 ON attributes_1.attribute_type = attribute_types.id
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{column.AdditionalForeignKeyTable().QueryName()} as {GetAdditionalJoinTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column.ForeignKeyColumn())}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column)}.{column.AdditionalForeignKeyColumnName()}");
        }
        
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins)
        {
            var translationsTable = column.ForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
        }
        
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins2)
        {
            var field = column.QueryName();
            var table = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().QueryName();
            var translationsTable = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column.AdditionalForeignKeyColumn())}.{field}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
        }
        sb.Append($" ");
        
        sb.Append($"WHERE");
        sb.Append($" ");
        List<IKeyElement> keyElements = key.KeyElements().ToList();
        for (int i = 0; i < keyElements.Count; i++)
        {
            var keyPart = keyElements[i];
            object? value = keyPart.Value();
            Type type = keyPart.Type();
            value = mapper.MapToDatabaseValue(value, type);
            string parameterName = _parameters.Add(type, value);
            sb.Append($"{reflection.MainTableReflection.QueryName()}.{keyPart.Column().QueryName()} = {parameterName}");
            if (i < keyElements.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        if (allTranslationJoins.Count > 0)
        {
            sb.Append(" ");
            sb.Append("GROUP BY");
            sb.Append(" ");
            var connectedTableColumns = reflection.MainTableReflection.Columns().ToList();
            for (int i = 0; i < connectedTableColumns.Count; i++)
            {
                var column = connectedTableColumns[i];
                sb.Append($"{reflection.MainTableReflection.QueryName()}.{column.QueryName()}");
                if (i < connectedTableColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }

            if (additionalColumns.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            for (int i = 0; i < additionalColumns.Count; i++)
            {
                var column = additionalColumns[i];
                sb.Append($"{GetTableSynonym(column)}.{column.QueryName()}");
                if (i < additionalColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
            }
            
            if (additionalColumns2.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            int i0 = 0;
            foreach (var column in additionalColumns2)
            {
                ITableReflection foreignKeyTable = column.ForeignKeyTable();
                sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()}");
                if (i0 < additionalColumns2.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                i0++;
            }
            
        }
        
        sb.Append(";");
        
        return sb.ToString();
    }
    
    private string ConnectedTableSelectQuery(IConnectedTable connectedTable)
    {
        
        var additionalTables = connectedTable.TableReflection().ColumnsWithForeignKey().ToList();
        var additionalTables2 = connectedTable.TableReflection().AdditionalFieldsWithForeignKey().ToList();
        var additionalColumns = connectedTable.TableReflection().AdditionalFields().ToList();
        var additionalColumns2 = connectedTable.TableReflection().AdditionalFields2().ToList();

        List<IColumnReflection> allTranslationJoins = new List<IColumnReflection>();
        var translatableFields = connectedTable.TableReflection().TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins.Add(translatableField);
            }
        }
        
        List<IColumnReflection> allTranslationJoins2 = new List<IColumnReflection>();
        var translatableFields2 = connectedTable.TableReflection().AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                allTranslationJoins2.Add(translatableField);
            }
        }
        
        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT");
        sb.Append($" ");
            
        var connectedTableColumns = connectedTable.TableReflection().Columns().ToList();
        for (int i = 0; i < connectedTableColumns.Count; i++)
        {
            var column = connectedTableColumns[i];
            sb.Append($"{connectedTable.QueryName()}.{column.QueryName()} as {column.QueryName()}");
            if (i < connectedTableColumns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        if (additionalColumns.Count > 0)
            sb.Append(",");
        
        for (int i = 0; i < additionalColumns.Count; i++)
        {
            var column = additionalColumns[i];
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{GetTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (i < additionalColumns.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
        }
        
        // additional fields 2
        if (additionalColumns2.Count > 0)
            sb.Append(",");
        
        int index1 = 0;
        foreach (var column in additionalColumns2)
        {
            ITableReflection foreignKeyTable = column.ForeignKeyTable();
            sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()} as {foreignKeyTable.QueryName()}_{column.QueryName()}");
            if (index1 < additionalColumns2.Count - 1)
                sb.Append(", ");
            sb.Append(" ");
            index1++;
        }
        
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index = 0;
        bool atLeastOneTranslationJoin = false;
        foreach (var translatableField in translatableFields)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index++;
                continue;
            }
            atLeastOneTranslationJoin = true;
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{translatableField.TranslationsQueryName()}");
            if (index < translatableFields.Count - 1)
                sb.Append(",");
            index++;
        }
        sb.Append(" ");
        
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                sb.Append(", ");
                break;
            }
        }
        
        int index03 = 0;
        foreach (var translatableField in translatableFields2)
        {
            if (!translatableField.HasForeignKeyTable())
            {
                index03++;
                continue;
            }
            atLeastOneTranslationJoin = true;
            var field = translatableField.QueryName();
            var table = translatableField.ForeignKeyTable().QueryName();
            var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
            if (translationsTable == null)
            {
                throw new ReflectionException($"Translations table not found for column {field} in table {table}");
            }
            sb.Append("jsonb_agg");
            sb.Append("(");
            sb.Append("jsonb_build_object");
            sb.Append("(");
            sb.Append("'language_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.language_id");
            sb.Append(",");
            sb.Append("'content_id'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content_id");
            sb.Append(",");
            sb.Append("'content'");
            sb.Append(",");
            sb.Append($"{GetTranslationsTableSynonym(translatableField)}.content");
            sb.Append(")");
            sb.Append(")");
            sb.Append(" ");
            sb.Append("as");
            sb.Append(" ");
            sb.Append($"{translatableField.TranslationsQueryName()}");
            if (index03 < translatableFields2.Count - 1)
                sb.Append(",");
            index03++;
        }
        sb.Append(" ");
        
        // Main table
        sb.Append($"FROM {connectedTable.QueryName()} as {connectedTable.QueryName()}");
        
        sb.Append($" ");
        // Additional tables
        foreach (var column in additionalTables)
        {
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{column.ForeignKeyTable().QueryName()} as {GetTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{connectedTable.QueryName()}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{column.ForeignKeyColumn().QueryName()}");
        }
        
        sb.Append($" ");
        
        // Additional tables 2
        foreach (var column in additionalTables2)
        {
            // LEFT JOIN attribute_types as attribute_types_3 ON product_families_attributes.attribute_type = attributes_1.attribute
            // LEFT JOIN attribute_types as attribute_types_3 ON attributes_1.attribute_type = attribute_types.id
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{column.AdditionalForeignKeyTable().QueryName()} as {GetAdditionalJoinTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column.ForeignKeyColumn())}.{column.QueryName()}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column)}.{column.AdditionalForeignKeyColumnName()}");
        }
        
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins)
        {
            var field = column.QueryName();
            var table = column.ForeignKeyTable().QueryName();
            var translationsTable = column.ForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetTableSynonym(column)}.{field}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
        }
        sb.Append($" ");
        
        foreach (var column in allTranslationJoins2)
        {
            var field = column.QueryName();
            var table = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().QueryName();
            var translationsTable = column.AdditionalForeignKeyColumn().AdditionalForeignKeyTable().TranslationsTableQueryName();
            sb.Append(" ");
            sb.Append("LEFT JOIN");
            sb.Append(" ");
            sb.Append($"{translationsTable} as {GetTranslationsTableSynonym(column)}");
            sb.Append(" ");
            sb.Append("ON");
            sb.Append(" ");
            sb.Append($"{GetAdditionalJoinTableSynonym(column.AdditionalForeignKeyColumn())}.{field}");
            sb.Append(" ");
            sb.Append("=");
            sb.Append(" ");
            sb.Append($"{GetTranslationsTableSynonym(column)}.content_id");
        }
        
        sb.Append($" ");
        sb.Append($"WHERE");
        sb.Append($" ");
            
        List<IColumnReflection> ownerKeys = connectedTable.TableReflection().PartsOfOwnerKey().ToList();
        for (int i = 0; i < ownerKeys.Count; i++)
        {
            var keyPart = ownerKeys[i];
            IKeyElement partOfKey = key.KeyElements().First(element => element.Column().Name() == keyPart.Relation());
            var value = partOfKey.Value();
            var type = partOfKey.Type();
            value = mapper.MapToDatabaseValue(value, type);
            string parameterName =_parameters.Add(type, value);
            sb.Append($"{connectedTable.QueryName()}.{keyPart.QueryName()} = {parameterName}");
            if (i < ownerKeys.Count - 1)
            {
                sb.Append(" ");
                sb.Append("AND");
            }
            sb.Append(" ");
        }
        
        if (atLeastOneTranslationJoin)
        {
            sb.Append(" ");
            sb.Append("GROUP BY");
            sb.Append(" ");
            int index200 = 0;
            for (int i = 0; i < connectedTableColumns.Count; i++)
            {
                var column = connectedTableColumns[i];
                sb.Append($"{connectedTable.TableReflection().QueryName()}.{column.QueryName()}");
                if (index200 < connectedTableColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                index200++;
            }

            if (additionalColumns.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            index200 = 0;
            foreach (var column in additionalColumns)
            {
                sb.Append($"{GetTableSynonym(column)}.{column.QueryName()}");
                if (index200 < additionalColumns.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                index200++;
            }
            
            if (additionalColumns2.Count > 0)
            {
                sb.Append(",");
                sb.Append(" ");
            }
            
            index200 = 0;
            foreach (var column in additionalColumns2)
            {
                ITableReflection foreignKeyTable = column.ForeignKeyTable();
                sb.Append($"{GetAdditionalTableSynonym(column)}.{column.QueryName()}");
                if (index200 < additionalColumns2.Count - 1)
                    sb.Append(", ");
                sb.Append(" ");
                index200++;
            }
            
        }
        
        sb.Append(";");
        return sb.ToString();
    }

    private void FillReplacements(ITableReflection tableReflection)
    {
        _tableReplacements.Clear();
        _translationsTableReplacements.Clear();
        var additionalColumns = tableReflection.ColumnsWithForeignKey().ToList();
        List<String[]> allTranslationJoins = new List<String[]>();
        var translatableFields = tableReflection.TranslatableColumns().ToList();
        foreach (var translatableField in translatableFields)
        {
            if (translatableField.HasForeignKeyTable())
            {
                var field = translatableField.QueryName();
                var table = translatableField.ForeignKeyTable().QueryName();
                var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
                allTranslationJoins.Add([translationsTable, table, field]);
            }
        }
        
        var translatableFields2 = tableReflection.AdditionalTranslatableColumns().ToList();
        foreach (var translatableField in translatableFields2)
        {
            if (translatableField.HasForeignKeyTable())
            {
                var field = translatableField.QueryName();
                var table = translatableField.ForeignKeyTable().QueryName();
                var translationsTable = translatableField.ForeignKeyTable().TranslationsTableQueryName();
                allTranslationJoins.Add([translationsTable, table, field]);
            }
        }

        int a = 1;
        foreach (var column in additionalColumns)
        {
            string field = column.QueryName();
            string table = column.Table().QueryName();
            string foreignKeyName = column.ForeignKeyName();
            string searchField = $"{foreignKeyName}.{field}";
            _tableReplacements.Add(searchField, $"{column.ForeignKeyTable().QueryName()}_{a}");
            a++;
        }
        
        var additionalColumns2 = tableReflection.AdditionalFieldsWithForeignKey().ToList();
        foreach (var column in additionalColumns2)
        {
            string field = column.QueryName();
            string table = column.ForeignKeyTable().QueryName();
            string foreignKeyName = column.AdditionalForeignKeyName();
            string searchField = $"{foreignKeyName}.{field}";
            _tableReplacements.Add(searchField, $"{column.AdditionalForeignKeyTable().QueryName()}_{a}");
            a++;
        }
        
        foreach (var column in translatableFields)
        {
            if (column.HasForeignKeyTable())
            {
                string field = column.QueryName();
                string table = column.ForeignKeyTable().QueryName();
                string foreignKeyName = column.ForeignKeyName();
                string searchField = $"{foreignKeyName}.{field}";
                _tableReplacements.Add(searchField, $"{column.ForeignKeyTable().QueryName()}_{a}");
                _translationsTableReplacements.Add(searchField, $"{column.TranslationsQueryName()}_{a}");
                a++;
            }
        }
        
        foreach (var column in translatableFields2)
        {
            if (column.HasForeignKeyTable())
            {
                string field = column.QueryName();
                string table = column.ForeignKeyTable().QueryName();
                string foreignKeyName = column.ForeignKeyName();
                string searchField = $"{foreignKeyName}.{field}";
                _tableReplacements.Add(searchField, $"{column.ForeignKeyTable().QueryName()}_{a}");
                _translationsTableReplacements.Add(searchField, $"{column.TranslationsQueryName()}_{a}");
                a++;
            }
        }
    }

    private string GetTableSynonym(IColumnReflection column)
    {
        string field = column.QueryName();
        if (column.HasForeignKeyTable())
        {
            IColumnReflection foreignKeyColumn = column.Table().GetForeignKeyColumn(column.ForeignKeyName());
            field = foreignKeyColumn.QueryName();
        }
        
        string table = column.Table().QueryName();
        string queryName = column.ForeignKeyTable().QueryName();
        string foreignKeyName = column.ForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_tableReplacements.ContainsKey(searchField))
        {
            queryName = _tableReplacements[searchField];
        }
        return queryName;
    }
    
    private string GetAdditionalTableSynonym(IColumnReflection column)
    {
        string field = column.AdditionalForeignKeyColumn().QueryName();
        string table = column.ForeignKeyTable().QueryName();
        string queryName = column.ForeignKeyTable().QueryName();
        string foreignKeyName = column.ForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_tableReplacements.ContainsKey(searchField))
        {
            queryName = _tableReplacements[searchField];
        }
        return queryName;
    }
    
    private string GetAdditionalJoinTableSynonym(IColumnReflection column)
    {
        string field = column.QueryName();
        string table = column.ForeignKeyTable().QueryName();
        string queryName = column.ForeignKeyTable().QueryName();
        string foreignKeyName = column.AdditionalForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_tableReplacements.ContainsKey(searchField))
        {
            queryName = _tableReplacements[searchField];
        }
        return queryName;
    }
    
    private string GetTranslationsTableSynonym(IColumnReflection column)
    {
        string field = column.QueryName();
        string table = column.ForeignKeyTable().QueryName();
        string queryName = column.TranslationsQueryName();
        string foreignKeyName = column.ForeignKeyName();
        string searchField = $"{foreignKeyName}.{field}";
        if (_translationsTableReplacements.ContainsKey(searchField))
        {
            queryName = _translationsTableReplacements[searchField];
        }
        return queryName;
    }
    
}