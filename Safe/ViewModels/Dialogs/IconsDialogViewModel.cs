using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using Microsoft.Win32;
using Prism.Events;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class IconsDialogViewModel : ObservableObject, IDialogAware
{
    private readonly IIconsRepository _iconsRepository;
    private readonly IDialogService _dialogService;
    private readonly IEventAggregator _eventAggregator;

    public IconsDialogViewModel(
        IIconsRepository iconsRepository,
        IDialogService dialogService,
        IEventAggregator eventAggregator
        )
    {
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

        Icons = new ObservableCollection<IconViewModel>(
            _iconsRepository
                .GetAllIcons()
                .Select(i => new IconViewModel(_iconsRepository, i))
        );
    }

    private bool CanDeleteIcon() => SelectedIcon != null;

    [RelayCommand(CanExecute = nameof(CanDeleteIcon))]
    private void DeleteIcon()
    {
        _dialogService.ShowConfirmationDialog((string) Application.Current.Resources["DeleteIconConfirmation"], res =>
        {
            if (res == ButtonResult.Yes)
            {
                var icon = SelectedIcon!;

                _iconsRepository.DeleteIcon(icon.Id);

                Icons.Remove(icon);

                _eventAggregator.GetEvent<IconRemoved>().Publish(icon.Id);
            }
        });
    }

    [RelayCommand]
    private void AddNewIcon()
    {
        var openDialog = new OpenFileDialog
        {
            CheckFileExists = true,
            CheckPathExists = true,
            Filter = $"{Application.Current.Resources["SvgFileFilter"]}|*.svg",
            DefaultExt = ".svg"
        };

        if (openDialog.ShowDialog() == true)
        {
            var fileStream = openDialog.OpenFile();

            var iconId = _iconsRepository.CreateNewIcon(fileStream);

            var icon = _iconsRepository.GetIconById(iconId);
            
            Icons.Add(new IconViewModel(_iconsRepository, icon!));
        }
    }

    private bool CanSelectIcon() => SelectedIcon != null;

    [RelayCommand(CanExecute = nameof(CanSelectIcon))]
    private void SelectIcon()
    {
        var p = new DialogParameters { { "IconId", SelectedIcon!.Id } };

        RequestClose?.Invoke(new DialogResult(ButtonResult.OK, p));
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    { }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    public void OnDialogOpened(IDialogParameters parameters)
    { }

    [ObservableProperty]
    private ObservableCollection<IconViewModel> _icons = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteIconCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectIconCommand))]
    private IconViewModel? _selectedIcon;

    public string Title { get; } = (string) Application.Current.Resources["SelectIconDialogTitle"];

    public event Action<IDialogResult>? RequestClose;
}

public class IconViewModel
{
    public IconViewModel(IIconsRepository iconsRepository, Icon icon)
    {
        Id = icon.Id;
        Icon = iconsRepository.GetIcon(icon.Id);
    }

    public string Id { get; }

    public ImageSource Icon { get; }
}