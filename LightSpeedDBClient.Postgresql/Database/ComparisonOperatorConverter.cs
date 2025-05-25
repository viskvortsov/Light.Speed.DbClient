using LightSpeedDbClient.Database;

namespace LightSpeedDBClient.Postgresql.Database;

public static class ComparisonOperatorConverter
{

    public static string Convert(ComparisonOperator comparisonOperator)
    {
        switch (comparisonOperator)
        {
            case ComparisonOperator.Equals:
                return "=";
            case ComparisonOperator.NotEquals:
                return "<>";
            case ComparisonOperator.GreaterThan:
                return ">";
            case ComparisonOperator.GreaterThanOrEquals:
                return ">=";
            case ComparisonOperator.LessThan:
                return "<";
            case ComparisonOperator.LessThanOrEquals:
                return "<=";
            case ComparisonOperator.In:
                return "IN";
            default:
                throw new ArgumentOutOfRangeException(nameof(comparisonOperator), comparisonOperator, null);
        }
    }
    
}