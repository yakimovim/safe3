using EdlinSoftware.Safe.Domain.Model;
using Prism.Mvvm;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class ItemDetailsViewModel : BindableBase, INavigationAware
{
    private Item? _item;

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

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _item = navigationContext.Parameters.GetValue<Item?>("Item");

        Title = _item?.Title;
        Description = _item?.Description;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}