using System;
using System.Windows;
using EdlinSoftware.Safe.Services;
using Prism.Commands;

namespace EdlinSoftware.Safe.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly IStorageService _storageService;

    public MainWindowViewModel(
        IConfigurationService configurationService,
        IStorageService storageService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

        ExitCommand = new DelegateCommand(OnExit);
        CloseStorageCommand = new DelegateCommand(OnCloseStorage);
        SettingsCommand = new DelegateCommand(OnSettings);
        GeneratePasswordCommand = new DelegateCommand(OnGeneratePassword);
    }

    private void OnGeneratePassword()
    {
        DialogService.Show("PasswordGenerationDialog", null, res => { });
    }

    private void OnSettings()
    {
        RegionManager.RequestNavigationToMainContent("Settings");
    }

    private void OnCloseStorage()
    {
        _storageService.CloseStorage();

        var configuration = _configurationService.GetConfiguration();
        configuration.LastOpenedStorage = null;
        _configurationService.SaveConfiguration(configuration);

        RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
    }

    private void OnExit()
    {
        Application.Current.Shutdown(0);
    }

    public DelegateCommand ExitCommand { get; }

    public DelegateCommand SettingsCommand { get; }
    
    public DelegateCommand GeneratePasswordCommand { get; }
    
    public DelegateCommand CloseStorageCommand { get; }
}