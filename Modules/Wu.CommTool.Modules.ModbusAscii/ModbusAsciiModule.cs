namespace Wu.CommTool.Modules.ModbusAscii;

public class ModbusAsciiModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<ModbusAsciiView, ModbusAsciiViewModel>();
    }
}