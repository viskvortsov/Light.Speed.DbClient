using ExampleModels.Currencies;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Implementations;
using LightSpeedDBClient.Postgresql.Database;

namespace LightSpeedDbClient.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test1()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();
        
        IManager<Currency> manager = new PostgresqlManager<Currency>(connection, transaction);
        IEnumerable<Currency> currencies = await manager.GetListAsync(1, 100);
        
        Currency currency = manager.Create();
        currency.Id = Guid.NewGuid();
        currency.Name = "Euro";
        currency.Deleted = "dj";
        Currency currency2 = await manager.SaveAsync(currency);
        Currency currency3 = await manager.GetByKeyAsync(new GuidKey<Currency>(currency2.Id));
        Currency currency4 = await manager.SaveAsync(currency3);
        
        await transaction.CommitAsync();

        await transaction.DisposeAsync();
        await connection.DisposeAsync();
        await db.DisposeAsync();

    }
}