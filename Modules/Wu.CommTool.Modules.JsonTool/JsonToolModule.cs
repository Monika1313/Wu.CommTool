using Prism.Ioc;
using Prism.Modularity;
using Wu.CommTool.Modules.JsonTool.Views;
using Wu.CommTool.Modules.JsonTool.ViewModels;


namespace Wu.CommTool.Modules.JsonTool
{
    public class JsonToolModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<JsonDataView, JsonDataViewModel>();
        }
    }
}