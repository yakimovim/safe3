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
    private readonly IItemsRepository _itemsRepository;

    public StorageContentViewModel(
        IEventAggregator eventAggregator,
        IItemsRepository itemsRepository)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
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

        SelectedItem.SubItems.Add(new ItemTreeViewModel(_itemsRepository, item) { Parent = SelectedItem });
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
        SubItems = new ObservableCollection<ItemTreeViewModel>(new []{ new ItemTreeViewModel(_itemsRepository) });
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
        set { SetProperty(ref _selectedItem, value); }
    }

    public DelegateCommand CreateItemCommand { get; }

    public DelegateCommand DeleteItemCommand { get; }
}

public class ItemTreeViewModel : BindableBase
{
    private readonly IItemsRepository _itemsRepository;
    public readonly Item? Item;
    
    public ItemTreeViewModel? Parent { get; set; }

    public ItemTreeViewModel(IItemsRepository itemsRepository, Item? item = null)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        Item = item;

        Text = Item?.Title ?? "Root";
        Tooltip = Item?.Description ?? string.Empty;

        _subItems = new Lazy<ObservableCollection<ItemTreeViewModel>>(CreateSubItems);
    }

    private ObservableCollection<ItemTreeViewModel> CreateSubItems()
    {
        var subItems = _itemsRepository.GetChildItems(Item)
            .Select(i => new ItemTreeViewModel(_itemsRepository, i) { Parent = this })
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
}