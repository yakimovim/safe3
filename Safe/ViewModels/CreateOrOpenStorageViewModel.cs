using System;
using EdlinSoftware.Safe.Services;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class CreateOrOpenStorageViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configurationService;

        public CreateOrOpenStorageViewModel(
            IConfigurationService configurationService
            )
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            CreateCommand = new DelegateCommand(OnCreateStorage);
            OpenCommand = new DelegateCommand(OnOpenStorage);
        }

        private void OnCreateStorage()
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = false
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
            var openDialog = new OpenFileDialog();
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
