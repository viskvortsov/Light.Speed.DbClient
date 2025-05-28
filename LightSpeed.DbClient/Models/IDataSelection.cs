namespace LightSpeed.DbClient.Models;

public interface IDataSelection<T> : IEnumerable<T> where T : IDatabaseElement
{
    bool PaginationUsed { get; }
    long Page { get; }
    long RowsPerBatch { get; }
    long Count { get; }
    long TotalRows { get; }
}