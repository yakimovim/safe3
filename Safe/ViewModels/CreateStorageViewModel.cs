using System;
using EdlinSoftware.Safe.Services;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class CreateStorageViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly IStorageService _storageService;
        private string? _storageFilePath;

        public CreateStorageViewModel(
            IConfigurationService configurationService,
            IStorageService storageService
            )
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

            CreateCommand = new DelegateCommand(OnCreate, CanCreate)
                .ObservesProperty(() => Title)
                .ObservesProperty(() => Password)
                .ObservesProperty(() => ConfirmPassword);
            CancelCommand = new DelegateCommand(OnCancel);
        }

        private bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(_storageFilePath)
                && !string.IsNullOrEmpty(_title)
                && !string.IsNullOrEmpty(_password)
                && string.Equals(_password, _confirmPassword);
        }

        private void OnCreate()
        {
            _storageService.CreateStorage(new StorageCreationOptions { 
                Title = Title,
                Description = Description ?? string.Empty,
                FileName = _storageFilePath!,
                Password = _password
            });

            if (_storageService.StorageIsOpened)
            {
                var configuration = _configurationService.GetConfiguration();
                configuration.LastOpenedStorage = _storageFilePath;
                _configurationService.SaveConfiguration(configuration);

                RegionManager.RequestNavigationToMainContent("StorageContent");
            }
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
            CreateCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand CreateCommand { get; }
        public DelegateCommand CancelCommand { get; }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _password = string.Empty;
        public string Password 
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword 
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }
    }
}
