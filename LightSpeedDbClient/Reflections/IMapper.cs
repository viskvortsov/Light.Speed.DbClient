
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Reflections;

public interface IMapper
{
    public IDatabaseObject MapToModel(IDatabaseObject element);
    public IDatabaseObjectTableElement MapToModel(IDatabaseObjectTableElement element);
}