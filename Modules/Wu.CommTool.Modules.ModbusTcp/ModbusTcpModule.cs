using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Modules.ModbusTcp.ViewModels;
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
            containerRegistry.RegisterForNavigation<ModbusTcpView, ModbusTcpViewModel>();   //ModbusTcp主界面 
            containerRegistry.RegisterForNavigation<ModbusTcpCustomFrameView, ModbusTcpCustomFrameViewModel>();//ModbusTcp自定义帧主界面 
            containerRegistry.RegisterForNavigation<ModbusTcpMasterView, ModbusTcpMasterViewModel>();//ModbusTcp自定义帧主界面 
        }
    }
}