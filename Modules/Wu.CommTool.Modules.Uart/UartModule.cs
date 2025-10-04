

namespace Wu.CommTool.Modules.Uart;

public class UartModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<UartView, UartViewModel>();
        containerRegistry.Register<UartModel>();
    }
}