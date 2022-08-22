using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Storage;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using EdlinSoftware.Safe.Tests.Infrastructure;
using EdlinSoftware.Safe.ViewModels;
using FluentAssertions;

namespace EdlinSoftware.Safe.Tests;

public class ItemViewModelTests
{
    private readonly ItemsRepository _itemsRepository;
    private readonly DummyItemViewModelOwner _owner;

    public ItemViewModelTests()
    {
        var databaseProvider = new LiteDbDatabaseProvider();
        var connectionProvider = new LiteDbConnectionProvider(databaseProvider.Database);
        var liteDbItemsRepository = new LiteDbItemsRepository(connectionProvider);
        _itemsRepository = new ItemsRepository(liteDbItemsRepository);
        _owner = new DummyItemViewModelOwner();
    }

    [Fact]
    public void CreateNewItem()
    {
        var vmItem = new ItemViewModel(_itemsRepository, _owner)
        {
            Title = "AAA",
            Description = "BBB"
        };

        var rootItems = _itemsRepository.GetChildItems(null);

        rootItems.Should().NotBeNull();
        rootItems.Should().HaveCount(1);

        rootItems.Single().Title.Should().Be(vmItem.Title);
        rootItems.Single().Description.Should().Be(vmItem.Description);
    }

    [Fact]
    public void LoadExistingItem()
    {
        var item = new Item
        {
            Title = "AAA",
            Description = "BBB",
            Tags = { "CCC", "DDD" }
        };

        _itemsRepository.SaveItem(item);

        var vmItem = new ItemViewModel(_itemsRepository, _owner, item);

        vmItem.Title.Should().Be("AAA");
        vmItem.Description.Should().Be("BBB");
        vmItem.Tags.Should().BeEquivalentTo("CCC", "DDD");
    }

    [Fact]
    public void WorkWithTags()
    {
        var vmItem = new ItemViewModel(_itemsRepository, _owner)
        {
            Tags = { "AAA", "BBB" }
        };

        var item = _itemsRepository.GetChildItems(null).Single();

        item.Tags.Should().BeEquivalentTo("AAA", "BBB");

        vmItem.Tags.Remove("BBB");

        item = _itemsRepository.GetChildItems(null).Single();
        item.Tags.Should().BeEquivalentTo("AAA");
        
        vmItem.Tags.Add("CCC");

        item = _itemsRepository.GetChildItems(null).Single();
        item.Tags.Should().BeEquivalentTo("AAA", "CCC");
    }

    [Fact]
    public void WorkNestedItems()
    {
        var vmItem1 = new ItemViewModel(_itemsRepository, _owner);

        var vmItem2 = new ItemViewModel(_itemsRepository, _owner);

        _itemsRepository.GetChildItems(null).Should().HaveCount(2);
        vmItem1.SubItems.Should().HaveCount(0);
        vmItem2.SubItems.Should().HaveCount(0);

        vmItem1.SubItems.Add(vmItem2);

        _itemsRepository.GetChildItems(null).Should().HaveCount(1);
        _itemsRepository.GetChildItems(vmItem1.Item).Should().HaveCount(1);

        vmItem1.SubItems.Remove(vmItem2);

        _itemsRepository.GetChildItems(null).Should().HaveCount(1);
        _itemsRepository.GetChildItems(vmItem1.Item).Should().HaveCount(1);
    }
}