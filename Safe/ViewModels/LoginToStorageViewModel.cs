using System;
using EdlinSoftware.Safe.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class LoginToStorageViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private readonly IStorageService _storageService;
        private string? _storageFilePath;

        public LoginToStorageViewModel(
            IRegionManager regionManager,
            IStorageService storageService
            )
        {
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

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
            _regionManager.RequestNavigate("MainContentRegion", "StorageContent");
        }

        private void OnCancel()
        {
            _regionManager.RequestNavigate("MainContentRegion", "CreateOrOpenStorage");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if(!navigationContext.Parameters.TryGetValue("StoragePath", out string storageFilePath))
            {
                _regionManager.RequestNavigate("MainContentRegion", "CreateOrOpenStorage");
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
            set
            {
                SetProperty(ref _password, value);
            }
        }
    }
}
