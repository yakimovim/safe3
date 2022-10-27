using System;
using System.Collections.ObjectModel;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Services;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class StorageTreeViewModel : ViewModelBase
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

        private ObservableCollection<ItemTreeViewModel> _subItems = new();
        public ObservableCollection<ItemTreeViewModel> SubItems
        {
            get => _subItems;
            set => SetProperty(ref _subItems, value);
        }

        private ItemTreeViewModel? _selectedItem;

        public ItemTreeViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
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

    }
}
