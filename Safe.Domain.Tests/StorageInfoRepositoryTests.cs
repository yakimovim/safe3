using EdlinSoftware.Safe.Storage;
using EdlinSoftware.Safe.Storage.Model;
using EdlinSoftware.Safe.Storage.Tests.Infrastructure;
using FluentAssertions;

namespace EdlinSoftware.Safe.Domain.Tests
{
    public class StorageInfoRepositoryTests
    {
        private readonly StorageInfoRepository _storageInfoRepository;

        public StorageInfoRepositoryTests()
        {
            var databaseProvider = new LiteDbDatabaseProvider();
            var connectionProvider = new LiteDbConnectionProvider(databaseProvider.Database);
            var liteDbStorageInfoRepository = new LiteDbStorageInfoRepository(connectionProvider);
            _storageInfoRepository = new StorageInfoRepository(liteDbStorageInfoRepository);
        }


        [Fact]
        public void SaveStorageInfo()
        {
            // Arrange

            var storageInfo = new StorageInfo
            {
                Title = "AAA",
                Description = "BBB",
                IconId = "CCC"
            };

            // Act

            _storageInfoRepository.SaveStorageInfo(storageInfo);

            // Assert

            var restoredStorageInfo = _storageInfoRepository.GetStorageInfo();

            restoredStorageInfo.Should().NotBeNull();
            restoredStorageInfo.Title.Should().Be("AAA");
            restoredStorageInfo.Description.Should().Be("BBB");
            restoredStorageInfo.IconId.Should().Be("CCC");
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
            restoredStorageInfo.IconId.Should().BeNull();
        }
    }
}