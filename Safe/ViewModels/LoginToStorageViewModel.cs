using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Services;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

internal partial class LoginToStorageViewModel : ObservableViewModelBase
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
    }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(_storageFilePath)
               && !string.IsNullOrEmpty(_password);
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        _storageService.OpenStorage(new StorageCreationOptions
        {
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
        else
        {
            PasswordIsValid = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (!navigationContext.Parameters.TryGetValue("StoragePath", out string storageFilePath))
        {
            RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
            return;
        }

        Password = string.Empty;
        PasswordIsValid = true;

        _storageFilePath = storageFilePath;

        LoginCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _passwordIsValid = true;
}
