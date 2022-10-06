using System;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Storage.Model;
using LiteDB;
using Prism.Events;

namespace EdlinSoftware.Safe.Services
{
    internal interface IStorageService
    {
        void CreateStorage(StorageCreationOptions options);

        void OpenStorage(StorageOpeningOptions options);

        void CloseStorage();

        bool StorageIsOpened { get; }
    }

    public class StorageOpeningOptions
    {
        public string FileName { get; set; }

        public string Password { get; set; }
    }

    public class StorageCreationOptions : StorageOpeningOptions
    {
        public string Title { get; set; }
        public string? Description { get; set; }
    }

    internal class StorageService : IStorageService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly LiteDbConnectionProvider _connectionProvider;
        private readonly IStorageInfoRepository _storageInfoRepository;

        public StorageService(
            IEventAggregator eventAggregator,
            LiteDbConnectionProvider connectionProvider,
            IStorageInfoRepository storageInfoRepository
        )
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
        }

        public void CreateStorage(StorageCreationOptions options)
        {
            CloseStorage();

            try
            {
                _connectionProvider.Database = new LiteDatabase($"Filename={options.FileName};Password={options.Password}");

                _storageInfoRepository.SaveStorageInfo(new StorageInfo
                {
                    Title = options.Title,
                    Description = options.Description
                });
            }
            catch (LiteException)
            {
                _connectionProvider.Database = null;
            }

            _eventAggregator.GetEvent<StorageChanged>().Publish();
        }

        public void OpenStorage(StorageOpeningOptions options)
        {
            CloseStorage();

            try
            {
                _connectionProvider.Database = new LiteDatabase($"Filename={options.FileName};Password={options.Password}");
            }
            catch (LiteException)
            {
                _connectionProvider.Database = null;
            }

            _eventAggregator.GetEvent<StorageChanged>().Publish();
        }

        public void CloseStorage()
        {
            if (_connectionProvider.Database != null)
            {
                _connectionProvider.Database.Dispose();

                _connectionProvider.Database = null;

                _eventAggregator.GetEvent<StorageChanged>().Publish();
            }
        }

        public bool StorageIsOpened => _connectionProvider.Database != null;

    }
}
