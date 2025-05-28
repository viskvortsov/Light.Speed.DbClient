using System.Collections;

namespace LightSpeed.DbClient.Models;

public class PaginatedDataSelection<T>(long page, long rowsPerBatch, IEnumerable<T> data, long totalRows) : IDataSelection<T>
    where T : IDatabaseElement
{
    public bool PaginationUsed => true;
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