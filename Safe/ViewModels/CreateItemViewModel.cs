using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels;

public partial class CreateItemViewModel : ItemViewModelBase
{
    private Item? _parent;

    [ObservableProperty]
    private string? _iconId;

    public CreateItemViewModel()
    {
        CreateItemCommand = new DelegateCommand(OnCreateItem, CanCreateItem)
            .ObservesProperty(() => Title)
            .ObservesProperty(() => Fields);

        Fields.CollectionChanged += FieldsCollectionChanged;
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
        CreateItemCommand.RaiseCanExecuteChanged();
    }

    public DelegateCommand CreateItemCommand { get; }

    [RelayCommand]
    private void AddFields()
    {
        DialogService.ShowAddFieldsDialog(Fields, OnFieldDeleted);
    }

    private void OnFieldDeleted(object? sender, FieldViewModel field)
    {
        DialogService.ShowConfirmationDialog((string) Application.Current.Resources["DeleteFieldConfirmation"], res =>
        {
            if (res == ButtonResult.Yes)
            {
                field.Deleted -= OnFieldDeleted;
                Fields.Remove(field);
                CreateItemCommand.RaiseCanExecuteChanged();
            }
        });
    }

    private void OnCreateItem()
    {
        var item = new Item(_parent)
        {
            Title = Title,
            Description = Description,
            IconId = IconId
        };

        if(!string.IsNullOrEmpty(Tags))
        {
            item.Tags.AddRange(Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()));
        }

        if(Fields.Any())
        {
            item.Fields.AddRange(Fields.Select(f => f.Field));
        }

        EventAggregator.GetEvent<NewItemCreated>().Publish((item, _parent));
    }

    private bool CanCreateItem()
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
        var parameters = new NavigationParameters { { "Item", _parent } };
        RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _parent = navigationContext.Parameters.GetValue<Item?>("Parent");
    }

    public override bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public override void OnNavigatedFrom(NavigationContext navigationContext)
    {
        foreach (var field in Fields)
        {
            field.Deleted -= OnFieldDeleted;
            field.PropertyChanged -= FieldPropertyChanged;
        }
    }
}