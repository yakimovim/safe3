using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Search;
using EdlinSoftware.Safe.Storage;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Domain.Tests;

public class ItemsRepositorySearchTests : IDisposable
{
    private readonly LiteDbDatabaseProvider _databaseProvider;
    private readonly ItemsRepository _itemsRepository;

    public ItemsRepositorySearchTests()
    {
        _databaseProvider = new LiteDbDatabaseProvider();
        var connectionProvider = new LiteDbConnectionProvider(_databaseProvider.Database);
        var liteDbItemsRepository = new LiteDbItemsRepository(connectionProvider);
        _itemsRepository = new ItemsRepository(liteDbItemsRepository);

        #region Data

        var providersItem = new Item
        {
            Title = "Providers",
            Description = "Different providers of Internet services",
            Tags = { "Providers" }
        };
        var shopsItem = new Item
        {
            Title = "Shops",
            Description = "Different on-line shops",
            Tags = { "Shops" }
        };

        _itemsRepository.SaveItem(providersItem);
        _itemsRepository.SaveItem(shopsItem);

        var providerItem1 = new Item(providersItem)
        {
            Title = "Google",
            Description = "Account in Google",
            Tags = { "Providers", "Mail" },
            Fields =
            {
                new TextField { Name = "URL:", Text = "http://google.com" },
                new TextField { Name = "Login:", Text = "googleUser" },
                new PasswordField { Name = "Password:", Password = "abc" }
            }
        };
        var providerItem2 = new Item(providersItem)
        {
            Title = "Microsoft",
            Description = "Account in Microsoft",
            Tags = { "Providers", "Mail" },
            Fields =
            {
                new TextField { Name = "URL:", Text = "http://www.microsoft.com" },
                new TextField { Name = "Login:", Text = "microsoftUser" },
                new PasswordField { Name = "Password:", Password = "123" }
            }
        };

        _itemsRepository.SaveItem(providerItem1);
        _itemsRepository.SaveItem(providerItem2);
        
        var shopItem1 = new Item(shopsItem)
        {
            Title = "Wildberries",
            Description = "Account in Wildberries",
            Tags = { "Shop" },
            Fields =
            {
                new TextField { Name = "URL:", Text = "http://wildberries.ru" },
                new TextField { Name = "Phone:", Text = "89993334455" },
            }
        };
        var shopItem2 = new Item(shopsItem)
        {
            Title = "Ozon",
            Description = "Account in Ozon",
            Tags = { "Shop" },
            Fields =
            {
                new TextField { Name = "URL:", Text = "http://ozon.ru" },
                new TextField { Name = "Login:", Text = "ozonUser" },
                new PasswordField { Name = "Password:", Password = "qwe" }
            }
        };

        _itemsRepository.SaveItem(shopItem1);
        _itemsRepository.SaveItem(shopItem2);

        #endregion
    }

    [Theory]
    [InlineData("Google", 1)]
    [InlineData("Micros", 1)] // Partial
    public void SearchByTitle(string term, int count)
    {
        // Act

        var items = _itemsRepository.Find(new[]
        {
            new SearchStringElement(term, Fields.Title)
        });

        // Assert

        items.Should().NotBeNull();
        items.Should().HaveCount(count);

        items.Should().AllSatisfy(i =>
        {
            i.Title.Should().Contain(term);
        });
    }

    [Theory]
    [InlineData("Account", 4)]
    [InlineData("Google", 1)]
    public void SearchByDescription(string term, int count)
    {
        // Act

        var items = _itemsRepository.Find(new[]
        {
            new SearchStringElement(term, Fields.Description)
        });

        // Assert

        items.Should().NotBeNull();
        items.Should().HaveCount(count);

        items.Should().AllSatisfy(i =>
        {
            i.Description.Should().Contain(term);
        });
    }

    [Theory]
    [InlineData("Provide", 3)]
    [InlineData("Mail", 2)]
    public void SearchByTag(string term, int count)
    {
        // Act

        var items = _itemsRepository.Find(new[]
        {
            new SearchStringElement(term, Fields.Tag)
        });

        // Assert

        items.Should().NotBeNull();
        items.Should().HaveCount(count);

        items.Should().AllSatisfy(i =>
        {
            i.Tags.Any(t => t.Contains(term))
                .Should().BeTrue();
        });
    }

    [Theory]
    [InlineData("googleUser", 1)]
    [InlineData("93334", 1)]
    [InlineData(".com", 2)]
    [InlineData("http://", 4)]
    public void SearchByField(string term, int count)
    {
        // Act

        var items = _itemsRepository.Find(new[]
        {
            new SearchStringElement(term, Fields.Field)
        });

        // Assert

        items.Should().NotBeNull();
        items.Should().HaveCount(count);
    }

    [Theory]
    [InlineData("a", 4)]
    [InlineData("3", 2)]
    public void SearchEverywhere(string term, int count)
    {
        // Act

        var items = _itemsRepository.Find(new[]
        {
            new SearchStringElement(term)
        });

        // Assert

        items.Should().NotBeNull();
        items.Should().HaveCount(count);
    }

    public void Dispose()
    {
        _databaseProvider.Dispose();
    }
}