using EdlinSoftware.Safe.Storage.Model;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Storage.Tests;

public class LiteDbItemsRepositoryTests : IDisposable
{
    private readonly LiteDbDatabaseProvider _databaseProvider;
    private readonly LiteDbItemsRepository _itemsRepository;
    private readonly LiteDbFieldsRepository _fieldsRepository;

    public LiteDbItemsRepositoryTests()
    {
        _databaseProvider = new LiteDbDatabaseProvider();

        var connectionProvider = new LiteDbConnectionProvider(_databaseProvider.Database);

        _itemsRepository = new LiteDbItemsRepository(connectionProvider);
        _fieldsRepository = new LiteDbFieldsRepository(connectionProvider);
    }

    [Fact]
    public void SaveNullItem()
    {
        Action act = () => _itemsRepository.SaveItems(new Item[] { null });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SaveTopLevelItem()
    {
        var item = CreateItem();

        _itemsRepository.SaveItems(item);

        item.Id.Should().NotBe(0);

        var restoredItem = _itemsRepository.GetItem(item.Id);

        restoredItem.Title.Should().Be(item.Title);
        restoredItem.Description.Should().Be(item.Description);
        restoredItem.ParentId.Should().BeNull();
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

        restoredItem.Title.Should().Be(item.Title);
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

        var fields = CreateSeveralFields(3, item.Id);

        _fieldsRepository.SaveFields(fields);

        _itemsRepository.DeleteItems(item);

        CheckThatItemDoesNotExist(item.Id);
    }

    [Fact]
    public void DeleteItemWithNestedItems()
    {
        var item = CreateItem();

        _itemsRepository.SaveItems(item);

        var fields = CreateSeveralFields(3, item.Id);

        _fieldsRepository.SaveFields(fields);

        var nestedItems = Enumerable
            .Range(1, 3)
            .Select(_ => CreateItem(item.Id))
            .ToArray();

        _itemsRepository.SaveItems(nestedItems);

        foreach (var nestedItem in nestedItems)
        {
            var nestedFields = CreateSeveralFields(3, nestedItem.Id);

            _fieldsRepository.SaveFields(nestedFields);
        }

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

        _itemsRepository.SaveItems(item);

        item.Title = "AAA";

        _itemsRepository.SaveItems(item);

        var restoredItem = _itemsRepository.GetItem(item.Id);

        restoredItem.Title.Should().Be("AAA");
    }

    private void CheckThatItemDoesNotExist(int itemId)
    {
        _itemsRepository.GetItem(itemId).Should().BeNull();

        _fieldsRepository.GetItemFields(itemId).Should().BeEmpty();
    }

    public void Dispose()
    {
        _databaseProvider.Dispose();
    }
}