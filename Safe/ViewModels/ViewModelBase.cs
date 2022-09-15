using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Unity;

namespace EdlinSoftware.Safe.ViewModels;

public abstract class ViewModelBase : BindableBase, INavigationAware
{
    [Dependency]
    public IRegionManager RegionManager { get; set; }

    private IEventAggregator _eventAggregator;

    [Dependency]
    public IEventAggregator EventAggregator
    {
        get => _eventAggregator;
        set
        {
            _eventAggregator = value;
            if (_eventAggregator != null)
            {
                SubscribeToEvents();
            }
        }
    }

    protected virtual void SubscribeToEvents() { }

    [Dependency]
    public IDialogService DialogService { get; set; }

    public virtual void OnNavigatedTo(NavigationContext navigationContext) { }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public virtual void OnNavigatedFrom(NavigationContext navigationContext) { }
}