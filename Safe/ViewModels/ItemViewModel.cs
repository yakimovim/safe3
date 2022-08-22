﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using Prism.Mvvm;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemViewModel : BindableBase, IItemViewModelOwner
{
    private readonly IItemsRepository _itemsRepository;
    private IItemViewModelOwner _owner;
    internal readonly Item Item;

    private readonly ObservableCollection<string> _tags;
    private readonly Lazy<ObservableCollection<ItemViewModel>> _subItems;

    public ItemViewModel(
        IItemsRepository itemsRepository,
        IItemViewModelOwner owner,
        Item? item = null)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));

        if (item == null)
        {
            item = new Item(_owner.Owner?.Item);
            _itemsRepository.SaveItem(item);
        }

        Item = item;

        _tags = new ObservableCollection<string>(Item.Tags);
        _tags.CollectionChanged += OnTagsCollectionChanged;

        _subItems = new Lazy<ObservableCollection<ItemViewModel>>(() =>
        {
            var subItems = _itemsRepository.GetChildItems(Item);

            var collection = new ObservableCollection<ItemViewModel>(
                subItems.Select(i => new ItemViewModel(_itemsRepository, this, i))
            );

            collection.CollectionChanged += OnSubItemsCollectionChanged;
            return collection;
        });
    }

    private void OnSubItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (ItemViewModel vmItem in e.NewItems)
                    {
                        vmItem._owner.SubItems.Remove(vmItem);
                        vmItem._owner = this;
                        vmItem.Item.MoveTo(Item);
                        _itemsRepository.SaveItem(vmItem.Item);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (ItemViewModel vmItem in e.OldItems)
                    {
                    }
                }
                break;
        }
    }

    private void OnTagsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    Item.Tags.AddRange(e.NewItems.OfType<string>());
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (string tag in e.OldItems)
                    {
                        Item.Tags.Remove(tag);
                    }
                }
                break;
        }
        _itemsRepository.SaveItem(Item);
    }

    public string Title
    {
        get => Item.Title;
        set
        {
            if (Item.Title != value)
            {
                Item.Title = value;
                _itemsRepository.SaveItem(Item);
                RaisePropertyChanged();
            }
        }
    }

    public string Description
    {
        get => Item.Description;
        set
        {
            if (Item.Description != value)
            {
                Item.Description = value;
                _itemsRepository.SaveItem(Item);
                RaisePropertyChanged();
            }
        }
    }

    public ObservableCollection<string> Tags => _tags;

    public ItemViewModel Owner => this;

    public ObservableCollection<ItemViewModel> SubItems => _subItems.Value;
}