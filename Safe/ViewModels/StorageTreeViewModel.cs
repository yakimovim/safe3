using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Services;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

internal partial class StorageTreeViewModel : ObservableViewModelBase
{
    private readonly IStorageService _storageService;
    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;
    private readonly IStorageInfoRepository _storageInfoRepository;

    public StorageTreeViewModel(
        IStorageService storageService,
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository,
        IStorageInfoRepository storageInfoRepository)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
        _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<StorageChanged>().Subscribe(OnStorageChanged);

        OnStorageChanged();
    }

    private void OnStorageChanged()
    {
        if (!_storageService.StorageIsOpened)
        {
            SubItems = new ObservableCollection<ItemTreeViewModel>();
            return;
        }

        var storageInfo = _storageInfoRepository.GetStorageInfo();

        SubItems = new ObservableCollection<ItemTreeViewModel>(new[]
        {
                new ItemTreeViewModel(_itemsRepository, _iconsRepository, _storageInfoRepository)
                {
                    DialogService = DialogService,
                    RegionManager = RegionManager,
                    EventAggregator = EventAggregator,
                    Title = storageInfo.Title,
                    Description = storageInfo.Description ?? string.Empty,
                    IconId = storageInfo.IconId
                }
            });
    }

    [ObservableProperty]
    private ObservableCollection<ItemTreeViewModel> _subItems = new();

    [ObservableProperty]
    private ItemTreeViewModel? _selectedItem;

    partial void OnSelectedItemChanged(ItemTreeViewModel? value)
    {
        if (value != null)
        {
            if (value.Parent == null)
            {
                RegionManager.RequestNavigationToDetails("StorageDetails");
            }
            else
            {
                var parameters = new NavigationParameters
                    { { "Item", value.Item } };
                RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
            }
        }
    }
}