using ExampleModels;
using ExampleModels.Currencies;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Implementations;
using LightSpeedDBClient.Postgresql.Database;

namespace LightSpeedDbClient.Tests;

public class Tests
{
    [SetUp]
    public async Task Setup()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();
        
        IManager<Company> coManager = new PostgresqlManager<Company>(connection, transaction);
        IManager<Currency> manager = new PostgresqlManager<Currency>(connection, transaction);
        
        await coManager.DeleteAsync();
        await manager.DeleteAsync();
        
        await transaction.CommitAsync();
        await transaction.DisposeAsync();
        await connection.DisposeAsync();
        await db.DisposeAsync();
        
    }
    
    [Test]
    public async Task Test3()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();
        
        IManager<Currency> manager = new PostgresqlManager<Currency>(connection, transaction);

        List<Currency> currencies = new List<Currency>();
        foreach (int i in Enumerable.Range(1, 5))
        {
            Currency currency = manager.CreateObject();
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
            currencies.Add(currency);
        }
        
        foreach (int i in Enumerable.Range(1, 5))
        {
            Currency currency = manager.CreateObject();
            currency.Id = Guid.NewGuid();
            currency.Name = "USD";
            currency.Deleted = "dj";
            currency.ExchangeRates = new DatabaseObjectTable<ExchangeRateRow>();
            currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  1, currency.Id));
            currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  2, currency.Id));
            currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  3, currency.Id));
        
            currency.Codes = new DatabaseObjectTable<CurrencyCodeRow>();
            currency.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "USD", currency.Id));
            currency.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "864", currency.Id));
            currencies.Add(currency);
        }
        
        foreach (int i in Enumerable.Range(1, 5))
        {
            Currency currency = manager.CreateObject();
            currency.Id = Guid.NewGuid();
            currency.Name = "KRW";
            currency.Deleted = "dj";
            currency.ExchangeRates = new DatabaseObjectTable<ExchangeRateRow>();
            currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  1, currency.Id));
            currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  2, currency.Id));
            currency.ExchangeRates.Add(new ExchangeRateRow(Guid.NewGuid(),  3, currency.Id));
        
            currency.Codes = new DatabaseObjectTable<CurrencyCodeRow>();
            currency.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "USD", currency.Id));
            currency.Codes.Add(new CurrencyCodeRow(Guid.NewGuid(),  "864", currency.Id));
            currencies.Add(currency);
        }

        IEnumerable<Currency> currencies2 = await manager.SaveManyAsync(currencies);
        Assert.NotNull(currencies2);
        Assert.That(currencies2.Count(), Is.EqualTo(15));

        IFilters<Currency> filters = manager.CreateFilters();
        filters.Add(new Filter<Currency>("name", ComparisonOperator.Equals, "Euro"));
        
        IEnumerable<Currency> currencies3 = await manager.GetListAsync(filters,1, 100);
        Assert.NotNull(currencies3);
        Assert.That(currencies3.Count(), Is.EqualTo(10));
        
        await transaction.CommitAsync();

        await transaction.DisposeAsync();
        await connection.DisposeAsync();
        await db.DisposeAsync();

    }

    [Test]
    public async Task Test2()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();
        
        IManager<Currency> manager = new PostgresqlManager<Currency>(connection, transaction);

        List<Currency> currencies = new List<Currency>();
        foreach (int i in Enumerable.Range(1, 10000))
        {
            Currency currency = manager.CreateObject();
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
            currencies.Add(currency);
        }

        IEnumerable<Currency> currencies2 = await manager.SaveManyAsync(currencies);
        Assert.NotNull(currencies2);
        Assert.That(currencies2.Count(), Is.EqualTo(10000));
        
        await transaction.CommitAsync();

        await transaction.DisposeAsync();
        await connection.DisposeAsync();
        await db.DisposeAsync();

    }

    [Test]
    public async Task Test1()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();

        IManager<Company> coManager = new PostgresqlManager<Company>(connection, transaction);
        IManager<Currency> manager = new PostgresqlManager<Currency>(connection, transaction);
        
        await coManager.DeleteAsync();
        await manager.DeleteAsync();
        
        IEnumerable<Currency> currencies = await manager.GetListAsync(1, 100);
        
        Assert.NotNull(currencies);
        Assert.That(currencies.Count(), Is.EqualTo(0));
        
        Currency currency = manager.CreateObject();
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
        
        
        Company company = coManager.CreateObject();
        company.Id = Guid.NewGuid();
        company.CurrencyId = currency2.Id;
        await coManager.SaveAsync(company);
        IEnumerable<Company> companies1 = await coManager.GetListAsync(1, 100);
        IEnumerable<Company> companies2 = await coManager.GetListObjectsAsync(1, 100);
        
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
        
        Currency currency10 = manager.CreateObject();
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
        
        Filters<Currency> filters = new ()
        {
            new Filter<Currency>("name", ComparisonOperator.Equals, "Euro"),
            new Filter<Currency>("deleted", ComparisonOperator.NotEquals, "dj")
        };
        IEnumerable<Currency> currencies2 = await manager.GetListAsync(filters,1, 100);
        Assert.NotNull(currencies2);
        Assert.That(currencies2.Count(), Is.EqualTo(0));
        
        Filters<Currency> filters2 = new ()
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
    
     [Test]
    public async Task Test4()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();

        IManager<Company> coManager = new PostgresqlManager<Company>(connection, transaction);
        IManager<Currency> manager = new PostgresqlManager<Currency>(connection, transaction);
        
        await coManager.DeleteAsync();
        await manager.DeleteAsync();
        
        IEnumerable<Currency> currencies = await manager.GetListAsync(1, 100);
        
        Assert.NotNull(currencies);
        Assert.That(currencies.Count(), Is.EqualTo(0));
        
        Currency currency = manager.CreateObject();
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
        
        Filters<Currency> filters = new ()
        {
            new Filter<Currency>("exchangerates.rownumber", ComparisonOperator.Equals, 2),
            new Filter<Currency>("exchangerates.rownumber", ComparisonOperator.Equals, 2)
        };
        IEnumerable<Currency> currencies3 = await manager.GetListAsync(filters,1, 100);
        Assert.NotNull(currencies3);
        Assert.That(currencies3.Count(), Is.EqualTo(1));
        
        IEnumerable<Currency> currencies4 = await manager.GetListObjectsAsync(filters,1, 100);
        Assert.NotNull(currencies4);
        Assert.That(currencies4.Count(), Is.EqualTo(1));
        Assert.That(currencies4.First().ExchangeRates.Count, Is.EqualTo(3));
        
        await transaction.CommitAsync();

        await transaction.DisposeAsync();
        await db.DisposeAsync();

    }
    
}