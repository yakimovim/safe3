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
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Dialogs;

public partial class IconsDialogViewModel : ObservableDialogBase
{
    private readonly IIconsRepository _iconsRepository;

    public IconsDialogViewModel(
        IIconsRepository iconsRepository
        )
    {
        SetTitleFromResource("SelectIconDialogTitle");

        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

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
        DialogService.ShowConfirmationDialog((string) Application.Current.Resources["DeleteIconConfirmation"], res =>
        {
            if (res == ButtonResult.Yes)
            {
                var icon = SelectedIcon!;

                _iconsRepository.DeleteIcon(icon.Id);

                Icons.Remove(icon);

                EventAggregator.GetEvent<IconRemoved>().Publish(icon.Id);
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

        RequestDialogClose(ButtonResult.OK, p);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestDialogClose(ButtonResult.Cancel);
    }

    [ObservableProperty]
    private ObservableCollection<IconViewModel> _icons = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteIconCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectIconCommand))]
    private IconViewModel? _selectedIcon;
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