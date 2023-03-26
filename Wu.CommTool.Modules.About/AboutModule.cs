using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Modules.About.ViewModels;
using Wu.CommTool.Modules.About.Views;
//using Wu.CommTool.Modules.About.Views;

namespace Wu.CommTool.Modules.About
{
    public class AboutModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AboutView, AboutViewModel>();         //注册页面
        }
    }
}