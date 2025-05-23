using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Implementations;

public class DatabaseObjectTableElement : IDatabaseObjectTableElement
{
    public IKey Key()
    {
        throw new NotImplementedException();
    }

    public IKey OwnerKey()
    {
        throw new NotImplementedException();
    }
}