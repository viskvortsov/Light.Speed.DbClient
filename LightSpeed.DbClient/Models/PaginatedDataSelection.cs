using System.Collections;

namespace LightSpeed.DbClient.Models;

public class PaginatedDataSelection<T>(long page, long rowsPerBatch, IEnumerable<T> data, long totalRows) : IDataSelection<T>
    where T : IDatabaseElement
{
    public bool PaginationUsed => true;
    public long Page => page;
    public long RowsPerBatch => rowsPerBatch;
    public long Count => data.Count();
    public long TotalRows => totalRows;
    
    public IEnumerator<T> GetEnumerator()
    {
        return data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}