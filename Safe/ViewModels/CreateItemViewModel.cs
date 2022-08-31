using System;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class CreateItemViewModel : BindableBase, INavigationAware
{
    private readonly IRegionManager _regionManager;
    private readonly IEventAggregator _eventAggregator;

    public CreateItemViewModel(
        IRegionManager regionManager,
        IEventAggregator eventAggregator
        )
    {
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        CancelCommand = new DelegateCommand(OnCancel);
        CreateItemCommand = new DelegateCommand(OnCreate, CanCreate)
            .ObservesProperty(() => Title);
    }

    private void OnCreate()
    {
        var item = new Item(_parent)
        {
            Title = Title,
            Description = Description
        };

        _eventAggregator.GetEvent<NewItemCreated>().Publish((item, _parent));
    }

    private bool CanCreate() => !string.IsNullOrWhiteSpace(Title);

    private void OnCancel()
    {
    }

    private string _title;
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    private string _description;
    
    private Item? _parent;

    public string Description
    {
        get { return _description; }
        set { SetProperty(ref _description, value); }
    }

    public DelegateCommand CreateItemCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _parent = navigationContext.Parameters.GetValue<Item?>("Parent");
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}