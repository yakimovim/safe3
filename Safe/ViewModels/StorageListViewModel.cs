using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Search;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public partial class StorageListViewModel : ObservableViewModelBase
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;

    public StorageListViewModel(
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<ItemDeleted>()
            .Subscribe(OnItemDeleted);
    }

    private void OnItemDeleted(Item item)
    {
        var itemViewModel = _items
            .FirstOrDefault(i => i.Item.Equals(item));

        if (itemViewModel != null)
        {
            _items.Remove(itemViewModel);
        }
    }

    [ObservableProperty]
    private ObservableCollection<ItemListViewModel> _items = new();

    [ObservableProperty]
    private ItemListViewModel? _selectedItem;

    partial void OnSelectedItemChanged(ItemListViewModel? value)
    {
        if (value != null)
        {
            var parameters = new NavigationParameters
                { { "Item", value.Item } };
            RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
        }
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var searchText = navigationContext.Parameters.GetValue<string>("SearchText");

        var searchModelBuilder = new SearchModelBuilder();

        var searchModel = searchModelBuilder.GetSearchStringElements(searchText);

        var foundItems = _itemsRepository.Find(searchModel);

        Items = new ObservableCollection<ItemListViewModel>(foundItems.Select(i =>
            new ItemListViewModel(_itemsRepository, _iconsRepository, i)
            {
                EventAggregator = EventAggregator,
                RegionManager = RegionManager
            }));

        if (Items.Any())
        {
            SelectedItem = Items[0];
        }
    }
}