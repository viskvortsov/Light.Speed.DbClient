namespace LightSpeedDbClient.Models;

public interface IObjectTable<E> : IEnumerable<E> where E : IObjectTableElement
{
    
}