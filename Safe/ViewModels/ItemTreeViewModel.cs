﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using EdlinSoftware.Safe.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemTreeViewModel : ItemViewModelBase
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;
    private readonly IStorageInfoRepository _storageInfoRepository;
    public readonly Item? Item;

    private ItemTreeViewModel? _parent;

    public ItemTreeViewModel? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }

    private string? _iconId;
    public string? IconId
    {
        get => _iconId;
        set
        {
            if (SetProperty(ref _iconId, value))
            {
                Icon = _iconsRepository.GetIcon(value);
            }
        }
    }

    public ItemTreeViewModel(
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository,
        IStorageInfoRepository storageInfoRepository,
        Item? item = null)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
        _storageInfoRepository = storageInfoRepository ?? throw new ArgumentNullException(nameof(storageInfoRepository));

        Item = item;
        IconId = Item?.IconId;

        Title = Item?.Title ?? "Root";
        Description = Item?.Description ?? string.Empty;

        _subItems = new Lazy<ObservableCollection<ItemTreeViewModel>>(CreateSubItems);

        DeleteItemCommand = new DelegateCommand(OnDeleteItem, CanDeleteItem)
            .ObservesProperty(() => Parent);

        CreateItemCommand = new DelegateCommand(OnCreateItem);

        EditItemCommand = new DelegateCommand(OnEditItem);
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<NewItemCreated>()
            .Subscribe(OnNewItemCreated, ThreadOption.PublisherThread,
                false, HandleNewItemCreated);

        if (Item != null)
        {
            EventAggregator.GetEvent<ItemChanged>()
                .Subscribe(OnItemChanged, ThreadOption.PublisherThread,
                    false, HandleItemChanged);
        }
        else
        {
            EventAggregator.GetEvent<StorageDetailsChanged>()
                .Subscribe(OnStorageDetailsChanged);
        }

        EventAggregator.GetEvent<IconRemoved>()
            .Subscribe(OnIconRemoved, ThreadOption.PublisherThread,
                false, HandleIconRemoved);

        EventAggregator.GetEvent<ItemDeleted>()
            .Subscribe(OnItemDeleted, ThreadOption.PublisherThread,
                false, HandleItemDeleted);
    }

    private bool HandleIconRemoved(string iconId)
    {
        return iconId == IconId;
    }

    private void OnIconRemoved(string iconId)
    {
        if (Item != null)
        {
            Item.IconId = null;
        }

        IconId = null;
    }

    private void OnItemDeleted(Item item)
    {
        Parent!.SubItems.Remove(this);
    }

    private bool HandleItemDeleted(Item item)
    {
        return item.Equals(Item);
    }

    private void OnItemChanged(Item item)
    {
        Title = item.Title;
        Description = item.Description ?? string.Empty;
        Icon = _iconsRepository.GetIcon(item.IconId);
    }

    private void OnStorageDetailsChanged()
    {
        var storageInfo = _storageInfoRepository.GetStorageInfo();

        Title = storageInfo.Title;
        Description = storageInfo.Description ?? string.Empty;
        IconId = storageInfo.IconId;
    }

    private bool HandleItemChanged(Item item)
    {
        return item.Equals(Item);
    }

    private void OnNewItemCreated((Item NewItem, Item? parentItem) info)
    {
        _itemsRepository.SaveItem(info.NewItem);

        SubItems.Add(
            new ItemTreeViewModel(_itemsRepository, _iconsRepository, _storageInfoRepository, info.NewItem)
            {
                Parent = this,
                DialogService = DialogService,
                RegionManager = RegionManager,
                EventAggregator = EventAggregator
            }
        );

        var parameters = new NavigationParameters
            { { "Item", info.NewItem } };
        RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
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
        DialogService.ShowConfirmationDialog((string) Application.Current.Resources["DeleteItemConfirmation"], res =>
        {
            if (res == ButtonResult.Yes)
            {
                Parent!.SubItems.Remove(this);

                _itemsRepository.DeleteItem(Item!);
            }
        });
    }

    private void OnCreateItem()
    {
        var parameters = new NavigationParameters
            { { "Parent", Item } };
        RegionManager.RequestNavigationToDetails("CreateItem", parameters);
    }

    private void OnEditItem()
    {
        if (Item != null)
        {
            var parameters = new NavigationParameters
                { { "Item", Item } };
            RegionManager.RequestNavigationToDetails("EditItem", parameters);
        }
        else
        {
            RegionManager.RequestNavigationToDetails("EditStorageDetails");
        }
    }

    private ObservableCollection<ItemTreeViewModel> CreateSubItems()
    {
        var subItems = _itemsRepository.GetChildItems(Item)
            .Select(i => new ItemTreeViewModel(_itemsRepository, _iconsRepository, _storageInfoRepository, i)
            {
                Parent = this,
                DialogService = DialogService,
                EventAggregator = EventAggregator,
                RegionManager = RegionManager
            })
            .ToArray();

        return new ObservableCollection<ItemTreeViewModel>(subItems);
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