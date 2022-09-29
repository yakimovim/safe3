using System;
using System.Collections.ObjectModel;
using System.Globalization;
using EdlinSoftware.Safe.Services;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

internal class SettingsViewModel : ViewModelBase
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

        CancelCommand = new DelegateCommand(OnCancel);
        OkCommand = new DelegateCommand(OnOk);
    }

    private void OnOk()
    {
        _languagesService.CurrentLanguage = SelectedLanguage;

        var configuration = _configurationService.GetConfiguration();

        configuration.Language = _languagesService.CurrentLanguage.Name;

        _configurationService.SaveConfiguration(configuration);

        _journal.GoBack();
    }

    private void OnCancel()
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

    private CultureInfo _selectedLanguage;
    public CultureInfo SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public DelegateCommand CancelCommand { get; set; }
    public DelegateCommand OkCommand { get; set; }
}