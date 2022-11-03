using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Threading;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Search;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public class MoveItemDialogViewModel : ViewModelBase, IDialogAware
{
    private const int MaxItems = 10;

    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;
    private readonly Subject<string> _searchTextChanged = new();

    private Item _item = null!;

    public MoveItemDialogViewModel(
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        _searchTextChanged.Throttle(TimeSpan.FromSeconds(2))
            .SubscribeOn(new DispatcherSynchronizationContext())
            .Subscribe(OnSearchTextChanged);

        MoveCommand = new DelegateCommand(OnMove, CanMove)
            .ObservesProperty(() => SelectedItem);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    private void OnSearchTextChanged(string searchText)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var itemsQuery = string.IsNullOrWhiteSpace(searchText)
                ? _itemsRepository.GetChildItems(null)
                : _itemsRepository.Find(
                    new SearchModelBuilder()
                        .GetSearchStringElements(searchText)
                );

            var items = itemsQuery
                .Take(MaxItems)
                .Select(i => new ItemListViewModel(_itemsRepository, _iconsRepository, i))
                .ToArray();

            Items = new ObservableCollection<ItemListViewModel>(items);
        });
    }

    private bool CanMove()
    {
        return SelectedItem != null;
    }

    private void OnMove()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
    }

    private void OnCancel()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        _item = parameters.GetValue<Item>("Item");

        _searchText = string.Empty;
        OnSearchTextChanged(_searchText);
    }

    public string Title => (string) Application.Current.FindResource("MoveItemDialogHeader");

    public event Action<IDialogResult>? RequestClose;

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

    private ObservableCollection<ItemListViewModel> _items = new();
    public ObservableCollection<ItemListViewModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    private ItemListViewModel? _selectedItem;
    public ItemListViewModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public DelegateCommand MoveCommand { get; }

    public DelegateCommand CancelCommand { get; }
}


public static class MoveItemDialogExtensions
{
    public static void ShowMoveItemDialog(this IDialogService dialogService,
        Item item)
    {
        var p = new DialogParameters { { "Item", item } };

        dialogService.ShowDialog(nameof(MoveItemDialog), p, _ => {});
    }
}