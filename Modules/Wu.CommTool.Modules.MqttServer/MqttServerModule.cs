namespace Wu.CommTool.Modules.MqttServer;

public class MqttServerModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MqttServerView, MqttServerViewModel>();   //MqttServerView 
    }
}