using EdlinSoftware.Safe.Storage.Model;

namespace EdlinSoftware.Safe.Domain
{
    public interface IStorageInfoRepository
    {
        StorageInfo GetStorageInfo();

        void SaveStorageInfo(StorageInfo storageInfo);
    }
}