using EdlinSoftware.Safe.Storage.Model;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Storage.Tests;

public sealed class LiteDbItemsRepositoryTests : IDisposable
{
    private readonly LiteDbDatabaseProvider _databaseProvider;
    private readonly LiteDbItemsRepository _itemsRepository;

    public LiteDbItemsRepositoryTests()
    {
        _databaseProvider = new LiteDbDatabaseProvider();

        var connectionProvider = new LiteDbConnectionProvider(_databaseProvider.Database);

        _itemsRepository = new LiteDbItemsRepository(connectionProvider);
    }

    [Fact]
    public void SaveNullItem()
    {
        Action act = () => _itemsRepository.SaveItems(new Item[] { null! });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SaveTopLevelItem()
    {
        var item = CreateItem();

        item.Fields.AddRange(CreateSeveralFields(3));

        _itemsRepository.SaveItems(item);

        item.Id.Should().NotBe(0);

        var restoredItem = _itemsRepository.GetItem(item.Id);

        restoredItem.Should().NotBeNull();
        restoredItem!.Title.Should().Be(item.Title);
        restoredItem.Description.Should().Be(item.Description);
        restoredItem.ParentId.Should().BeNull();
        restoredItem.Tags.Should().HaveCount(2);
        restoredItem.Fields.Should().NotBeNull();
        restoredItem.Fields.Should().HaveCount(3);
        restoredItem.Fields.Should().AllSatisfy(f =>
        {
            f.Should().NotBeNull();
            f.Should().BeOfType<TextField>()
                .Which.Text.Should().NotBeNullOrWhiteSpace();
            f.Name.Should().NotBeNullOrWhiteSpace();
        });
    }

    [Fact]
    public void SaveItemWithoutParent()
    {
        var item = CreateItem(6);

        Action act = () => _itemsRepository.SaveItems(item);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SaveItemWithParent()
    {
        var parentItem = CreateItem();

        _itemsRepository.SaveItems(parentItem);

        var item = CreateItem(parentItem.Id);

        _itemsRepository.SaveItems(item);

        item.Id.Should().NotBe(0);

        var restoredItem = _itemsRepository.GetItem(item.Id);

        restoredItem!.Title.Should().Be(item.Title);
        restoredItem.Description.Should().Be(item.Description);
        restoredItem.ParentId.Should().Be(parentItem.Id);
    }

    [Fact]
    public void GetChildItems()
    {
        var parentItem = CreateItem();

        _itemsRepository.SaveItems(parentItem);

        _itemsRepository.SaveItems(Enumerable
            .Range(1, 10)
            .Select(i => new Item { Title = $"A{i}", Description = $"B{i}", ParentId = parentItem.Id })
            .ToArray());

        var childItems = _itemsRepository.GetChildItems(parentItem.Id);

        childItems.Should().HaveCount(10);
        childItems.Should().AllSatisfy(i => i.Title.Should().StartWith("A"));
        childItems.Should().AllSatisfy(i => i.Description.Should().StartWith("B"));
        childItems.Should().AllSatisfy(i => i.ParentId.Should().Be(parentItem.Id));
    }

    [Fact]
    public void DeleteSingleItem()
    {
        var item = CreateItem();

        _itemsRepository.SaveItems(item);

        _itemsRepository.DeleteItems(item);

        CheckThatItemDoesNotExist(item.Id);
    }

    [Fact]
    public void DeleteItemWithNestedItems()
    {
        var item = CreateItem();
        item.Fields.AddRange(CreateSeveralFields(3));

        _itemsRepository.SaveItems(item);

        var nestedItems = Enumerable
            .Range(1, 3)
            .Select(_ =>
            {
                var nestedItem = CreateItem(item.Id);
                nestedItem.Fields.AddRange(CreateSeveralFields(3));
                return nestedItem;
            })
            .ToArray();

        _itemsRepository.SaveItems(nestedItems);

        _itemsRepository.DeleteItems(item);

        CheckThatItemDoesNotExist(item.Id);

        foreach (var nestedItem in nestedItems)
        {
            CheckThatItemDoesNotExist(nestedItem.Id);
        }
    }

    [Fact]
    public void UpdateItem()
    {
        var item = CreateItem();
        item.Fields.AddRange(CreateSeveralFields(4));

        _itemsRepository.SaveItems(item);

        item.Title = "AAA";
        item.Fields[2].Name = "BBB";

        _itemsRepository.SaveItems(item);

        var restoredItem = _itemsRepository.GetItem(item.Id);

        restoredItem!.Title.Should().Be("AAA");
        restoredItem.Fields[2].Name.Should().Be("BBB");
    }

    private void CheckThatItemDoesNotExist(int itemId)
    {
        _itemsRepository.GetItem(itemId).Should().BeNull();
    }

    public void Dispose()
    {
        _databaseProvider.Dispose();
    }
}