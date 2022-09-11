using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace EdlinSoftware.Safe.ViewModels;

public class StorageContentViewModel : ViewModelBase
{
    private readonly Subject<string> _searchTextChanged = new();

    public StorageContentViewModel()
    {
        _searchTextChanged.Throttle(TimeSpan.FromSeconds(2)).Subscribe(OnSearchTextChanged);
    }

    private void OnSearchTextChanged(string searchText)
    {
        if(string.IsNullOrWhiteSpace(searchText))
        {
            RegionManager.RequestNavigationToStorageContent("StorageTree");
        }
        else
        {
            Debug.WriteLine(searchText);
        }
    }

    private string _searchText = string.Empty;
    
    public string SearchText
    {
        get { return _searchText; }
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _searchTextChanged.OnNext(value);
            }
        }
    }
}
