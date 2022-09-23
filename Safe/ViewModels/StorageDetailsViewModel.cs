using System;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Images;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class StorageDetailsViewModel : ViewModelBase
{
    private readonly IStorageInfoRepository _storageInfoRepository;
    private readonly IIconsRepository _iconsRepository;

    public StorageDetailsViewModel(
        IStorageInfoRepository storageInfoRepository,
        IIconsRepository iconsRepository)
    {
        _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
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
        Icon = _iconsRepository.GetIcon(storageInfo.IconId);
    }
}