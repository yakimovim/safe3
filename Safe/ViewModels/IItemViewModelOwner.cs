using System.Collections.ObjectModel;

namespace EdlinSoftware.Safe.ViewModels;

public interface IItemViewModelOwner
{
    ItemViewModel? Owner { get; }

    ObservableCollection<ItemViewModel> SubItems { get; }
}