using System;
using System.Windows.Media;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Images;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace EdlinSoftware.Safe.ViewModels.Controls;

public class IconSelectorViewModel : ViewModelBase
{
    private readonly IIconsRepository _iconsRepository;

    public IconSelectorViewModel(IIconsRepository iconsRepository)
    {
        _iconsRepository = iconsRepository ?? throw new ArgumentNullException(nameof(iconsRepository));

        ClearIconCommand = new DelegateCommand(OnClearIcon);
        SelectIconCommand = new DelegateCommand(OnSelectIcon);
    }

    public override bool IsNavigationTarget(NavigationContext navigationContext) => false;

    private string? _iconId;
    public string? IconId
    {
        get => _iconId;
        set
        {
            if (SetProperty(ref _iconId, value))
            {
                Icon = _iconsRepository.GetIcon(value);
            }
        }
    }

    private ImageSource _icon = Icons.DefaultItemIcon;
    public ImageSource Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public DelegateCommand ClearIconCommand { get; }

    public DelegateCommand SelectIconCommand { get; }

    private void OnSelectIcon()
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

    private void OnClearIcon()
    {
        IconId = null;
    }
}