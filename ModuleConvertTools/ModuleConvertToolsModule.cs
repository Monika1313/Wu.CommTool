using ModuleConvertTools.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleConvertTools
{
    public class ModuleConvertToolsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("MainViewRegion", typeof(TimestampConvertView));
            //regionManager.RegisterViewWithRegion("MainViewRegion", typeof(ViewA));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}