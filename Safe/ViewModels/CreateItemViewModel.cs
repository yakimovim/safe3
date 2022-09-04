using System;
using System.Collections.ObjectModel;
using System.Linq;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class CreateItemViewModel : ViewModelBase
{
    private Item? _parent;

    public CreateItemViewModel()
    {
        CancelCommand = new DelegateCommand(OnCancel);
        CreateItemCommand = new DelegateCommand(OnCreate, CanCreate)
            .ObservesProperty(() => Title);
        AddTextFieldCommand = new DelegateCommand(OnAddTextField);
        AddPasswordFieldCommand = new DelegateCommand(OnAddPasswordField);
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

    public DelegateCommand CreateItemCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public DelegateCommand AddTextFieldCommand { get; }
    public DelegateCommand AddPasswordFieldCommand { get; }

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