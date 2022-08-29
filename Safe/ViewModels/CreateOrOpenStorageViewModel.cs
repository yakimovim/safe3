using System;
using EdlinSoftware.Safe.Services;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels
{
    internal class CreateOrOpenStorageViewModel
    {
        private readonly IConfigurationService _configurationService;
        private readonly IRegionManager _regionManager;

        public CreateOrOpenStorageViewModel(
            IConfigurationService configurationService,
            IRegionManager regionManager
            )
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));

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
                _regionManager.RequestNavigate("MainContentRegion", "CreateStorage", parameters);
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
                _regionManager.RequestNavigate("MainContentRegion", "LoginToStorage", parameters);
            }
        }

        public DelegateCommand CreateCommand { get; }

        public DelegateCommand OpenCommand { get; }
    }
}
