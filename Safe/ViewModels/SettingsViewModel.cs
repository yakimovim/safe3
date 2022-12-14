using System;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Services;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

internal partial class SettingsViewModel : ObservableViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ILanguagesService _languagesService;

    private IRegionNavigationJournal _journal = null!;

    public SettingsViewModel(
        IConfigurationService configurationService,
        ILanguagesService languagesService
        )
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _languagesService = languagesService ?? throw new ArgumentNullException(nameof(languagesService));
    }

    [RelayCommand]
    private void Ok()
    {
        _languagesService.CurrentLanguage = SelectedLanguage;

        var configuration = _configurationService.GetConfiguration();

        configuration.Language = _languagesService.CurrentLanguage.Name;

        _configurationService.SaveConfiguration(configuration);

        _journal.GoBack();
    }

    [RelayCommand]
    private void Cancel()
    {
        _journal.GoBack();
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        _journal = navigationContext.NavigationService.Journal;

        Languages.Clear();
        Languages.AddRange(_languagesService.AvailableLanguages);
        SelectedLanguage = _languagesService.CurrentLanguage;
    }

    public ObservableCollection<CultureInfo> Languages { get; } = new();

    [ObservableProperty]
    private CultureInfo _selectedLanguage = null!;
}