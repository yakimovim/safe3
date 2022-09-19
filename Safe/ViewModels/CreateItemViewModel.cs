using System;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using EdlinSoftware.Safe.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels;

public class CreateItemViewModel : ItemViewModelBase
{
    private readonly IIconsRepository _iconsRepository;

    private Item? _parent;
    private string? _iconId;

    public CreateItemViewModel(IIconsRepository iconsRepository)
    {
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        CancelCommand = new DelegateCommand(OnCancel);
        CreateItemCommand = new DelegateCommand(OnCreate, CanCreate)
            .ObservesProperty(() => Title);
        AddFieldsCommand = new DelegateCommand(OnAddFields);
        ClearIconCommand = new DelegateCommand(OnClearIcon);
        SelectIconCommand = new DelegateCommand(OnSelectIcon);
    }

    private void OnAddFields()
    {
        DialogService.ShowAddFieldsDialog(Fields, OnFieldDeleted);
    }

    private void OnSelectIcon()
    {
        DialogService.ShowDialog("IconsDialog", new DialogParameters(), result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                var iconId = result.Parameters.GetValue<string>("IconId");

                _iconId = iconId;

                Icon = _iconsRepository.GetIcon(_iconId);
            }
        });
    }

    private void OnClearIcon()
    {
        _iconId = null;
        Icon = Icons.DefaultItemIcon;
    }

    private void OnFieldDeleted(object? sender, FieldViewModel field)
    {
        field.Deleted -= OnFieldDeleted;
        Fields.Remove(field);
    }

    private void OnCreate()
    {
        var item = new Item(_parent)
        {
            Title = Title,
            Description = Description,
            IconId = _iconId
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

    private bool CanCreate() => !string.IsNullOrWhiteSpace(Title);

    private void OnCancel()
    {
        var parameters = new NavigationParameters { { "Item", _parent } };
        RegionManager.RequestNavigationToDetails("ItemDetails", parameters);
    }

    public DelegateCommand CreateItemCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand AddFieldsCommand { get; }
    public DelegateCommand ClearIconCommand { get; }
    public DelegateCommand SelectIconCommand { get; }

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
        }
    }
}