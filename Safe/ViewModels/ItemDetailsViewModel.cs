using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemDetailsViewModel : ViewModelBase
{
    private readonly IItemsRepository _itemsRepository;
    private Item? _item;

    public ItemDetailsViewModel(IItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
    }

    private string _title;
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    private string _description;

    public string Description
    {
        get { return _description; }
        set { SetProperty(ref _description, value); }
    }

    private string _tags;

    public string Tags
    {
        get { return _tags; }
        set { SetProperty(ref _tags, value); }
    }

    public ObservableCollection<FieldViewModel> Fields { get; } = new ObservableCollection<FieldViewModel>();

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _item = navigationContext.Parameters.GetValue<Item?>("Item");

        Title = _item?.Title;
        Description = _item?.Description;
        Tags = string.Join(", ", _item?.Tags ?? new List<string>());

        Fields.Clear();

        if (_item != null)
        {
            var fieldConstructor = new FieldViewModelConstructor();
            Fields.AddRange(_item?.Fields.Select(f =>
            {
                var field = fieldConstructor.Create(f);
                field.PropertyChanged += OnFieldChanged;
                return field;
            }));
        }
    }

    public override void OnNavigatedFrom(NavigationContext navigationContext)
    {
        foreach (var field in Fields)
        {
            field.PropertyChanged -= OnFieldChanged;
        }
    }

    private void OnFieldChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_item != null)
        {
            _itemsRepository.SaveItem(_item);
        }
    }
}