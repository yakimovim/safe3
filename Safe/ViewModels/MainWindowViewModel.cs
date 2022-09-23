using System;
using System.Windows;
using EdlinSoftware.Safe.Services;
using Prism.Commands;

namespace EdlinSoftware.Safe.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;

    public MainWindowViewModel(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

        ExitCommand = new DelegateCommand(OnExit);
        CloseStorageCommand = new DelegateCommand(OnCloseStorage);
    }

    private void OnCloseStorage()
    {
        _storageService.CloseStorage();

        RegionManager.RequestNavigationToMainContent("CreateOrOpenStorage");
    }

    private void OnExit()
    {
        Application.Current.Shutdown(0);
    }

    public DelegateCommand ExitCommand { get; }

    public DelegateCommand CloseStorageCommand { get; }
}