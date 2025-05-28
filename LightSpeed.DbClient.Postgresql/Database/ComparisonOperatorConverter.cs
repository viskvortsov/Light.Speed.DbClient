using LightSpeed.DbClient.Database;

namespace LightSpeed.DbClient.Postgresql.Database;

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
            case ComparisonOperator.Like:
                return "LIKE";
            default:
                throw new ArgumentOutOfRangeException(nameof(comparisonOperator), comparisonOperator, null);
        }
    }
    
}