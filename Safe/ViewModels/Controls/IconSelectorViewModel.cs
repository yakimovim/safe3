using System;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Images;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Controls;

public partial class IconSelectorViewModel : ObservableViewModelBase
{
    private readonly IIconsRepository _iconsRepository;

    public IconSelectorViewModel(IIconsRepository iconsRepository)
    {
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));
    }

    public override bool IsNavigationTarget(NavigationContext navigationContext) => false;

    [ObservableProperty]
    private string? _iconId;

    partial void OnIconIdChanged(string? value)
    {
        Icon = _iconsRepository.GetIcon(value);
    }

    [ObservableProperty]
    private ImageSource _icon = Icons.DefaultItemIcon;

    [RelayCommand]
    private void SelectIcon()
    {
        DialogService.ShowDialog("IconsDialog", new DialogParameters(), result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                var iconId = result.Parameters.GetValue<string>("IconId");

                IconId = iconId;
            }
        });
    }

    [RelayCommand]
    private void ClearIcon()
    {
        IconId = null;
    }
}