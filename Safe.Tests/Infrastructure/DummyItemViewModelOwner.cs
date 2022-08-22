using System.Collections.ObjectModel;
using EdlinSoftware.Safe.ViewModels;

namespace EdlinSoftware.Safe.Tests.Infrastructure;

public class DummyItemViewModelOwner : IItemViewModelOwner
{
    public ItemViewModel Owner { get; set; }
    public ObservableCollection<ItemViewModel> SubItems { get; set; } = new();
}