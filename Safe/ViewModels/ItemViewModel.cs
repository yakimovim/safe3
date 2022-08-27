using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using Prism.Commands;
using Prism.Mvvm;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemViewModel : BindableBase, IItemViewModelOwner
{
    private readonly IItemsRepository _itemsRepository;
    private IItemViewModelOwner? _owner;
    internal readonly Item Item;

    private readonly ObservableCollection<string> _tags;
    private readonly Lazy<ObservableCollection<ItemViewModel>> _subItems;
    private readonly ObservableCollection<FieldViewModel> _fields;

    public ItemViewModel(
        IItemsRepository itemsRepository,
        IItemViewModelOwner? owner = null,
        Item? item = null)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _owner = owner;

        if (item == null)
        {
            item = new Item(_owner?.Owner?.Item);
            _itemsRepository.SaveItem(item);
        }

        Item = item;

        _tags = new ObservableCollection<string>(Item.Tags);
        _tags.CollectionChanged += OnTagsCollectionChanged;

        var fieldViewModelCreator = new FieldViewModelConstructor();
        _fields = new ObservableCollection<FieldViewModel>(Item.Fields.Select(fieldViewModelCreator.Create));
        _fields.CollectionChanged += OnFieldsCollectionChanged;

        _subItems = new Lazy<ObservableCollection<ItemViewModel>>(() =>
        {
            var subItems = _itemsRepository.GetChildItems(Item);

            var collection = new ObservableCollection<ItemViewModel>(
                subItems.Select(i => new ItemViewModel(_itemsRepository, this, i))
            );

            collection.CollectionChanged += OnSubItemsCollectionChanged;
            return collection;
        });

        DeleteCommand = new DelegateCommand(OnDelete, CanDelete);
        MoveToCommand = new DelegateCommand<IItemViewModelOwner>(OnMoveTo, CanMoveTo);
    }

    private void OnMoveTo(IItemViewModelOwner newOwner)
    {
        _owner!.SubItems.Remove(this);
        newOwner.SubItems.Add(this);
        _owner = newOwner;
        Item.MoveTo(newOwner.Owner?.Item);
        SaveChanges();
    }

    private bool CanMoveTo(IItemViewModelOwner newOwner) => _owner != null && newOwner != null;

    private void OnDelete()
    {
        _owner!.SubItems.Remove(this);
        _itemsRepository.DeleteItem(Item);
        _owner = null;
    }

    private bool CanDelete() => _owner != null;

    private void OnFieldsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    Item.Fields.InsertRange(
                        e.NewStartingIndex, 
                        e.NewItems
                            .Cast<FieldViewModel>()
                            .Select(fvm => fvm.Field)
                            .ToArray());
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (FieldViewModel field in e.OldItems)
                    {
                        Item.Fields.Remove(field.Field);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Move:
                var movingField = Item.Fields[e.OldStartingIndex];
                Item.Fields.RemoveAt(e.OldStartingIndex);
                Item.Fields.Insert(e.NewStartingIndex, movingField);
                break;
        }
        SaveChanges();
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
                        vmItem._owner?.SubItems.Remove(vmItem);
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
        SaveChanges();
    }

    public string Title
    {
        get => Item.Title;
        set
        {
            if (Item.Title != value)
            {
                Item.Title = value;
                SaveChanges();
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
                SaveChanges();
                RaisePropertyChanged();
            }
        }
    }

    public ObservableCollection<string> Tags => _tags;

    public ObservableCollection<FieldViewModel> Fields => _fields;

    public ItemViewModel Owner => this;

    public ObservableCollection<ItemViewModel> SubItems => _subItems.Value;

    public DelegateCommand DeleteCommand { get; }

    public DelegateCommand<IItemViewModelOwner> MoveToCommand { get; }

    private void SaveChanges()
    {
        _itemsRepository.SaveItem(Item);
    }

    public override int GetHashCode()
    {
        return Item.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var anotherViewModel = obj as ItemViewModel;

        if (anotherViewModel == null) return false;

        return Item.Equals(anotherViewModel.Item);
    }
}