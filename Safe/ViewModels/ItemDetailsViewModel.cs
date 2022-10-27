using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Images;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemDetailsViewModel : ItemViewModelBase
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;

    private Item? _item;

    public ItemDetailsViewModel(
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        EditItemCommand = new DelegateCommand(OnEditItem);
    }

    private void OnEditItem()
    {
        var parameters = new NavigationParameters
            { { "Item", _item } };
        RegionManager.RequestNavigationToDetails("EditItem", parameters);
    }

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _item = navigationContext.Parameters.GetValue<Item?>("Item");

        Title = _item?.Title ?? string.Empty;
        Description = _item?.Description ?? string.Empty;
        Tags = string.Join(", ", _item?.Tags ?? new List<string>());
        Icon = _iconsRepository.GetIcon(_item?.IconId);

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

    public DelegateCommand EditItemCommand { get; }
}