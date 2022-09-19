using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Threading;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public class StorageContentViewModel : ViewModelBase
{
    private readonly Subject<string> _searchTextChanged = new();

    public StorageContentViewModel()
    {
        _searchTextChanged.Throttle(TimeSpan.FromSeconds(2))
            .SubscribeOn(new DispatcherSynchronizationContext())
            .Subscribe(OnSearchTextChanged);
    }

    private void OnSearchTextChanged(string searchText)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if(string.IsNullOrWhiteSpace(searchText))
            {
                RegionManager.RequestNavigationToStorageContent("StorageTree");
            }
            else
            {
                var p = new NavigationParameters
                {
                    { "SearchText", searchText }
                };
                RegionManager.RequestNavigationToStorageContent("StorageList", p);
            }
        });
    }

    private string _searchText = string.Empty;
    
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _searchTextChanged.OnNext(value);
            }
        }
    }
}
