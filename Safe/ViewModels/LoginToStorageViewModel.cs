using System;
using EdlinSoftware.Safe.Services;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class LoginToStorageViewModel : ViewModelBase
    {
        private readonly IStorageService _storageService;
        private readonly IConfigurationService _configurationService;
        private string? _storageFilePath;

        public LoginToStorageViewModel(
            IStorageService storageService,
            IConfigurationService configurationService
            )
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            LoginCommand = new DelegateCommand(OnLogin, CanLogin)
                .ObservesProperty(() => Password);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(_storageFilePath)
                && !string.IsNullOrEmpty(_password);
        }

        private void OnLogin()
        {
            _storageService.OpenStorage(new StorageCreationOptions { 
                FileName = _storageFilePath!,
                Password = _password
            });
            RegionManager.RequestNavigationToMainContent("StorageContent");
        }

        private void OnCancel()
        {
            RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if(!navigationContext.Parameters.TryGetValue("StoragePath", out string storageFilePath))
            {
                RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
                return;
            }

            _storageFilePath = storageFilePath;
            LoginCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand LoginCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private string _password;

        public string Password 
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
    }
}
