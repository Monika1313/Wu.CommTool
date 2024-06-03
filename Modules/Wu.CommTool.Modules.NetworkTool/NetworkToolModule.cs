using Prism.Modularity;
using Wu.CommTool.Modules.NetworkTool.ViewModels;
using Wu.CommTool.Modules.NetworkTool.Views;

namespace Wu.CommTool.Modules.NetworkTool;

public class NetworkToolModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<NetworkToolView,NetworkToolViewModel>();
    }
}