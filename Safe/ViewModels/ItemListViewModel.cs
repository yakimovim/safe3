using System;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemListViewModel : ItemViewModelBase
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;
    public readonly Item Item;

    public ItemListViewModel(
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository,
        Item item)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
        Item = item;

        Title = Item.Title;
        Description = Item.Description ?? string.Empty;
        Icon = _iconsRepository.GetIcon(Item.IconId);

        DeleteItemCommand = new DelegateCommand(OnDeleteItem);

        EditItemCommand = new DelegateCommand(OnEditItem);
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<ItemChanged>()
            .Subscribe(OnItemChanged, ThreadOption.PublisherThread,
                false, HandleItemChanged);
    }

    private void OnItemChanged(Item item)
    {
        Title = item.Title;
        Description = item.Description ?? string.Empty;
        Icon = _iconsRepository.GetIcon(item.IconId);
    }

    private bool HandleItemChanged(Item item)
    {
        return item.Equals(Item);
    }

    private void OnDeleteItem()
    {
        _itemsRepository.DeleteItem(Item);

        EventAggregator.GetEvent<ItemDeleted>().Publish(Item);
    }

    private void OnEditItem()
    {
        var parameters = new NavigationParameters
                    { { "Item", Item } };
        RegionManager.RequestNavigationToDetails("EditItem", parameters);
    }

    public DelegateCommand DeleteItemCommand { get; }

    public DelegateCommand EditItemCommand { get; }
}