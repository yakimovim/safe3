using System;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Storage;
using Prism.Events;

namespace EdlinSoftware.Safe.Services
{
    internal interface IStorageService
    {
        ILiteDbConnectionProvider? CurrentStorage { get; }

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
        private ILiteDbConnectionProvider? _currentStorage;

        public ILiteDbConnectionProvider? CurrentStorage => _currentStorage;

        public void CreateStorage(StorageCreationOptions options)
        {
            _currentStorage = new LiteDbConnectionProvider(options.FileName, options.Password);
            _eventAggregator.GetEvent<StorageChanged>().Publish(_currentStorage);
        }

        public void OpenStorage(StorageOpeningOptions options)
        {
            _currentStorage = new LiteDbConnectionProvider(options.FileName, options.Password);
            _eventAggregator.GetEvent<StorageChanged>().Publish(_currentStorage);
        }

        public StorageService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }
    }
}
