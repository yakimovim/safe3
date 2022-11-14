using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Services;
using Microsoft.Win32;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

internal partial class ExportStorageViewModel : ObservableViewModelBase
{
    private readonly IStorageService _storageService;
    private IRegionNavigationJournal _journal = null!;

    public ExportStorageViewModel(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    private bool CanExport()
    {
        return !string.IsNullOrEmpty(_password);
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void Export()
    {
        var openDialog = new OpenFileDialog
        {
            CheckFileExists = false,
            CheckPathExists = false,
            Filter = $"{Application.Current.Resources["JsonFileFilter"]}|*.json",
            AddExtension = true,
            DefaultExt = ".json",
        };

        if (openDialog.ShowDialog() == true)
        {

            if (_storageService.Export(Password, openDialog.FileName))
            {
                _journal.GoBack();
            }
            else
            {
                PasswordIsValid = false;
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _journal.GoBack();
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _journal = navigationContext.NavigationService.Journal;

        Password = string.Empty;
        PasswordIsValid = true;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _passwordIsValid = true;
}