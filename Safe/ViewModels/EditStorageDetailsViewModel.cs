using System;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using EdlinSoftware.Safe.Storage.Model;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels;

public class EditStorageDetailsViewModel : ViewModelBase
{
    private readonly IStorageInfoRepository _storageInfoRepository;
    private readonly IIconsRepository _iconsRepository;

    private string? _iconId;
    public string? IconId
    {
        get => _iconId;
        set => SetProperty(ref _iconId, value);
    }

    public EditStorageDetailsViewModel(
        IStorageInfoRepository storageInfoRepository,
        IIconsRepository iconsRepository
        )
    {
        _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        SaveChangesCommand = new DelegateCommand(OnSaveChanges, CanSaveChanges)
            .ObservesProperty(() => Title);
        CancelCommand = new DelegateCommand(OnCancel);
        ClearIconCommand = new DelegateCommand(OnClearIcon);
        SelectIconCommand = new DelegateCommand(OnSelectIcon);
    }
    private void OnSelectIcon()
    {
        DialogService.ShowDialog("IconsDialog", new DialogParameters(), result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                var iconId = result.Parameters.GetValue<string>("IconId");

                IconId = iconId;

                Icon = _iconsRepository.GetIcon(IconId);
            }
        });
    }

    private void OnClearIcon()
    {
        IconId = null;
        Icon = Icons.DefaultItemIcon;
    }

    private void OnSaveChanges()
    {
        _storageInfoRepository.SaveStorageInfo(new StorageInfo
        {
            Title = Title,
            Description = Description,
            IconId = IconId
        });

        RegionManager.RequestNavigationToDetails("StorageDetails");

        EventAggregator.GetEvent<StorageDetailsChanged>().Publish();
    }

    private bool CanSaveChanges() => !string.IsNullOrWhiteSpace(Title);

    private void OnCancel()
    {
        RegionManager.RequestNavigationToDetails("StorageDetails");
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value, Validate);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private ImageSource _icon = Icons.DefaultItemIcon;
    public ImageSource Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var storageInfo = _storageInfoRepository.GetStorageInfo();

        Title = storageInfo.Title;
        Description = storageInfo.Description ?? string.Empty;
        IconId = storageInfo.IconId;
    }

    public DelegateCommand SaveChangesCommand { get; }

    public DelegateCommand CancelCommand { get; }

    public DelegateCommand ClearIconCommand { get; }

    public DelegateCommand SelectIconCommand { get; }

    private void Validate()
    {
        CheckNullOrWhiteSpace(Title, nameof(Title));
    }
}