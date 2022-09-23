using EdlinSoftware.Safe.Storage.Model;

namespace EdlinSoftware.Safe.Storage
{
    public interface IStorageInfoRepository
    {
        StorageInfo GetStorageInfo();

        void SaveStorageInfo(StorageInfo storageInfo);
    }
}