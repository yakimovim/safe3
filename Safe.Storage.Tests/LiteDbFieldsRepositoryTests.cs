using EdlinSoftware.Safe.Storage.Model;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Storage.Tests;

public class LiteDbFieldsRepositoryTests : IDisposable
{
    private readonly LiteDbDatabaseProvider _databaseProvider;
    private readonly LiteDbItemsRepository _itemsRepository;
    private readonly LiteDbFieldsRepository _fieldsRepository;

    public LiteDbFieldsRepositoryTests()
    {
        _databaseProvider = new LiteDbDatabaseProvider();

        var connectionProvider = new LiteDbConnectionProvider(_databaseProvider.Database);

        _itemsRepository = new LiteDbItemsRepository(connectionProvider);
        _fieldsRepository = new LiteDbFieldsRepository(connectionProvider);
    }

    [Fact]
    public void SaveNullItem()
    {
        Action act = () => _fieldsRepository.SaveFields(new Field[] { null });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SaveItemFields()
    {
        var item = CreateItem();

        _itemsRepository.SaveItems(item);

        var fields = CreateSeveralFields(4, item.Id);

        _fieldsRepository.SaveFields(fields);

        var restoredFields = _fieldsRepository.GetItemFields(item.Id);

        restoredFields.Should().HaveCount(4);
    }

    [Fact]
    public void SaveFieldWithoutParentItem()
    {
        var field = CreateTextField(5);

        Action act = () => _fieldsRepository.SaveFields(field);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetField()
    {
        var item = CreateItem();

        _itemsRepository.SaveItems(item);

        var field = CreateTextField(item.Id);

        _fieldsRepository.SaveFields(field);

        var restoredField = _fieldsRepository.GetField(field.Id);

        restoredField.Should().NotBeNull();
        restoredField.Id.Should().Be(field.Id);
        restoredField.ItemId.Should().Be(item.Id);
        restoredField.Name.Should().Be(field.Name);
        restoredField.Should().BeOfType<TextField>()
            .Which.Text.Should().Be(field.Text);
    }

    [Fact]
    public void DeleteFields()
    {
        var item = CreateItem();

        _itemsRepository.SaveItems(item);

        var fields = CreateSeveralFields(3, item.Id);

        _fieldsRepository.SaveFields(fields);

        _fieldsRepository.DeleteFields(fields[1]);

        _fieldsRepository.GetItemFields(item.Id)
            .Should().HaveCount(2);
    }

    [Fact]
    public void UpdateField()
    {
        var item = CreateItem();

        _itemsRepository.SaveItems(item);

        var field = CreateTextField(item.Id);

        _fieldsRepository.SaveFields();

        field.Text = "BBB";

        _fieldsRepository.SaveFields(field);

        var restoredField = _fieldsRepository.GetField(field.Id);

        restoredField.Should().BeOfType<TextField>()
            .Which.Text.Should().Be("BBB");
    }
    
    public void Dispose()
    {
        _databaseProvider.Dispose();
    }
}