using System;
using System.Collections.ObjectModel;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class StorageContentViewModel : BindableBase, INavigationAware
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager;
    private readonly IItemsRepository _itemsRepository;

    public StorageContentViewModel(
        IEventAggregator eventAggregator,
        IRegionManager regionManager,
        IItemsRepository itemsRepository)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));

        OnStorageChanged();

        _eventAggregator.GetEvent<StorageChanged>().Subscribe(OnStorageChanged);

        CreateItemCommand = new DelegateCommand(OnCreateItem, CanCreateItem)
            .ObservesProperty(() => SelectedItem);
        DeleteItemCommand = new DelegateCommand(OnDeleteItem, CanDeleteItem)
            .ObservesProperty(() => SelectedItem);
    }

    private bool CanCreateItem()
    {
        return SelectedItem != null;
    }

    private void OnCreateItem()
    {
        var item = new Item(SelectedItem!.Item)
        {
            Title = "T" + DateTime.UtcNow.ToLongTimeString(),
            Description = "D" + DateTime.UtcNow.ToLongTimeString(),
        };

        _itemsRepository.SaveItem(item);

        SelectedItem.SubItems.Add(new ItemTreeViewModel(_eventAggregator, _regionManager, _itemsRepository, item) { Parent = SelectedItem });
    }

    private bool CanDeleteItem()
    {
        return SelectedItem != null 
               && SelectedItem.Item != null
               && SelectedItem.Parent != null;
    }

    private void OnDeleteItem()
    {
        var itemToDelete = SelectedItem!;

        var parentOfItemToDelete = itemToDelete.Parent!;

        parentOfItemToDelete.SubItems.Remove(itemToDelete);

        _itemsRepository.DeleteItem(itemToDelete.Item!);
    }

    private void OnStorageChanged()
    {
        SubItems = new ObservableCollection<ItemTreeViewModel>(new []{ new ItemTreeViewModel(_eventAggregator, _regionManager, _itemsRepository) });
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
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
                _regionManager.RequestNavigate("DetailsRegion", "ItemDetails", parameters);
            }
        }
    }

    public DelegateCommand CreateItemCommand { get; }

    public DelegateCommand DeleteItemCommand { get; }
}

public class ItemTreeViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager;
    private readonly IItemsRepository _itemsRepository;
    public readonly Item? Item;

    private ItemTreeViewModel? _parent;

    public ItemTreeViewModel? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }

    public ItemTreeViewModel(
        IEventAggregator eventAggregator,
        IRegionManager regionManager,
        IItemsRepository itemsRepository, 
        Item? item = null)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        Item = item;

        Text = Item?.Title ?? "Root";
        Tooltip = Item?.Description ?? string.Empty;

        _eventAggregator.GetEvent<NewItemCreated>()
            .Subscribe(OnNewItemCreated, ThreadOption.PublisherThread,
                false, HandleNewItemCreated);

        _subItems = new Lazy<ObservableCollection<ItemTreeViewModel>>(CreateSubItems);

        DeleteItemCommand = new DelegateCommand(OnDeleteItem, CanDeleteItem)
            .ObservesProperty(() => Parent);

        CreateItemCommand = new DelegateCommand(OnCreateItem);
    }

    private void OnNewItemCreated((Item NewItem, Item? parentItem) info)
    {
        _itemsRepository.SaveItem(info.NewItem);

        SubItems.Add(
            new ItemTreeViewModel(_eventAggregator, _regionManager, _itemsRepository, info.NewItem)
            {
                Parent = this
            }
        );

        var parameters = new NavigationParameters
            { { "Item", info.NewItem } };
        _regionManager.RequestNavigate("DetailsRegion", "ItemDetails", parameters);
    }

    private bool HandleNewItemCreated((Item NewItem, Item? ParentItem) info)
    {
        return ReferenceEquals(Item, info.ParentItem);
    }

    private bool CanDeleteItem()
    {
        return Item != null
               && Parent != null;
    }

    private void OnDeleteItem()
    {
        Parent!.SubItems.Remove(this);

        _itemsRepository.DeleteItem(Item!);
    }

    private void OnCreateItem()
    {
        var parameters = new NavigationParameters
            { { "Parent", Item } };
        _regionManager.RequestNavigate("DetailsRegion", "CreateItem", parameters);
    }

    private ObservableCollection<ItemTreeViewModel> CreateSubItems()
    {
        var subItems = _itemsRepository.GetChildItems(Item)
            .Select(i => new ItemTreeViewModel(_eventAggregator, _regionManager, _itemsRepository, i) { Parent = this })
            .ToArray();

        return new ObservableCollection<ItemTreeViewModel>(subItems);
    }

    private string _text;
    public string Text
    {
        get { return _text; }
        set { SetProperty(ref _text, value); }
    }

    private string _tooltip;
    public string Tooltip
    {
        get { return _tooltip; }
        set { SetProperty(ref _tooltip, value); }
    }

    private readonly Lazy<ObservableCollection<ItemTreeViewModel>> _subItems;

    public ObservableCollection<ItemTreeViewModel> SubItems => _subItems.Value;

    public void MoveTo(ItemTreeViewModel targetItem)
    {
        if(Parent != null)
            Parent.SubItems.Remove(this);

        targetItem.SubItems.Add(this);
        Parent = targetItem;

        if (Item != null)
        {
            Item.MoveTo(targetItem.Item);
            _itemsRepository.SaveItem(Item);
        }
    }

    public DelegateCommand DeleteItemCommand { get; }

    public DelegateCommand CreateItemCommand { get; }
}