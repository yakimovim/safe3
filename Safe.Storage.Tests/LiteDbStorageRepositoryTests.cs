using EdlinSoftware.Safe.Storage.Model;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Storage.Tests;

public class LiteDbStorageRepositoryTests
{
    private readonly LiteDbStorageInfoRepository _storageInfoRepository;

    public LiteDbStorageRepositoryTests()
    {
        var databaseProvider = new LiteDbDatabaseProvider();

        var connectionProvider = new LiteDbConnectionProvider(databaseProvider.Database);

        _storageInfoRepository = new LiteDbStorageInfoRepository(connectionProvider);
    }

    [Fact]
    public void SaveStorageInfo()
    {
        // Arrange

        var storageInfo = new StorageInfo
        {
            Title = "AAA",
            Description = "BBB"
        };

        // Act

        _storageInfoRepository.SaveStorageInfo(storageInfo);

        // Assert

        var restoredStorageInfo = _storageInfoRepository.GetStorageInfo();

        restoredStorageInfo.Should().NotBeNull();
        restoredStorageInfo.Title.Should().Be("AAA");
        restoredStorageInfo.Description.Should().Be("BBB");
    }

    [Fact]
    public void GetDefaultStorageInfo()
    {
        // Act

        var restoredStorageInfo = _storageInfoRepository.GetStorageInfo();

        // Assert

        restoredStorageInfo.Should().NotBeNull();
        restoredStorageInfo.Title.Should().Be("Safe Storage");
        restoredStorageInfo.Description.Should().BeNull();
    }
}