using System.Windows;
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

        }
    }
}
