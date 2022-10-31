using System;
using System.Windows;
using EdlinSoftware.Safe.Services;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

internal class ExportStorageViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private IRegionNavigationJournal _journal = null!;

    public ExportStorageViewModel(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

        ExportCommand = new DelegateCommand(OnExport, CanExport)
            .ObservesProperty(() => Password);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    private bool CanExport()
    {
        return !string.IsNullOrEmpty(_password);
    }

    private void OnExport()
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

    private void OnCancel()
    {
        _journal.GoBack();
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _journal = navigationContext.NavigationService.Journal;

        Password = string.Empty;
        PasswordIsValid = true;
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private bool _passwordIsValid = true;
    public bool PasswordIsValid
    {
        get => _passwordIsValid;
        set => SetProperty(ref _passwordIsValid, value);
    }

    public DelegateCommand ExportCommand { get; set; }
    public DelegateCommand CancelCommand { get; set; }
}