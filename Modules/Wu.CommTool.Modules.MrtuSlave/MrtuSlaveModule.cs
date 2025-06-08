namespace Wu.CommTool.Modules.MrtuSlave;

public class MrtuSlaveModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MrtuSlaveView, MrtuSlaveViewModel>("MrtuSlaveView");
    }
}