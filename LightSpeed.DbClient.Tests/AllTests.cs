using ExampleModels;
using ExampleModels.Currencies;
using LightSpeed.DbClient.Database;
using LightSpeed.DbClient.ExampleModels;
using LightSpeed.DbClient.Implementations;
using LightSpeed.DbClient.Models;
using LightSpeed.DbClient.Postgresql.Database;

namespace LightSpeed.DbClient.Tests;

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
            //currency.Id = Guid.NewGuid();
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
        
        IDataSelection<Currency> cur1 = await manager.GetListAsync(1, 1000);
        Assert.NotNull(cur1);
        Assert.That(cur1.Count(), Is.EqualTo(1000));
        Assert.That(cur1.Page, Is.EqualTo(1));
        Assert.That(cur1.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur1.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur2 = await manager.GetListAsync(2, 1000);
        Assert.NotNull(cur2);
        Assert.That(cur2.Count(), Is.EqualTo(1000));
        Assert.That(cur2.Page, Is.EqualTo(2));
        Assert.That(cur2.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur2.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur3 = await manager.GetListAsync(3, 1000);
        Assert.NotNull(cur3);
        Assert.That(cur3.Count(), Is.EqualTo(1000));
        Assert.That(cur3.Page, Is.EqualTo(3));
        Assert.That(cur3.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur3.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur4 = await manager.GetListAsync(4, 1000);
        Assert.NotNull(cur4);
        Assert.That(cur4.Count(), Is.EqualTo(1000));
        Assert.That(cur4.Page, Is.EqualTo(4));
        Assert.That(cur4.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur4.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur5 = await manager.GetListAsync(5, 1000);
        Assert.NotNull(cur5);
        Assert.That(cur5.Count(), Is.EqualTo(1000));
        Assert.That(cur5.Page, Is.EqualTo(5));
        Assert.That(cur5.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur5.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur6 = await manager.GetListObjectsAsync(6, 1000);
        Assert.NotNull(cur6);
        Assert.That(cur6.Count(), Is.EqualTo(1000));
        Assert.That(cur6.Page, Is.EqualTo(6));
        Assert.That(cur6.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur6.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur7 = await manager.GetListObjectsAsync(7, 1000);
        Assert.NotNull(cur7);
        Assert.That(cur7.Count(), Is.EqualTo(1000));
        Assert.That(cur7.Page, Is.EqualTo(7));
        Assert.That(cur7.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur7.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur8 = await manager.GetListObjectsAsync(8, 1000);
        Assert.NotNull(cur8);
        Assert.That(cur8.Count(), Is.EqualTo(1000));
        Assert.That(cur8.Page, Is.EqualTo(8));
        Assert.That(cur8.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur8.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur9 = await manager.GetListObjectsAsync(9, 1000);
        Assert.NotNull(cur9);
        Assert.That(cur9.Count(), Is.EqualTo(1000));
        Assert.That(cur9.Page, Is.EqualTo(9));
        Assert.That(cur9.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur9.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur10 = await manager.GetListObjectsAsync(10, 1000);
        Assert.NotNull(cur10);
        Assert.That(cur10.Count(), Is.EqualTo(1000));
        Assert.That(cur10.Page, Is.EqualTo(10));
        Assert.That(cur10.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur10.TotalRows, Is.EqualTo(10000));
        
        IDataSelection<Currency> cur11 = await manager.GetListObjectsAsync(11, 1000);
        Assert.NotNull(cur11);
        Assert.That(cur11.Count(), Is.EqualTo(0));
        Assert.That(cur11.Page, Is.EqualTo(11));
        Assert.That(cur11.RowsPerBatch, Is.EqualTo(1000));
        Assert.That(cur11.TotalRows, Is.EqualTo(10000));
        
        await transaction.DisposeAsync();
        await connection.DisposeAsync();
        await db.DisposeAsync();

    }

    [Test]
    public async Task TestGeneral()
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
    public async Task TestConnectedTableFilter()
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
        product.Name.AddTranslation(spanishMock, "Producto 1 Versace");
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
        
        Product product20 = productManager.CreateObject();
        product20.Id = Guid.NewGuid();
        product20.Name = new Translatable();
        product20.Name.AddTranslation(englishMock, "Product 2");
        product20.Name.AddTranslation(spanishMock, "Producto 2 Gucci");
        product20.ProductType = ProductType.Value.Product;
        await productManager.SaveAsync(product20);
        
        await transaction.CommitAsync();
        
        IEnumerable<Product> products = await productManager.GetListAsync(1, 100);
        var list3 = products.ToList();
        
        //var product1 = list3[0];
        //Assert.That(product1.Translations.Count, Is.EqualTo(3));
        //Assert.That(product1.Name.AllTranslations().Count, Is.EqualTo(2));
        
        IEnumerable<Product> products2 = await productManager.GetListObjectsAsync(1, 1000);
        var list4 = products2.ToList();
        
        //var product2 = list4[0];
        //Assert.That(product2.Attributes.Count, Is.EqualTo(1));

        //AttributeRow? attributeRow = (AttributeRow) product2.Attributes[0];
        //Assert.That(attributeRow.Value.AllTranslations().Count, Is.EqualTo(1));
        //Assert.That(attributeRow.Value.GetTranslation(englishMock), Is.EqualTo("Fire!"));
        //Assert.That(attributeRow.AttributeName.AllTranslations().Count, Is.EqualTo(2));
        //Assert.That(attributeRow.AttributeName.GetTranslation(englishMock), Is.EqualTo("Type"));
        //Assert.That(attributeRow.AttributeName.GetTranslation(spanishMock), Is.EqualTo("typo"));
        
        IFilters<Product> filters = productManager.CreateFilters();
        filters.Add(new Filter<Product>("name", ComparisonOperator.Like, "%versace%"));
        IEnumerable<Product> products3 = await productManager.GetListObjectsAsync(filters, 1, 100);
        var list30 = products3.ToList();
        var product30 = list30[0];
        Assert.That(list30.Count, Is.EqualTo(1));
        Assert.That(product30.Name.GetTranslation(spanishMock), Is.EqualTo("Producto 1 Versace"));
        
        long n = await productManager.CountAsync(filters);
        Assert.That(n, Is.EqualTo(1));
        n = await productManager.CountAsync();
        Assert.That(n, Is.EqualTo(2));
        
        await transaction.DisposeAsync();
        await db.DisposeAsync();

    }
    
    [Test]
    public async Task TestSelfReference()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();

        IManager<SelfReference> manager = new PostgresqlManager<SelfReference>(connection, transaction);
        await manager.GetListAsync();
        
        SelfReference ob = manager.CreateObject();
        ob.Name = "name";
        ob.Id = Guid.NewGuid();
        var ob2 = await manager.SaveAsync(ob);
        
        await transaction.DisposeAsync();
        await db.DisposeAsync();

    }
    
    [Test]
    public async Task TestFilterIn()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();

        IManager<SelfReference> manager = new PostgresqlManager<SelfReference>(connection, transaction);
        IFilters<SelfReference> filters = manager.CreateFilters();
        var filterValues = new List<string>();
        filterValues.Add("%versace%");
        filterValues.Add("%gucci%");
        
        filters.Add(new Filter<SelfReference>("name", ComparisonOperator.In, filterValues));
        await manager.GetListAsync(filters);
        
        await transaction.DisposeAsync();
        await db.DisposeAsync();

    }
    
    [Test]
    public async Task TestRecords()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();

        IManager<Price> manager = new PostgresqlManager<Price>(connection, transaction);
        
        Price price = manager.CreateObject();
        price.Product = Guid.NewGuid();
        price.Variant = Guid.NewGuid();
        price.ListPrice = 100;
        price.SalePrice = 80;
        
        IFilters<Price> filters = manager.CreateFilters();
        filters.Add(new Filter<Price>("product", ComparisonOperator.Equals, price.Product));
        filters.Add(new Filter<Price>("variant", ComparisonOperator.Equals, price.Variant));
        await manager.DeleteAsync();
        await manager.SaveRecordsAsync(filters, price);
        
        var prices1 = await manager.GetListAsync();
        Assert.That(prices1.Count, Is.EqualTo(1));
        
        var prices2 = await manager.GetListObjectsAsync();
        Assert.That(prices2.Count, Is.EqualTo(1));
        
        long l = await manager.CountAsync();
        Assert.That(l, Is.EqualTo(1));

        await transaction.CommitAsync();
        
        await transaction.DisposeAsync();
        await db.DisposeAsync();

    }
    
    [Test]
    public async Task TestConnectedTableSave()
    {
        IDatabase db = new PostgresqlDatabase("localhost",5432,"backend", "backend", "mysecretpassword");
        IConnection connection = await db.OpenConnectionAsync();
        ITransaction transaction = await connection.BeginTransactionAsync();
        
        IManager<ProductAttribute> attributeManager = new PostgresqlManager<ProductAttribute>(connection, transaction);
        ProductAttribute attribute = attributeManager.CreateObject();
        attribute.Id = Guid.NewGuid();
        attribute.Name = new Translatable();
        
        await attributeManager.SaveAsync(attribute);
        
        IManager<Product> productManager = new PostgresqlManager<Product>(connection, transaction);
        Product product = productManager.CreateObject();
        product.Id = Guid.NewGuid();
        product.Name = new Translatable();
        product.ProductType = ProductType.Value.Product;

        ITranslatable value = new Translatable();
        AttributeRow row = new AttributeRow
        {
            Id = Guid.NewGuid(),
            Attribute = attribute.Id,
            Value = value,
        };
        product.Attributes = new DatabaseObjectTable<AttributeRow>();
        product.Attributes.Add(row);
        
        var result = await productManager.SaveAsync(product);
        
        Assert.NotNull(result.Attributes);
        Assert.That(result.Attributes.Count, Is.EqualTo(1));
        
    }
    
}