using System;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Images;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public partial class StorageDetailsViewModel : ObservableViewModelBase
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

    [RelayCommand]
    private void EditStorage()
    {
        RegionManager.RequestNavigationToDetails("EditStorageDetails");
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ImageSource _icon = Icons.DefaultItemIcon;

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var storageInfo = _storageInfoRepository.GetStorageInfo();

        Title = storageInfo.Title;
        Description = storageInfo.Description ?? string.Empty;
        Icon = _iconsRepository.GetIcon(storageInfo.IconId);
    }
}