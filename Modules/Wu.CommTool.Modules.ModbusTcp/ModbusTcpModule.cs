namespace Wu.CommTool.Modules.ModbusTcp;

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
        containerRegistry.RegisterForNavigation<AnalyzeMtcpFrameView, AnalyzeMtcpFrameViewModel>();//ModbusTcp 解析帧弹窗
        containerRegistry.RegisterForNavigation<MtcpDeviceMonitorView>();
    }
}