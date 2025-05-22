
using LightSpeedDbClient.Models;

namespace LightSpeedDbClient.Reflections;

public interface IMapper
{
    public IDatabaseElement MapToModel(IDatabaseElement element);
}