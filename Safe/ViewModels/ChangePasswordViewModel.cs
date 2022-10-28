using System;
using System.Windows;
using EdlinSoftware.Safe.Services;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

internal class ChangePasswordViewModel : ViewModelBase
{
    private readonly IStorageService _storageService;
    private IRegionNavigationJournal _journal = null!;

    private string _oldPassword = string.Empty;
    public string OldPassword
    {
        get => _oldPassword;
        set => SetProperty(ref _oldPassword, value, Validate);
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value, Validate);
    }
    
    private string _confirmNewPassword = string.Empty;
    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value, Validate);
    }

    private bool _oldPasswordIsValid = true;
    public bool OldPasswordIsValid
    {
        get => _oldPasswordIsValid;
        set => SetProperty(ref _oldPasswordIsValid, value);
    }

    public DelegateCommand ChangePasswordCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public ChangePasswordViewModel(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

        CancelCommand = new DelegateCommand(OnCancel);
        ChangePasswordCommand = new DelegateCommand(OnChangePassword, CanChangePassword)
            .ObservesProperty(() => OldPassword)
            .ObservesProperty(() => NewPassword)
            .ObservesProperty(() => ConfirmNewPassword);
    }

    private bool CanChangePassword()
    {
        return !string.IsNullOrEmpty(_oldPassword)
               && !string.IsNullOrEmpty(_newPassword)
               && string.Equals(_newPassword, _confirmNewPassword);
    }

    private void OnChangePassword()
    {
        if (!_storageService.ChangePassword(OldPassword, NewPassword))
        {
            OldPasswordIsValid = false;
        }
        else
        {
            _journal.GoBack();
        }
    }

    private void OnCancel()
    {
        _journal.GoBack();
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _journal = navigationContext.NavigationService.Journal;

        if (!_storageService.StorageIsOpened)
        {
            _journal.GoBack();
        }

        OldPasswordIsValid = true;
        OldPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
    }

    private void Validate()
    {
        CheckNullOrEmpty(OldPassword, nameof(OldPassword));
        CheckNullOrEmpty(NewPassword, nameof(NewPassword));

        if (ConfirmNewPassword != NewPassword)
        {
            ValidationErrors[nameof(ConfirmNewPassword)] = (string) Application.Current.Resources["PasswordsAreDifferentValidationMessage"];
        }
        else
        {
            ValidationErrors.Remove(nameof(ConfirmNewPassword));
        }

        RaiseErrorsChanged(nameof(ConfirmNewPassword));
    }

}