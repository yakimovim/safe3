using System;
using EdlinSoftware.Safe.Storage.Model;
using IStorageStorageInfoRepository = EdlinSoftware.Safe.Storage.IStorageInfoRepository;
using StorageStorageInfo = EdlinSoftware.Safe.Storage.Model.StorageInfo;

namespace EdlinSoftware.Safe.Domain
{
    public class StorageInfoRepository : IStorageInfoRepository
    {
        private readonly IStorageStorageInfoRepository _storageInfoRepository;

        public StorageInfoRepository(
            IStorageStorageInfoRepository storageInfoRepository
        )
        {
            _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
        }

        public StorageInfo GetStorageInfo()
        {
            var info = _storageInfoRepository.GetStorageInfo();

            return new StorageInfo
            {
                Title = info.Title,
                Description = info.Description,
                IconId = info.IconId
            };
        }

        public void SaveStorageInfo(StorageInfo storageInfo)
        {
            _storageInfoRepository.SaveStorageInfo(
                new StorageStorageInfo
                {
                    Title = storageInfo.Title,
                    Description = storageInfo.Description,
                    IconId = storageInfo.IconId
                }
            );
        }
    }
}