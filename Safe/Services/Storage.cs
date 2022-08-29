using System;
using EdlinSoftware.Safe.Events;
using LiteDB;
using Prism.Events;

namespace EdlinSoftware.Safe.Services
{
    internal interface IStorageService
    {
        void CreateStorage(StorageCreationOptions options);

        void OpenStorage(StorageOpeningOptions options);
    }

    public class StorageOpeningOptions
    {
        public string FileName { get; set; }

        public string Password { get; set; }
    }

    public class StorageCreationOptions : StorageOpeningOptions
    {
    }

    internal class StorageService : IStorageService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly LiteDbConnectionProvider _connectionProvider;

        public void CreateStorage(StorageCreationOptions options)
        {
            _connectionProvider.Database = new LiteDatabase($"Filename={options.FileName};Password={options.Password}");
            _eventAggregator.GetEvent<StorageChanged>().Publish();
        }

        public void OpenStorage(StorageOpeningOptions options)
        {
            _connectionProvider.Database = new LiteDatabase($"Filename={options.FileName};Password={options.Password}");
            _eventAggregator.GetEvent<StorageChanged>().Publish();
        }

        public StorageService(
            IEventAggregator eventAggregator,
            LiteDbConnectionProvider connectionProvider
            )
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }
    }
}
