using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using EdlinSoftware.Safe.Events;
using EdlinSoftware.Safe.Images;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public class IconsDialogViewModel : ViewModelBase, IDialogAware
{
    private readonly IIconsRepository _iconsRepository;

    public IconsDialogViewModel(IIconsRepository iconsRepository)
    {
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        Icons = new ObservableCollection<IconViewModel>(
            _iconsRepository
                .GetAllIcons()
                .Select(i => new IconViewModel(_iconsRepository, i))
        );

        AddNewIconCommand = new DelegateCommand(OnAddNewIcon);
        DeleteIconCommand = new DelegateCommand(OnDeleteIcon, CanDeleteIcon)
            .ObservesProperty(() => SelectedIcon);
        SelectIconCommand = new DelegateCommand(OnSelectIcon, CanSelectIcon)
            .ObservesProperty(() => SelectedIcon);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    private bool CanDeleteIcon() => SelectedIcon != null;

    private void OnDeleteIcon()
    {
        var icon = SelectedIcon!;

        _iconsRepository.DeleteIcon(icon.Id);

        Icons.Remove(icon);

        EventAggregator.GetEvent<IconRemoved>().Publish(icon.Id);
    }

    private void OnAddNewIcon()
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

    private void OnSelectIcon()
    {
        var p = new DialogParameters { { "IconId", SelectedIcon!.Id } };

        RequestClose?.Invoke(new DialogResult(ButtonResult.OK, p));
    }

    private bool CanSelectIcon() => SelectedIcon != null;

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    { }

    private void OnCancel()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    public void OnDialogOpened(IDialogParameters parameters)
    { }

    private ObservableCollection<IconViewModel> _icons = new();
    public ObservableCollection<IconViewModel> Icons
    {
        get => _icons;
        set => SetProperty(ref _icons, value);
    }

    private IconViewModel? _selectedIcon;
    public IconViewModel? SelectedIcon
    {
        get => _selectedIcon;
        set => SetProperty(ref _selectedIcon, value);
    }

    public string Title { get; } = (string) Application.Current.Resources["SelectIconDialogTitle"];

    public event Action<IDialogResult>? RequestClose;

    public DelegateCommand AddNewIconCommand { get; }
    public DelegateCommand DeleteIconCommand { get; }
    public DelegateCommand SelectIconCommand { get; }
    public DelegateCommand CancelCommand { get; }
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