using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels;

public class CreateItemViewModel : ViewModelBase
{
    private readonly IIconsRepository _iconsRepository;
    private Item? _parent;

    public CreateItemViewModel(IIconsRepository iconsRepository)
    {
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        CancelCommand = new DelegateCommand(OnCancel);
        CreateItemCommand = new DelegateCommand(OnCreate, CanCreate)
            .ObservesProperty(() => Title);
        AddTextFieldCommand = new DelegateCommand(OnAddTextField);
        AddPasswordFieldCommand = new DelegateCommand(OnAddPasswordField);
        ClearIconCommand = new DelegateCommand(OnClearIcon);
        SelectIconCommand = new DelegateCommand(OnSelectIcon);
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

    private void OnAddTextField()
    {
        var field = new TextFieldViewModel();
        field.Deleted += OnFieldDeleted;
        Fields.Add(field);
    }

    private void OnAddPasswordField()
    {
        var field = new PasswordFieldViewModel();
        field.Deleted += OnFieldDeleted;
        Fields.Add(field);
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

    private string _title = string.Empty;
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    private string _description = string.Empty;

    public string Description
    {
        get { return _description; }
        set { SetProperty(ref _description, value); }
    }

    private string _tags = string.Empty;

    public string Tags
    {
        get { return _tags; }
        set { SetProperty(ref _tags, value); }
    }

    private string? _iconId;
    private ImageSource _icon = Icons.DefaultItemIcon;
    public ImageSource Icon
    {
        get { return _icon; }
        set { SetProperty(ref _icon, value); }
    }

    public DelegateCommand CreateItemCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand AddTextFieldCommand { get; }
    public DelegateCommand AddPasswordFieldCommand { get; }
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

    public ObservableCollection<FieldViewModel> Fields { get; } = new ObservableCollection<FieldViewModel>();
}