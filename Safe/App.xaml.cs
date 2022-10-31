using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EdlinSoftware.Safe.Services;
using EdlinSoftware.Safe.ViewModels.Dialogs;
using EdlinSoftware.Safe.Views;
using EdlinSoftware.Safe.Views.Dialogs;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;

namespace EdlinSoftware.Safe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IConfigurationService, ConfigurationService>();
            containerRegistry.RegisterSingleton<IStorageService, StorageService>();
            containerRegistry.RegisterSingleton<ExportService>();
            containerRegistry.RegisterSingleton<ILanguagesService, LanguagesService>();
            containerRegistry.Register<IPasswordGenerator, PasswordGenerator>();

            var connectionProvider = new LiteDbConnectionProvider();
            containerRegistry.RegisterSingleton<Storage.ILiteDbConnectionProvider>(() => connectionProvider);
            containerRegistry.RegisterSingleton<LiteDbConnectionProvider>(() => connectionProvider);
            containerRegistry.Register<Domain.IItemsRepository, Domain.ItemsRepository>();
            containerRegistry.Register<Storage.IItemsRepository, Storage.LiteDbItemsRepository>();
            containerRegistry.Register<Domain.IIconsRepository, Domain.IconsRepository>();
            containerRegistry.Register<Storage.IIconsRepository, Storage.LiteDbIconsRepository>();
            containerRegistry.Register<Domain.IStorageInfoRepository, Domain.StorageInfoRepository>();
            containerRegistry.Register<Storage.IStorageInfoRepository, Storage.LiteDbStorageInfoRepository>();

            containerRegistry.RegisterDialog<IconsDialog, IconsDialogViewModel>();
            containerRegistry.RegisterDialog<FieldsDialog, FieldsDialogViewModel>();
            containerRegistry.RegisterDialog<ConfirmationDialog, ConfirmationDialogViewModel>();
            containerRegistry.RegisterDialog<PasswordGenerationDialog, PasswordGenerationDialogViewModel>();

            RegisterViewsForNavigation(containerRegistry);
        }

        private void RegisterViewsForNavigation(IContainerRegistry containerRegistry)
        {
            var assembly = typeof(App).Assembly;

            var viewTypes = assembly.GetTypes()
                .Where(t => t.BaseType == typeof(UserControl));

            foreach (var viewType in viewTypes)
            {
                var name = viewType.Name;
                if (name.EndsWith("View"))
                    name = name.Substring(0, name.Length - "View".Length);

                containerRegistry.RegisterForNavigation(viewType, name);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var configurationService = Container.Resolve<IConfigurationService>();

            var configuration = configurationService.GetConfiguration();

            var languagesService = Container.Resolve<ILanguagesService>();

            languagesService.LoadLanguage(configuration.Language);

            var regionManager = Container.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion<StorageTreeView>("StorageContentRegion");
        }
    }
}
