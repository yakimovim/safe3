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

        _eventAggregator.GetEvent<ItemChanged>()
            .Subscribe(OnItemChanged, ThreadOption.PublisherThread,
                false, HandleItemChanged);

        _subItems = new Lazy<ObservableCollection<ItemTreeViewModel>>(CreateSubItems);

        DeleteItemCommand = new DelegateCommand(OnDeleteItem, CanDeleteItem)
            .ObservesProperty(() => Parent);

        CreateItemCommand = new DelegateCommand(OnCreateItem);

        EditItemCommand = new DelegateCommand(OnEditItem, CanEditItem);
    }

    private void OnItemChanged(Item item)
    {
        Text = item.Title ?? "Root";
        Tooltip = item.Description ?? string.Empty;
    }

    private bool HandleItemChanged(Item item)
    {
        return ReferenceEquals(Item, item);
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
        _regionManager.RequestNavigationToDetails("ItemDetails", parameters);
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
        _regionManager.RequestNavigationToDetails("CreateItem", parameters);
    }

    private bool CanEditItem()
    {
        return Item != null;
    }

    private void OnEditItem()
    {
        var parameters = new NavigationParameters
                    { { "Item", Item } };
        _regionManager.RequestNavigationToDetails("EditItem", parameters);
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

    public DelegateCommand EditItemCommand { get; }
}