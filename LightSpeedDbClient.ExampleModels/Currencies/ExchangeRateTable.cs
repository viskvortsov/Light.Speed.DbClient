using System.Collections;
using LightSpeedDbClient.Models;

namespace ExampleModels.Currencies;

public class ExchangeRateTable : IObjectArray
{
    
    public IEnumerator<IObjectArrayElement> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
}