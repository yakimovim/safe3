﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
    }
}
