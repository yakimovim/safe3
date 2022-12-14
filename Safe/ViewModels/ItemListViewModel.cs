using System;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using Prism.Events;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public partial class ItemListViewModel : ItemViewModelBase
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
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<ItemChanged>()
            .Subscribe(OnItemChanged, ThreadOption.PublisherThread,
                false, HandleItemChanged);

        EventAggregator.GetEvent<IconRemoved>()
            .Subscribe(OnIconRemoved, ThreadOption.PublisherThread,
                false, HandleIconRemoved);
    }

    private bool HandleIconRemoved(string iconId)
    {
        return iconId == Item.IconId;
    }

    private void OnIconRemoved(string iconId)
    {
        Item.IconId = null;
        Icon = Icons.DefaultItemIcon;
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

    [RelayCommand]
    private void DeleteItem()
    {
        _itemsRepository.DeleteItem(Item);

        EventAggregator.GetEvent<ItemDeleted>().Publish(Item);
    }

    [RelayCommand]
    private void EditItem()
    {
        var parameters = new NavigationParameters
                    { { "Item", Item } };
        RegionManager.RequestNavigationToDetails("EditItem", parameters);
    }
}