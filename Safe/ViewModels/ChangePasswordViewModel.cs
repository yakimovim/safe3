using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Services;
using Prism.Regions;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace EdlinSoftware.Safe.ViewModels;

public partial class ChangePasswordViewModel : ObservableViewModelBase
{
    private readonly IStorageService _storageService;
    private IRegionNavigationJournal _journal = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    [IsNotNullOrEmpty]
    [NotifyDataErrorInfo]
    private string _oldPassword = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    [IsNotNullOrEmpty]
    [NotifyDataErrorInfo]
    private string _newPassword = string.Empty;

    partial void OnNewPasswordChanged(string value)
    {
        ValidateProperty(value, nameof(ConfirmNewPassword));
    }
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    [CustomValidation(typeof(ChangePasswordViewModel), nameof(ValidateConfirmNewPassword))]
    [NotifyDataErrorInfo]
    private string _confirmNewPassword = string.Empty;

    [ObservableProperty]
    private bool _oldPasswordIsValid = true;

    internal ChangePasswordViewModel(IStorageService storageService)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    private bool CanChangePassword()
    {
        return !string.IsNullOrEmpty(_oldPassword)
               && !string.IsNullOrEmpty(_newPassword)
               && string.Equals(_newPassword, _confirmNewPassword);
    }

    [RelayCommand(CanExecute = nameof(CanChangePassword))]
    private void ChangePassword()
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

    [RelayCommand]
    private void Cancel()
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

    public static ValidationResult ValidateConfirmNewPassword(string value, ValidationContext context)
    {
        var instance = (ChangePasswordViewModel)context.ObjectInstance;

        if (instance.ConfirmNewPassword != instance.NewPassword)
        {
            return new((string) Application.Current.Resources["PasswordsAreDifferentValidationMessage"]);
        }

        return ValidationResult.Success!;
    }
}