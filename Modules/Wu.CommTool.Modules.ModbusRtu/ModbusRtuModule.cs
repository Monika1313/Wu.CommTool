using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Core;
using Wu.CommTool.Modules.ModbusRtu.Models;
using Wu.CommTool.Modules.ModbusRtu.ViewModels;
using Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels;
using Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels.DialogDesignViewModels;
using Wu.CommTool.Modules.ModbusRtu.Views;
using Wu.CommTool.Modules.ModbusRtu.Views.DialogViews;

namespace Wu.CommTool.Modules.ModbusRtu
{
    public class ModbusRtuModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ModbusRtuModel>();

            containerRegistry.RegisterForNavigation<ModbusRtuView, ModbusRtuViewModel>();
            //containerRegistry.RegisterForNavigation<ModbusRtuView, ModbusRtuViewModel>("ModbusRtuViewNew");
            containerRegistry.RegisterForNavigation<CustomFrameView, CustomFrameViewModel>();
            containerRegistry.RegisterForNavigation<SearchDeviceView, SearchDeviceViewModel>();
            containerRegistry.RegisterForNavigation<DataMonitorView, DataMonitorViewModel>();
            containerRegistry.RegisterForNavigation<AutoResponseView, AutoResponseViewModel>();
            containerRegistry.RegisterForNavigation<ModbusRtuAutoResponseDataEditView, ModbusRtuAutoResponseDataEditViewModel>();
        }
    }
}