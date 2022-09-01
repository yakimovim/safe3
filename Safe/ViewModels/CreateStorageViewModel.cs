using System;
using EdlinSoftware.Safe.Services;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class CreateStorageViewModel : ViewModelBase
    {
        private readonly IStorageService _storageService;
        private string? _storageFilePath;

        public CreateStorageViewModel(
            IStorageService storageService
            )
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

            CreateCommand = new DelegateCommand(OnCreate, CanCreate)
                .ObservesProperty(() => Password);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(_storageFilePath)
                && !string.IsNullOrEmpty(_password);
        }

        private void OnCreate()
        {
            _storageService.CreateStorage(new StorageCreationOptions { 
                FileName = _storageFilePath!,
                Password = _password
            });
            RegionManager.RequestNavigate("MainContentRegion", "StorageContent");
        }

        private void OnCancel()
        {
            RegionManager.RequestNavigate("MainContentRegion", "CreateOrOpenStorage");
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if(!navigationContext.Parameters.TryGetValue("StoragePath", out string storageFilePath))
            {
                RegionManager.RequestNavigate("MainContentRegion", "CreateOrOpenStorage");
                return;
            }

            _storageFilePath = storageFilePath;
            CreateCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand CreateCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private string _password;

        public string Password 
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }
    }
}
