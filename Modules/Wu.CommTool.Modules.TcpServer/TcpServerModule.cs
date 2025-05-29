using Wu.CommTool.Modules.TcpServer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Modules.TcpServer.ViewModels;

namespace Wu.CommTool.Modules.TcpServer;

public class TcpServerModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<TcpServerView, TcpServerViewModel>();
    }
}