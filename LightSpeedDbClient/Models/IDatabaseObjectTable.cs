namespace LightSpeedDbClient.Models;

public interface IDatabaseObjectTable<E> : ICollection<E> where E : IDatabaseObject
{
}