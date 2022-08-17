using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Storage;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Domain.Tests;

public class ItemsRepositoryTests : IDisposable
{
    private readonly LiteDbDatabaseProvider _databaseProvider;
    private readonly ItemsRepository _itemsRepository;

    public ItemsRepositoryTests()
    {
        _databaseProvider = new LiteDbDatabaseProvider();
        var connectionProvider = new LiteDbConnectionProvider(_databaseProvider.Database);
        var liteDbItemsRepository = new LiteDbItemsRepository(connectionProvider);
        _itemsRepository = new ItemsRepository(liteDbItemsRepository);
    }

    [Fact]
    public void SaveTopLevelItem()
    {
        var item = CreateItem();
        item.Fields.AddRange(CreateSeveralFields(3));

        _itemsRepository.SaveItem(item);

        var rootItems = _itemsRepository.GetChildItems(null);

        rootItems.Should().HaveCount(1);

        var restoredItem = rootItems.Single();

        restoredItem.Id.Should().NotBe(0);
        restoredItem.ParentId.Should().BeNull();
        restoredItem.Title.Should().Be(item.Title);
        restoredItem.Description.Should().Be(item.Description);
        restoredItem.Tags.Should().BeEquivalentTo(item.Tags);
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
    public void SaveChanges()
    {
        var item = CreateItem();

        item.Fields.Add(CreateTextField());

        _itemsRepository.SaveItem(item);

        item.Description = "DDD";
        item.Fields[0].Name = "NNN";

        _itemsRepository.SaveItem(item);
        _itemsRepository.SaveItem(item);
        _itemsRepository.SaveItem(item);

        var rootItems = _itemsRepository.GetChildItems(null);

        rootItems.Should().HaveCount(1);

        var restoredItem = rootItems.Single();

        restoredItem.Id.Should().NotBe(0);
        restoredItem.ParentId.Should().BeNull();
        restoredItem.Title.Should().Be(item.Title);
        restoredItem.Description.Should().Be("DDD");
        restoredItem.Tags.Should().BeEquivalentTo(item.Tags);
        restoredItem.Fields.Should().HaveCount(1);
        restoredItem.Fields[0].Name.Should().Be("NNN");
    }

    [Fact]
    public void SaveNestedItem()
    {
        var rootItem = CreateItem();

        _itemsRepository.SaveItem(rootItem);

        var item = CreateItem(rootItem);
        item.Description = "DDD";
        item.Fields.AddRange(CreateSeveralFields(2));

        _itemsRepository.SaveItem(item);

        var restoredItems = _itemsRepository.GetChildItems(rootItem);

        restoredItems.Should().HaveCount(1);

        var restoredItem = restoredItems.Single();
        restoredItem.Description.Should().Be("DDD");
    }

    [Fact]
    public void DeleteItem()
    {
        var rootItem = CreateItem();
        rootItem.Fields.AddRange(CreateSeveralFields(5));

        _itemsRepository.SaveItem(rootItem);

        var item = CreateItem(rootItem);
        item.Fields.AddRange(CreateSeveralFields(2));

        _itemsRepository.SaveItem(item);

        _itemsRepository.DeleteItem(rootItem);

        _itemsRepository.GetChildItems(null).Should().BeEmpty();
        _itemsRepository.GetChildItems(rootItem).Should().BeEmpty();
    }

    [Fact]
    public void ChangeFields()
    {
        var item = CreateItem();
        item.Fields.Add(new TextField { Name = "A", Text = "A" });
        item.Fields.Add(new TextField { Name = "B", Text = "B" });

        _itemsRepository.SaveItem(item);

        item.Fields.RemoveAt(1);
        item.Fields.Add(new TextField { Name = "C", Text = "C" });

        _itemsRepository.SaveItem(item);

        var restoredItem = _itemsRepository.GetChildItems(null).Single();

        restoredItem.Fields.Should().HaveCount(2);

        restoredItem.Fields[0].Name.Should().Be("A");
        restoredItem.Fields[1].Name.Should().Be("C");
    }

    [Fact]
    public void PreserveFieldsOrder()
    {
        var item = CreateItem();
        item.Fields.Add(new TextField { Name = "A" });
        item.Fields.Add(new TextField { Name = "B" });

        _itemsRepository.SaveItem(item);

        var fields = item.Fields.ToArray();
        item.Fields.Clear();
        item.Fields.AddRange(fields.Reverse());

        _itemsRepository.SaveItem(item);

        var restoredItem = _itemsRepository.GetChildItems(null).Single();

        restoredItem.Fields.Should().HaveCount(2);
        restoredItem.Fields[0].Name.Should().Be("B");
        restoredItem.Fields[1].Name.Should().Be("A");
    }

    [Fact]
    public void MoveItem()
    {
        var rootItem1 = CreateItem();
        var rootItem2 = CreateItem();

        _itemsRepository.SaveItem(rootItem1);
        _itemsRepository.SaveItem(rootItem2);

        var item = CreateItem(rootItem1);

        _itemsRepository.SaveItem(item);

        _itemsRepository.GetChildItems(rootItem1).Should().HaveCount(1);
        _itemsRepository.GetChildItems(rootItem2).Should().HaveCount(0);

        item.MoveTo(rootItem2);

        _itemsRepository.SaveItem(item);

        _itemsRepository.GetChildItems(rootItem1).Should().HaveCount(0);
        _itemsRepository.GetChildItems(rootItem2).Should().HaveCount(1);
    }

    public void Dispose()
    {
        _databaseProvider.Dispose();
    }
}