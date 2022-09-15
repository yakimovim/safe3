using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
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
        SelectIconCommand = new DelegateCommand(OnSelectIcon, CanSelectIcon)
            .ObservesProperty(() => SelectedIcon);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    private void OnAddNewIcon()
    {
        var openDialog = new OpenFileDialog
        {
            CheckFileExists = true,
            CheckPathExists = true
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

    private ObservableCollection<IconViewModel> _icons;
    public ObservableCollection<IconViewModel> Icons
    {
        get { return _icons; }
        set { SetProperty(ref _icons, value); }
    }

    private IconViewModel? _selectedIcon;
    public IconViewModel? SelectedIcon
    {
        get { return _selectedIcon; }
        set { SetProperty(ref _selectedIcon, value); }
    }

    public string Title { get; } = "Select an icon";

    public event Action<IDialogResult>? RequestClose;

    public DelegateCommand AddNewIconCommand { get; }
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