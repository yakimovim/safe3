using System;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Services;
using Microsoft.Win32;

namespace EdlinSoftware.Safe.ViewModels;

internal partial class MainWindowViewModel : ObservableViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly IStorageService _storageService;

    public MainWindowViewModel(
        IConfigurationService configurationService,
        IStorageService storageService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    [RelayCommand(CanExecute = nameof(StorageIsOpened))]
    private void ChangePassword()
    {
        RegionManager.RequestNavigationToMainContent("ChangePassword");
    }

    [RelayCommand(CanExecute = nameof(StorageIsOpened))]
    private void Export()
    {
        RegionManager.RequestNavigationToMainContent("ExportStorage");
    }

    [RelayCommand(CanExecute = nameof(StorageIsOpened))]
    private void Import()
    {
        var openDialog = new OpenFileDialog
        {
            CheckFileExists = true,
            CheckPathExists = true,
            Filter = $"{Application.Current.Resources["JsonFileFilter"]}|*.json",
            AddExtension = true,
            DefaultExt = ".json",
        };

        if (openDialog.ShowDialog() == true)
        {
            _storageService.Import(openDialog.FileName);

            EventAggregator.GetEvent<StorageChanged>().Publish();
        }
    }

    private bool StorageIsOpened() => _storageService.StorageIsOpened;

    [RelayCommand]
    private void GeneratePassword()
    {
        DialogService.Show("PasswordGenerationDialog", null, _ => { });
    }

    [RelayCommand]
    private void Settings()
    {
        RegionManager.RequestNavigationToMainContent("Settings");
    }

    [RelayCommand(CanExecute = nameof(StorageIsOpened))]
    private void CloseStorage()
    {
        _storageService.CloseStorage();

        var configuration = _configurationService.GetConfiguration();
        configuration.LastOpenedStorage = null;
        _configurationService.SaveConfiguration(configuration);

        RegionManager.RequestNavigationToDetails("Blank");
        RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown(0);
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<StorageChanged>()
            .Subscribe(() =>
            {
                CloseStorageCommand.NotifyCanExecuteChanged();
                ChangePasswordCommand.NotifyCanExecuteChanged();
                ExportCommand.NotifyCanExecuteChanged();
                ImportCommand.NotifyCanExecuteChanged();
            });
    }
}