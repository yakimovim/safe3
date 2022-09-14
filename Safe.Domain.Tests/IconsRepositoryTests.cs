using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Storage;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Domain.Tests;

public class IconsRepositoryTests : IDisposable
{
    private readonly LiteDbDatabaseProvider _databaseProvider;
    private readonly ItemsRepository _itemsRepository;
    private readonly IconsRepository _iconsRepository;

    public IconsRepositoryTests()
    {
        _databaseProvider = new LiteDbDatabaseProvider();
        var connectionProvider = new LiteDbConnectionProvider(_databaseProvider.Database);
        var liteDbItemsRepository = new LiteDbItemsRepository(connectionProvider);
        var liteDbIconsRepository = new LiteDbIconsRepository(connectionProvider);
        _itemsRepository = new ItemsRepository(liteDbItemsRepository);
        _iconsRepository = new IconsRepository(liteDbIconsRepository);
    }

    [Fact]
    public void CreateNewIcon()
    {
        var id = _iconsRepository.CreateNewIcon(CreateMemoryStream());

        id.Should().NotBeNullOrWhiteSpace();

        var icons = _iconsRepository.GetAllIcons().ToArray();

        icons.Should().HaveCount(1);
        icons[0].Id.Should().Be(id);
    }

    [Fact]
    public void GetIconById()
    {
        var inputStream = CreateMemoryStream();
        var inputData = inputStream.ToArray();

        var id = _iconsRepository.CreateNewIcon(inputStream);

        id.Should().NotBeNullOrWhiteSpace();

        var icon = _iconsRepository.GetIconById(id);

        icon.Should().NotBeNull();
        icon!.Id.Should().Be(id);

        var outputStream = icon.GetStream();

        outputStream.Should().NotBeNull();
        outputStream.Length.Should().Be(inputData.Length);

        var outputData = new byte[outputStream.Length];
        outputStream.Read(outputData).Should().Be(outputData.Length);

        for (int i = 0; i < inputData.Length; i++)
        {
            outputData[i].Should().Be(inputData[i]);
        }
    }

    [Fact]
    public void DeleteIcon()
    {
        var id = _iconsRepository.CreateNewIcon(CreateMemoryStream());

        id.Should().NotBeNullOrWhiteSpace();

        var item = new Item
        {
            IconId = id,
            Title = "A"
        };

        _itemsRepository.SaveItem(item);

        var icons = _iconsRepository.GetAllIcons().ToArray();

        icons.Should().HaveCount(1);
        icons[0].Id.Should().Be(id);

        _iconsRepository.DeleteIcon(id);

        icons = _iconsRepository.GetAllIcons().ToArray();

        icons.Should().BeEmpty();

        var restoredItem = _itemsRepository.GetChildItems(null).Single();

        restoredItem.IconId.Should().BeNull();
    }

    private MemoryStream CreateMemoryStream()
    {
        return new MemoryStream(
            Enumerable
                .Range(0, 100)
                .Select(_ => (byte)Random.Shared.Next(0, 100))
                .ToArray()
        );
    }

    public void Dispose()
    {
        _databaseProvider.Dispose();
    }
}