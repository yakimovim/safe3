using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemListViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRegionManager _regionManager;
    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;
    public readonly Item Item;

    public ItemListViewModel(
        IEventAggregator eventAggregator,
        IRegionManager regionManager,
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository,
        Item item)
    {
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
        Item = item;

        Text = Item.Title;
        Tooltip = Item.Description ?? string.Empty;
        Icon = _iconsRepository.GetIcon(Item.IconId);

        _eventAggregator.GetEvent<ItemChanged>()
            .Subscribe(OnItemChanged, ThreadOption.PublisherThread,
                false, HandleItemChanged);

        DeleteItemCommand = new DelegateCommand(OnDeleteItem);

        EditItemCommand = new DelegateCommand(OnEditItem);
    }

    private void OnItemChanged(Item item)
    {
        Text = item.Title;
        Tooltip = item.Description ?? string.Empty;
        Icon = _iconsRepository.GetIcon(item.IconId);
    }

    private bool HandleItemChanged(Item item)
    {
        return item.Equals(Item);
    }

    private void OnDeleteItem()
    {
        _itemsRepository.DeleteItem(Item);

        _eventAggregator.GetEvent<ItemDeleted>().Publish(Item);
    }

    private void OnEditItem()
    {
        var parameters = new NavigationParameters
                    { { "Item", Item } };
        _regionManager.RequestNavigationToDetails("EditItem", parameters);
    }

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    private string _tooltip = string.Empty;
    public string Tooltip
    {
        get => _tooltip;
        set => SetProperty(ref _tooltip, value);
    }

    private ImageSource _icon;
    public ImageSource Icon
    {
        get { return _icon; }
        set { SetProperty(ref _icon, value); }
    }

    public DelegateCommand DeleteItemCommand { get; }

    public DelegateCommand EditItemCommand { get; }
}