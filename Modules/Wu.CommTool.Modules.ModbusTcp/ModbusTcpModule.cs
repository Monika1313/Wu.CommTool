using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Modules.ModbusTcp.Views;

namespace Wu.CommTool.Modules.ModbusTcp
{
    public class ModbusTcpModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ModbusTcpView>();   //ModbusTcp主界面 
        }
    }
}