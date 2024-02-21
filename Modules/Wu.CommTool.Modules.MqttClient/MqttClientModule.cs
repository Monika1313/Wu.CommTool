namespace Wu.CommTool.Modules.MqttClient;

public class MqttClientModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MqttClientView, MqttClientViewModel>();
    }
}