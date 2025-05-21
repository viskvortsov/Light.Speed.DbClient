using System.Collections;
using LightSpeedDbClient.Attributes;
using LightSpeedDbClient.Models;

namespace ExampleModels.Currencies;

public class ExchangeRateTable : IObjectTable<ExchangeRateRow>
{
    
    private readonly List<ExchangeRateRow> _rows = new();
    
    public IEnumerator<ExchangeRateRow> GetEnumerator()
    {
        return _rows.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}