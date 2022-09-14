using System;
using System.Collections.ObjectModel;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Search;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class StorageListViewModel : ViewModelBase
{
    private readonly IItemsRepository _itemsRepository;

    public StorageListViewModel(
        IItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<ItemDeleted>()
            .Subscribe(OnItemDeleted);
    }

    private void OnItemDeleted(Item item)
    {
        if (_items == null) return;

        var itemViewModel = _items
            .FirstOrDefault(i => i.Item.Equals(item));

        if (itemViewModel != null)
        {
            _items.Remove(itemViewModel);
        }
    }

    private ObservableCollection<ItemListViewModel> _items;
    public ObservableCollection<ItemListViewModel> Items
    {
        get { return _items; }
        set { SetProperty(ref _items, value); }
    }

    private ItemListViewModel? _selectedItem;

    public ItemListViewModel? SelectedItem
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

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var searchText = navigationContext.Parameters.GetValue<string>("SearchText");

        var searchModelBuilder = new SearchModelBuilder();

        var searchModel = searchModelBuilder.GetSearchStringElements(searchText);

        var foundItems = _itemsRepository.Find(searchModel);

        Items = new ObservableCollection<ItemListViewModel>(foundItems.Select(i =>
            new ItemListViewModel(EventAggregator, RegionManager, _itemsRepository, i)));

        if (Items.Any())
        {
            SelectedItem = Items[0];
        }
    }
}