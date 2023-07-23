using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Modules.MqttClient.ViewModels;
using Wu.CommTool.Modules.MqttClient.Views;

namespace Wu.CommTool.Modules.MqttClient
{
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
}