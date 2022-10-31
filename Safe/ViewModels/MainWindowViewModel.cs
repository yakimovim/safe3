﻿using System;
using System.Windows;
using EdlinSoftware.Safe.Events;
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
        CloseStorageCommand = new DelegateCommand(OnCloseStorage, CanCloseStorage);
        ChangePasswordCommand = new DelegateCommand(OnChangePassword, CanChangePassword);
        ExportCommand = new DelegateCommand(OnExport, CanExport);
        SettingsCommand = new DelegateCommand(OnSettings);
        GeneratePasswordCommand = new DelegateCommand(OnGeneratePassword);
    }

    private void OnChangePassword()
    {
        RegionManager.RequestNavigationToMainContent("ChangePassword");
    }

    private bool CanChangePassword() => _storageService.StorageIsOpened;

    private void OnExport()
    {
        RegionManager.RequestNavigationToMainContent("ExportStorage");
    }

    private bool CanExport() => _storageService.StorageIsOpened;

    private void OnGeneratePassword()
    {
        DialogService.Show("PasswordGenerationDialog", null, res => { });
    }

    private void OnSettings()
    {
        RegionManager.RequestNavigationToMainContent("Settings");
    }

    private bool CanCloseStorage() => _storageService.StorageIsOpened;

    private void OnCloseStorage()
    {
        _storageService.CloseStorage();

        var configuration = _configurationService.GetConfiguration();
        configuration.LastOpenedStorage = null;
        _configurationService.SaveConfiguration(configuration);

        RegionManager.RequestNavigationToDetails("Blank");
        RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
    }

    private void OnExit()
    {
        Application.Current.Shutdown(0);
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<StorageChanged>()
            .Subscribe(() =>
            {
                CloseStorageCommand.RaiseCanExecuteChanged();
                ChangePasswordCommand.RaiseCanExecuteChanged();
                ExportCommand.RaiseCanExecuteChanged();
            });
    }

    public DelegateCommand ExitCommand { get; }

    public DelegateCommand SettingsCommand { get; }
    
    public DelegateCommand GeneratePasswordCommand { get; }
    
    public DelegateCommand CloseStorageCommand { get; }
    
    public DelegateCommand ChangePasswordCommand { get; }
    
    public DelegateCommand ExportCommand { get; }
}