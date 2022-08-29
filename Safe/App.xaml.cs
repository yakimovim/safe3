using System.Windows;
using EdlinSoftware.Safe.Services;
using EdlinSoftware.Safe.Views;
using Prism.Ioc;
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

            var connectionProvider = new LiteDbConnectionProvider();
            containerRegistry.RegisterSingleton<Storage.ILiteDbConnectionProvider>(() => connectionProvider);
            containerRegistry.RegisterSingleton<LiteDbConnectionProvider>(() => connectionProvider);
            containerRegistry.Register<Domain.IItemsRepository, Domain.ItemsRepository>();
            containerRegistry.Register<Storage.IItemsRepository, Storage.LiteDbItemsRepository>();

            containerRegistry.RegisterForNavigation<CreateOrOpenStorageView>("CreateOrOpenStorage");
            containerRegistry.RegisterForNavigation<CreateStorageView>("CreateStorage");
            containerRegistry.RegisterForNavigation<LoginToStorageView>("LoginToStorage");
            containerRegistry.RegisterForNavigation<StorageContentView>("StorageContent");
        }
    }
}
