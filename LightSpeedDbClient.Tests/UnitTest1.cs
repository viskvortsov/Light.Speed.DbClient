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
        
        await manager.DeleteAsync();
        
        IEnumerable<Currency> currencies = await manager.GetListAsync(1, 100);
        
        Assert.NotNull(currencies);
        Assert.That(currencies.Count(), Is.EqualTo(0));
        
        Currency currency = manager.Create();
        Assert.NotNull(currency);
        
        currency.Id = Guid.NewGuid();
        currency.Name = "Euro";
        currency.Deleted = "dj";
        currency.ExchangeRates = new DatabaseObjectTable<ExchangeRateRow>();
        currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  1, currency.Id));
        currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  2, currency.Id));
        currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  3, currency.Id));
        
        currency.Codes = new DatabaseObjectTable<CurrencyCodeRow>();
        currency.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "USD", currency.Id));
        currency.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "864", currency.Id));
        
        Currency currency2 = await manager.SaveAsync(currency);
        
        Assert.NotNull(currency2);
        Assert.That(currency2.Id, Is.EqualTo(currency.Id));
        Assert.That(currency2.Name, Is.EqualTo(currency.Name));
        Assert.That(currency2.Deleted, Is.EqualTo(currency.Deleted));
        Assert.That(currency2.ExchangeRates.Count, Is.EqualTo(3));
        Assert.That(currency2.Codes.Count, Is.EqualTo(2));
        
        Currency currency3 = await manager.GetByKeyAsync(new GuidKey<Currency>(currency2.Id));
        
        Assert.NotNull(currency3);
        Assert.That(currency3.Id, Is.EqualTo(currency2.Id));
        Assert.That(currency3.Name, Is.EqualTo(currency2.Name));
        Assert.That(currency3.Deleted, Is.EqualTo(currency2.Deleted));
        
        Currency currency4 = await manager.SaveAsync(currency3);
        
        Assert.NotNull(currency4);
        Assert.That(currency4.Id, Is.EqualTo(currency3.Id));
        Assert.That(currency4.Name, Is.EqualTo(currency3.Name));
        Assert.That(currency4.Deleted, Is.EqualTo(currency3.Deleted));
        
        Currency currency10 = manager.Create();
        currency10.Id = Guid.NewGuid();
        currency10.Name = "Euro";
        currency10.Deleted = "dj";
        currency10.ExchangeRates = new DatabaseObjectTable<ExchangeRateRow>();
        currency10.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  1, currency10.Id));
        currency10.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  2, currency10.Id));
        currency10.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  3, currency10.Id));
        
        currency10.Codes = new DatabaseObjectTable<CurrencyCodeRow>();
        currency10.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "USD", currency10.Id));
        currency10.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "864", currency10.Id));
        
        IEnumerable<Currency> elements = await manager.SaveManyAsync(new List<Currency>(){currency10, currency4});
        
        List<IFilter> filters = new List<IFilter>()
        {
            new Filter<Currency>("name", ComparisonOperator.Equals, "Euro"),
            new Filter<Currency>("deleted", ComparisonOperator.NotEquals, "dj")
        };
        IEnumerable<Currency> currencies2 = await manager.GetListAsync(filters,1, 100);
        Assert.NotNull(currencies2);
        Assert.That(currencies2.Count(), Is.EqualTo(0));
        
        List<IFilter> filters2 = new List<IFilter>()
        {
            new Filter<Currency>("name", ComparisonOperator.Equals, "Euro"),
            new Filter<Currency>("deleted", ComparisonOperator.Equals, "dj")
        };
        IEnumerable<Currency> currencies3 = await manager.GetListAsync(filters2,1, 100);
        Assert.NotNull(currencies3);
        Assert.That(currencies3.Count(), Is.EqualTo(2));
        
        IEnumerable<Currency> currencies7 = await manager.GetListObjectsAsync(filters2,1, 100);
        Assert.NotNull(currencies7);
        Assert.That(currencies7.Count(), Is.EqualTo(2));
        
        await transaction.CommitAsync();

        await transaction.DisposeAsync();
        await connection.DisposeAsync();
        await db.DisposeAsync();

    }
}