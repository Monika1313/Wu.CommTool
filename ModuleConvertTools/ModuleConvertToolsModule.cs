using ModuleConvertTools.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleConvertTools
{
    public class ModuleConvertToolsModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ModuleConvertToolsModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RequestNavigate("", nameof(TimestampConvertView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}