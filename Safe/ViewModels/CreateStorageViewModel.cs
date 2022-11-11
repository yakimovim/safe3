using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Services;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public partial class CreateStorageViewModel : ObservableViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly IStorageService _storageService;
    private string? _storageFilePath;

    internal CreateStorageViewModel(
        IConfigurationService configurationService,
        IStorageService storageService
        )
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }

    private bool CanCreate()
    {
        return !string.IsNullOrWhiteSpace(_storageFilePath)
            && !string.IsNullOrEmpty(_title)
            && !string.IsNullOrEmpty(_password)
            && string.Equals(_password, _confirmPassword);
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        _storageService.CreateStorage(new StorageCreationOptions
        {
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

        Title = string.Empty;
        Description = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;

        _storageFilePath = storageFilePath;
        CreateCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [IsNotNullOrWhiteSpace(nameof(Title))]
    [NotifyDataErrorInfo]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [IsNotNullOrWhiteSpace(nameof(Password))]
    [NotifyDataErrorInfo]
    private string _password = string.Empty;

    partial void OnPasswordChanged(string value)
    {
        ValidateProperty(value, nameof(ConfirmPassword));
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    [CustomValidation(typeof(CreateStorageViewModel), nameof(ValidateConfirmPassword))]
    [NotifyDataErrorInfo]
    private string _confirmPassword = string.Empty;


    public static ValidationResult ValidateConfirmPassword(object? value, ValidationContext context)
    {
        var instance = (CreateStorageViewModel) context.ObjectInstance;

        if (instance.ConfirmPassword != instance.Password)
        {
            return new((string)Application.Current.Resources["PasswordsAreDifferentValidationMessage"]);
        }

        return ValidationResult.Success!;
    }
}
