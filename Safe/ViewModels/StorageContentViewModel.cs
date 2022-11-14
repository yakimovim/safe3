using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public partial class StorageContentViewModel : ObservableViewModelBase
{
    private readonly Subject<string> _searchTextChanged = new();

    public StorageContentViewModel()
    {
        _searchTextChanged.Throttle(TimeSpan.FromSeconds(2))
            .SubscribeOn(new DispatcherSynchronizationContext())
            .Subscribe(OnSearchForText);
    }

    private void OnSearchForText(string searchText)
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

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        _searchTextChanged.OnNext(value);
    }
}
