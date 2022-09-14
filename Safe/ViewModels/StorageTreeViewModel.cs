﻿using System;
using System.Collections.ObjectModel;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Events;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class StorageTreeViewModel : ViewModelBase
    {
        private readonly IItemsRepository _itemsRepository;

        public StorageTreeViewModel(
            IItemsRepository itemsRepository)
        {
            _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        }

        protected override void SubscribeToEvents()
        {
            EventAggregator.GetEvent<StorageChanged>().Subscribe(OnStorageChanged);

            OnStorageChanged();
        }

        private void OnStorageChanged()
        {
            SubItems = new ObservableCollection<ItemTreeViewModel>(new[] { new ItemTreeViewModel(EventAggregator, RegionManager, _itemsRepository) });
        }

        private ObservableCollection<ItemTreeViewModel> _subItems;
        public ObservableCollection<ItemTreeViewModel> SubItems
        {
            get { return _subItems; }
            set { SetProperty(ref _subItems, value); }
        }

        private ItemTreeViewModel? _selectedItem;

        public ItemTreeViewModel? SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    var parameters = new NavigationParameters
                    { { "Item", value.Item } };
                    RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
                }
            }
        }

    }
}