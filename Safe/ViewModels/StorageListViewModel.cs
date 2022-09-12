using System;
using System.Collections.ObjectModel;
using System.Linq;
using EdlinSoftware.Safe.Domain;
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

    private ObservableCollection<ItemTreeViewModel> _items;
    public ObservableCollection<ItemTreeViewModel> Items
    {
        get { return _items; }
        set { SetProperty(ref _items, value); }
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

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        var searchText = navigationContext.Parameters.GetValue<string>("SearchText");

        var searchModelBuilder = new SearchModelBuilder();

        var searchModel = searchModelBuilder.GetSearchStringElements(searchText);

        var foundItems = _itemsRepository.Find(searchModel);

        Items = new ObservableCollection<ItemTreeViewModel>(foundItems.Select(i =>
            new ItemTreeViewModel(EventAggregator, RegionManager, _itemsRepository, i)));

        if (Items.Any())
        {
            SelectedItem = Items[0];
        }
    }
}