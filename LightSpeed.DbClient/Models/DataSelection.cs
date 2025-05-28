using System.Collections;

namespace LightSpeed.DbClient.Models;

public class DataSelection<T>(IEnumerable<T> data) : IDataSelection<T>
    where T : IDatabaseElement
{
    public bool PaginationUsed => false;
    public long Page => 0;
    public long RowsPerBatch => 0;
    public long Count => data.Count();
    public long TotalRows => data.Count();
    
    public IEnumerator<T> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}