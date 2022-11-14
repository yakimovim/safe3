using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using EdlinSoftware.Safe.ViewModels.Dialogs;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels;

internal partial class EditItemViewModel : ItemViewModelBase, IRecipient<PropertyChangedMessage<string>>
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;

    private Item _item = null!;

    [ObservableProperty]
    private string? _iconId;

    public EditItemViewModel(
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        Fields.CollectionChanged += FieldsCollectionChanged;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    protected override void SubscribeToEvents()
    {
        EventAggregator.GetEvent<IconRemoved>()
            .Subscribe(OnIconRemoved);
    }

    private void OnIconRemoved(string iconId)
    {
        if (IconId == iconId)
        {
            IconId = null;
            Icon = Icons.DefaultItemIcon;
        }
    }

    private void FieldsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems != null)
                    {
                        foreach (var field in e.NewItems.OfType<FieldViewModel>())
                        {
                            field.PropertyChanged += FieldPropertyChanged;
                        }
                    }
                    break;
                }
            case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems != null)
                    {
                        foreach (var field in e.OldItems.OfType<FieldViewModel>())
                        {
                            field.PropertyChanged -= FieldPropertyChanged;
                        }
                    }
                    break;
                }
        }
    }

    private void FieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveChangesCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void AddFields()
    {
        DialogService.ShowAddFieldsDialog(Fields, OnFieldDeleted);
    }

    [RelayCommand(CanExecute = nameof(CanSaveChanges))]
    private void SaveChanges()
    {
        _item.Title = Title;
        _item.Description = Description;

        _item.Fields.Clear();
        _item.Fields.AddRange(Fields.Select(f => f.Field));

        _item.Tags.Clear();
        if (!string.IsNullOrEmpty(Tags))
        {
            _item.Tags.AddRange(Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()));
        }

        _item.IconId = IconId;

        _itemsRepository.SaveItem(_item);

        var parameters = new NavigationParameters { { "Item", _item } };
        RegionManager.RequestNavigationToDetails("ItemDetails", parameters);

        EventAggregator.GetEvent<ItemChanged>().Publish(_item);
    }

    private bool CanSaveChanges()
    {
        if (Fields.Count > 0 && Fields.Any(f => f.HasErrors))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(Title);
    }

    [RelayCommand]
    private void Cancel()
    {
        var parameters = new NavigationParameters { { "Item", _item } };
        RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _item = navigationContext.Parameters.GetValue<Item>("Item");

        Title = _item.Title;
        Description = _item.Description ?? string.Empty;
        Tags = string.Join(", ", _item.Tags);
        Icon = _iconsRepository.GetIcon(_item.IconId);
        IconId = _item.IconId;

        Fields.Clear();

        var fieldConstructor = new FieldViewModelConstructor();
        Fields.AddRange(_item.Fields.Select(f =>
        {
            var field = fieldConstructor.Create(f);
            field.ContainingCollection = Fields;
            field.Deleted += OnFieldDeleted;
            return field;
        }));
    }

    public override void OnNavigatedFrom(NavigationContext navigationContext)
    {
        foreach (var field in Fields)
        {
            field.Deleted -= OnFieldDeleted;
            field.PropertyChanged -= FieldPropertyChanged;
        }
    }

    private void OnFieldDeleted(object? sender, FieldViewModel field)
    {
        DialogService.ShowConfirmationDialog((string)Application.Current.Resources["DeleteFieldConfirmation"], res =>
        {
            if (res == ButtonResult.Yes)
            {
                field.Deleted -= OnFieldDeleted;
                Fields.Remove(field);
                SaveChangesCommand.NotifyCanExecuteChanged();
            }
        });
    }

    public void Receive(PropertyChangedMessage<string> message)
    {
        if(!ReferenceEquals(message.Sender, this))
            return;

        switch (message.PropertyName)
        {
            case nameof(Title):
            {
                SaveChangesCommand.NotifyCanExecuteChanged();
                break;
            }
        }
    }
}
