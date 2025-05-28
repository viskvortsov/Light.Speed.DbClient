using ExampleModels;
using ExampleModels.Currencies;
using LightSpeedDbClient.Database;
using LightSpeedDbClient.Implementations;
using LightSpeedDbClient.Models;
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
        
        IEnumerable<Currency> currencies3 = await manager.GetListAsync(filters, 1, 100);
        Assert.NotNull(currencies3);
        Assert.That(currencies3.Count(), Is.EqualTo(5));
        
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
    
    [Test]
    public async Task TestTranslatable()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();

        IManager<ProductType> manager = new PostgresqlManager<ProductType>(connection, transaction);
        await manager.DeleteAsync();
        IManager<Product> productManager0 = new PostgresqlManager<Product>(connection, transaction);
        await productManager0.DeleteAsync();
        
        Guid englishMock = Guid.NewGuid();
        Guid spanishMock = Guid.NewGuid();

        ProductType type1 = manager.CreateObject();
        type1.Id = ProductType.Value.Product;
        type1.Name = new Translatable();
        type1.Name.AddTranslation(englishMock, "Product");
        type1.Name.AddTranslation(spanishMock, "Producto");

        await manager.SaveAsync(type1);
        
        ProductType type2 = manager.CreateObject();
        type2.Id = ProductType.Value.Service;
        type2.Name = new Translatable();
        type2.Name.AddTranslation(englishMock, "Service");
        type2.Name.AddTranslation(spanishMock, "Servicio");
        
        ProductType type3 = manager.CreateObject();
        type3.Id = ProductType.Value.Empty;
        type3.Name = new Translatable();
        type3.Name.AddTranslation(englishMock, "Empty");
        type3.Name.AddTranslation(spanishMock, "Vacío");

        await manager.SaveManyAsync([type2, type3]);

        ISorting<ProductType> sorting = new Sorting<ProductType>();
        sorting.Add(new SortingElement<ProductType>("id", SortingDirection.Ascending));
        
        IEnumerable<ProductType> productTypes = await manager.GetListAsync(sorting, 1, 100);
        var list = productTypes.ToList();
        
        Assert.NotNull(productTypes);
        Assert.That(list.Count, Is.EqualTo(3));

        var productType3 = list[0];
        Assert.That(productType3.Id, Is.EqualTo(ProductType.Value.Empty));
        Assert.That(productType3.Name.AllTranslations().Count, Is.EqualTo(2));
        Assert.That(productType3.Name.GetTranslation(englishMock), Is.EqualTo("Empty"));
        Assert.That(productType3.Name.GetTranslation(spanishMock), Is.EqualTo("Vacío"));
        
        var productType1 = list[1];
        Assert.That(productType1.Id, Is.EqualTo(ProductType.Value.Product));
        Assert.That(productType1.Name.AllTranslations().Count, Is.EqualTo(2));
        Assert.That(productType1.Name.GetTranslation(englishMock), Is.EqualTo("Product"));
        Assert.That(productType1.Name.GetTranslation(spanishMock), Is.EqualTo("Producto"));
        
        var productType2 = list[2];
        Assert.That(productType2.Id, Is.EqualTo(ProductType.Value.Service));
        Assert.That(productType2.Name.AllTranslations().Count, Is.EqualTo(2));
        Assert.That(productType2.Name.GetTranslation(englishMock), Is.EqualTo("Service"));
        Assert.That(productType2.Name.GetTranslation(spanishMock), Is.EqualTo("Servicio"));
        
        ISorting<ProductType> sorting2 = new Sorting<ProductType>();
        sorting2.Add(new SortingElement<ProductType>("id", SortingDirection.Descending));
        
        IEnumerable<ProductType> productTypes2 = await manager.GetListObjectsAsync(sorting2, 1, 100);
        var list2 = productTypes2.ToList();
        
        var productType5 = list2[0];
        Assert.That(productType5.Id, Is.EqualTo(ProductType.Value.Service));
        Assert.That(productType5.Name.AllTranslations().Count, Is.EqualTo(2));
        Assert.That(productType5.Name.GetTranslation(englishMock), Is.EqualTo("Service"));
        Assert.That(productType5.Name.GetTranslation(spanishMock), Is.EqualTo("Servicio"));
        
        var productType4 = list2[1];
        Assert.That(productType4.Id, Is.EqualTo(ProductType.Value.Product));
        Assert.That(productType4.Name.AllTranslations().Count, Is.EqualTo(2));
        Assert.That(productType4.Name.GetTranslation(englishMock), Is.EqualTo("Product"));
        Assert.That(productType4.Name.GetTranslation(spanishMock), Is.EqualTo("Producto"));
        
        var productType6 = list2[2];
        Assert.That(productType6.Id, Is.EqualTo(ProductType.Value.Empty));
        Assert.That(productType6.Name.AllTranslations().Count, Is.EqualTo(2)); 
        Assert.That(productType6.Name.GetTranslation(englishMock), Is.EqualTo("Empty"));
        Assert.That(productType6.Name.GetTranslation(spanishMock), Is.EqualTo("Vacío"));
        
        await transaction.CommitAsync();
        
        ITransaction transaction2 = await connection.BeginTransactionAsync();
        
        IManager<ProductAttribute> attributeManager = new PostgresqlManager<ProductAttribute>(connection, transaction2);
        ProductAttribute attribute = attributeManager.CreateObject();
        attribute.Id = Guid.NewGuid();
        attribute.Name = new Translatable();
        attribute.Name.AddTranslation(englishMock, "Type");
        attribute.Name.AddTranslation(spanishMock, "typo");
        
        await attributeManager.SaveAsync(attribute);
        
        IManager<Product> productManager = new PostgresqlManager<Product>(connection, transaction2);
        Product product = productManager.CreateObject();
        product.Id = Guid.NewGuid();
        product.Name = new Translatable();
        product.Name.AddTranslation(englishMock, "Product 1");
        product.Name.AddTranslation(spanishMock, "Producto 1");
        product.ProductType = ProductType.Value.Product;

        ITranslatable value = new Translatable();
        value.AddTranslation(englishMock, "Fire!");
        AttributeRow row = new AttributeRow
        {
            Id = Guid.NewGuid(),
            OwnerId = product.Id,
            Attribute = attribute.Id,
            Value = value,
        };
        product.Attributes = new DatabaseObjectTable<AttributeRow>();
        product.Attributes.Add(row);
        
        await productManager.SaveAsync(product);
        
        await transaction.CommitAsync();
        
        IEnumerable<Product> products = await productManager.GetListAsync(1, 100);
        var list3 = products.ToList();
        
        var product1 = list3[0];
        Assert.That(product1.Translations.Count, Is.EqualTo(3));
        Assert.That(product1.Name.AllTranslations().Count, Is.EqualTo(2));
        
        IEnumerable<Product> products2 = await productManager.GetListObjectsAsync(1, 100);
        var list4 = products2.ToList();
        
        var product2 = list4[0];
        Assert.That(product2.Attributes.Count, Is.EqualTo(1));

        AttributeRow? attributeRow = (AttributeRow) product2.Attributes[0];
        Assert.That(attributeRow.Value.AllTranslations().Count, Is.EqualTo(1));
        Assert.That(attributeRow.Value.GetTranslation(englishMock), Is.EqualTo("Fire!"));
        Assert.That(attributeRow.AttributeName.AllTranslations().Count, Is.EqualTo(2));
        Assert.That(attributeRow.AttributeName.GetTranslation(englishMock), Is.EqualTo("Type"));
        Assert.That(attributeRow.AttributeName.GetTranslation(spanishMock), Is.EqualTo("typo"));

        await transaction.DisposeAsync();
        await db.DisposeAsync();

    }
    
}