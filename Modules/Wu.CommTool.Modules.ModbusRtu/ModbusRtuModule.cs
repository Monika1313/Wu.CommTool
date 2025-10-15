namespace Wu.CommTool.Modules.ModbusRtu;

public class ModbusRtuModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ModbusRtuModel>();
        containerRegistry.RegisterForNavigation<ModbusRtuView, ModbusRtuViewModel>();
        containerRegistry.RegisterForNavigation<CustomFrameView, CustomFrameViewModel>();
        containerRegistry.RegisterForNavigation<SearchDeviceView, SearchDeviceViewModel>();
        containerRegistry.RegisterForNavigation<DataMonitorView, DataMonitorViewModel>();
        containerRegistry.RegisterForNavigation<AutoResponseView, AutoResponseViewModel>();
        containerRegistry.RegisterForNavigation<ModbusRtuAutoResponseDataEditView, ModbusRtuAutoResponseDataEditViewModel>();
        containerRegistry.RegisterForNavigation<AnalyzeFrameView, AnalyzeFrameViewModel>();
        containerRegistry.RegisterForNavigation<EditFrameView, EditFrameViewModel>();
        containerRegistry.RegisterForNavigation<MrtuDeviceMonitorView, MrtuDeviceMonitorViewModel>();
        containerRegistry.RegisterForNavigation<MrtuDataEditView, MrtuDataEditViewModel>();
        containerRegistry.RegisterForNavigation<MrtuDeviceEditView, MrtuDeviceEditViewModel>();
        containerRegistry.RegisterForNavigation<MrtuDeviceManagerLogView, MrtuDeviceManagerLogViewModel>();

        containerRegistry.RegisterForNavigation<MrtuDeviceLogView, MrtuDeviceLogViewModel>();
        containerRegistry.RegisterForNavigation<UartView, UartViewModel>();
        containerRegistry.RegisterForNavigation<EditUartCustomnFrameView, EditUartCustomnFrameViewModel>();


    }
}
