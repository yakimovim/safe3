using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels;

public partial class CreateOrOpenStorageViewModel : ObservableViewModelBase
{
    [RelayCommand]
    private void Create()
    {
        var openDialog = new OpenFileDialog
        {
            AddExtension = true,
            DefaultExt = ".safe",
            CheckFileExists = false,
            Filter = $"{Application.Current.Resources["StorageFileFilter"]}|*.safe"
        };
        if (openDialog.ShowDialog() == true)
        {
            var parameters = new NavigationParameters
                {
                    { "StoragePath", openDialog.FileName }
                };
            RegionManager.RequestNavigationToMainContent("CreateStorage", parameters);
        }
    }

    [RelayCommand]
    private void Open()
    {
        var openDialog = new OpenFileDialog
        {
            DefaultExt = ".safe",
            CheckFileExists = true,
            Filter = $"{Application.Current.Resources["StorageFileFilter"]}|*.safe"
        };
        if (openDialog.ShowDialog() == true)
        {
            var parameters = new NavigationParameters
                {
                    { "StoragePath", openDialog.FileName }
                };
            RegionManager.RequestNavigationToMainContent("LoginToStorage", parameters);
        }
    }
}

