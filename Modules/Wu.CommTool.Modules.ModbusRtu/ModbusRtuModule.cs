using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Core;
using Wu.CommTool.Modules.ModbusRtu.ViewModels;
using Wu.CommTool.Modules.ModbusRtu.Views;

namespace Wu.CommTool.Modules.ModbusRtu
{
    public class ModbusRtuModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ModbusRtuView, ModbusRtuViewModel>("ModbusRtuViewNew");
            containerRegistry.RegisterForNavigation<CustomFrameView, CustomFrameViewModel>();
        }
    }
}