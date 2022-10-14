using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels;

public class CreateItemViewModel : ItemViewModelBase
{
    private Item? _parent;

    private string? _iconId;
    public string? IconId
    {
        get => _iconId;
        set => SetProperty(ref _iconId, value);
    }

    public CreateItemViewModel()
    {
        CancelCommand = new DelegateCommand(OnCancel);
        CreateItemCommand = new DelegateCommand(OnCreate, CanCreate)
            .ObservesProperty(() => Title)
            .ObservesProperty(() => Fields);
        AddFieldsCommand = new DelegateCommand(OnAddFields);

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

    private void OnAddFields()
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

    private void OnCreate()
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

    private bool CanCreate()
    {
        if (Fields.Count > 0 && Fields.Any(f => f.HasErrors))
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(Title);
    }

    private void OnCancel()
    {
        var parameters = new NavigationParameters { { "Item", _parent } };
        RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
    }

    public DelegateCommand CreateItemCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand AddFieldsCommand { get; }

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