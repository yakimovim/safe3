using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Storage.Model;
using LiteDB;
using LiteDB.Engine;
using Prism.Events;

namespace EdlinSoftware.Safe.Services
{
    internal interface IStorageService
    {
        void CreateStorage(StorageCreationOptions options);

        void OpenStorage(StorageOpeningOptions options);

        void CloseStorage();

        bool StorageIsOpened { get; }

        bool ChangePassword(string oldPassword, string newPassword);

        bool Export(string password, string exportFileName);
    }

    public class StorageOpeningOptions
    {
        public string FileName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class StorageCreationOptions : StorageOpeningOptions
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    internal class StorageService : IStorageService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly LiteDbConnectionProvider _connectionProvider;
        private string? _lastOpenedFile;
        private byte[]? _lastOpenedStoragePasswordHash;
        private readonly IStorageInfoRepository _storageInfoRepository;
        private readonly ExportService _exportService;

        public StorageService(
            IEventAggregator eventAggregator,
            LiteDbConnectionProvider connectionProvider,
            IStorageInfoRepository storageInfoRepository,
            ExportService exportService
        )
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
            _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        }

        public void CreateStorage(StorageCreationOptions options)
        {
            CloseStorage();

            try
            {
                _connectionProvider.Database = new LiteDatabase($"Filename={options.FileName};Password={options.Password}");
                _lastOpenedFile = options.FileName;
                _lastOpenedStoragePasswordHash = GetStringHash(options.Password);

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

        private byte[] GetStringHash(string value) => SHA256.HashData(Encoding.UTF32.GetBytes(value));

        public void OpenStorage(StorageOpeningOptions options)
        {
            CloseStorage();

            try
            {
                _connectionProvider.Database = new LiteDatabase($"Filename={options.FileName};Password={options.Password}");
                _lastOpenedFile = options.FileName;
                _lastOpenedStoragePasswordHash = GetStringHash(options.Password);
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
                _lastOpenedFile = null;
                _lastOpenedStoragePasswordHash = null;

                _eventAggregator.GetEvent<StorageChanged>().Publish();
            }
        }

        public bool StorageIsOpened => _connectionProvider.Database != null;

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if (_connectionProvider.Database != null)
            {
                try
                {
                    var oldPasswordHash = GetStringHash(oldPassword);

                    if (!oldPasswordHash.SequenceEqual(_lastOpenedStoragePasswordHash!))
                        return false;

                    _connectionProvider.Database.Rebuild(new RebuildOptions
                    {
                        Password = newPassword
                    });

                    var fileName = _lastOpenedFile!;

                    CloseStorage();

                    OpenStorage(new StorageOpeningOptions
                    {
                        FileName = fileName,
                        Password = newPassword
                    });

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public bool Export(string password, string exportFileName)
        {
            if (!GetStringHash(password).SequenceEqual(_lastOpenedStoragePasswordHash!))
                return false;

            return _exportService.Export(exportFileName);
        }
    }
}
