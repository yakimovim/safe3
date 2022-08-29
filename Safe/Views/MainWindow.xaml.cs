using System.IO;
using System.Windows;
using EdlinSoftware.Safe.Services;
using Prism.Regions;

namespace EdlinSoftware.Safe.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfigurationService _configurationService;
        private readonly IRegionManager _regionManager;

        internal MainWindow(
            IConfigurationService configurationService,
            IRegionManager regionManager)
        {
            _configurationService = configurationService ?? throw new System.ArgumentNullException(nameof(configurationService));
            _regionManager = regionManager ?? throw new System.ArgumentNullException(nameof(regionManager));

            InitializeComponent();

            Loaded += OnLoaded;
            
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var configuration = _configurationService.GetConfiguration();

            if(string.IsNullOrWhiteSpace(configuration.LastOpenedStorage))
            {
                _regionManager.RequestNavigate("MainContentRegion", "CreateOrOpenStorage");
            }
            else if(!File.Exists(configuration.LastOpenedStorage))
            {
                configuration.LastOpenedStorage = null;
                _configurationService.SaveConfiguration(configuration);
                _regionManager.RequestNavigate("MainContentRegion", "CreateOrOpenStorage");
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { "StoragePath", configuration.LastOpenedStorage }
                };
                _regionManager.RequestNavigate("MainContentRegion", "LoginToStorage");
            }
        }
    }
}
