using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class CreateOrOpenStorageViewModel : ViewModelBase
    {
        public CreateOrOpenStorageViewModel()
        {
            CreateCommand = new DelegateCommand(OnCreateStorage);
            OpenCommand = new DelegateCommand(OnOpenStorage);
        }

        private void OnCreateStorage()
        {
            var openDialog = new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = ".safe",
                CheckFileExists = false,
                Filter = "Safe storage|*.safe"
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

        private void OnOpenStorage()
        {
            var openDialog = new OpenFileDialog
            {
                DefaultExt = ".safe",
                CheckFileExists = true,
                Filter = "Safe storage|*.safe"
            };
            if(openDialog.ShowDialog() == true)
            {
                var parameters = new NavigationParameters
                {
                    { "StoragePath", openDialog.FileName }
                };
                RegionManager.RequestNavigationToMainContent("LoginToStorage", parameters);
            }
        }

        public DelegateCommand CreateCommand { get; }

        public DelegateCommand OpenCommand { get; }
    }
}
