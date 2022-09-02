using System.Windows.Controls;
using Prism.Regions;

namespace EdlinSoftware.Safe.ViewModels.Helpers
{
    internal static class RegionManagerExtensions
    {
        public static void RequestNavigationToMainContent(
            this IRegionManager regionManager, 
            string source,
            NavigationParameters? parameters = null)
        {
            regionManager.RequestNavigate("MainContentRegion", source, parameters ?? new NavigationParameters());
        }

        public static void RequestNavigationToDetails(
            this IRegionManager regionManager,
            string source,
            NavigationParameters? parameters = null)
        {
            regionManager.RequestNavigate("DetailsRegion", source, parameters ?? new NavigationParameters());
        }
    }
}
