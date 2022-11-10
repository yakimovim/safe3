using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Search;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Events;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class MoveItemDialogViewModel : ObservableObject, IDialogAware
{
    private const int MaxItems = 10;

    private readonly IItemsRepository _itemsRepository;
    private readonly IIconsRepository _iconsRepository;
    private readonly IEventAggregator _eventAggregator;
    private readonly Subject<string> _searchTextChanged = new();

    private Item _item = null!;

    public MoveItemDialogViewModel(
        IItemsRepository itemsRepository,
        IIconsRepository iconsRepository,
        IEventAggregator eventAggregator)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        _searchTextChanged.Throttle(TimeSpan.FromSeconds(2))
            .SubscribeOn(new DispatcherSynchronizationContext())
            .Subscribe(MakeSearch);
    }

    private void MakeSearch(string searchText)
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
        if(SelectedItem == null) return false;

        if (_itemsRepository.IsChildOrSelfOf(SelectedItem.Item, _item)) return false;

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanMove))]
    private void Move()
    {
        _item.MoveTo(SelectedItem!.Item);
        _itemsRepository.SaveItem(_item);

        _eventAggregator.GetEvent<ItemMoved>().Publish((_item, SelectedItem.Item));

        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
    }

    [RelayCommand]
    private void MoveToRoot()
    {
        _item.MoveTo(null);
        _itemsRepository.SaveItem(_item);

        _eventAggregator.GetEvent<ItemMoved>().Publish((_item, null));

        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        _item = parameters.GetValue<Item>("Item");

        _searchText = string.Empty;
        MakeSearch(_searchText);
    }

    public string Title => (string) Application.Current.FindResource("MoveItemDialogHeader");

    public event Action<IDialogResult>? RequestClose;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        _searchTextChanged.OnNext(value);
    }

    [ObservableProperty]
    private ObservableCollection<ItemListViewModel> _items = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MoveCommand))]
    private ItemListViewModel? _selectedItem;
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