using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Modules.MqttServer.Views;

namespace Wu.CommTool.Modules.MqttServer
{
    public class MqttServerModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MqttServerView>();   //MqttServerView 
        }
    }
}