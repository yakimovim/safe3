using System;
using System.Linq;
using EdlinSoftware.Safe.Storage.Model;
using LiteDB;

namespace EdlinSoftware.Safe.Storage
{
    public class LiteDbStorageInfoRepository : IStorageInfoRepository
    {
        private readonly ILiteDbConnectionProvider _dbProvider;

        public LiteDbStorageInfoRepository(ILiteDbConnectionProvider dbProvider)
        {
            _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        }

        private ILiteCollection<StorageInfo> Collection => _dbProvider.GetDatabase().GetCollection<StorageInfo>();

        public StorageInfo GetStorageInfo()
        {
            return Collection.FindAll().SingleOrDefault()
                ?? new StorageInfo
                {
                    Title = "Safe Storage"
                };
        }

        public void SaveStorageInfo(StorageInfo storageInfo)
        {
            Collection.DeleteAll();

            Collection.Insert(storageInfo);
        }
    }
}