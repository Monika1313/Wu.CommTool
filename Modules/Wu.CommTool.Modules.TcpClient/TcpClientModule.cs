using Wu.CommTool.Modules.TcpClient.ViewModels;
using Wu.CommTool.Modules.TcpClient.Views;

namespace Wu.CommTool.Modules.TcpClient
{
    public class TcpClientModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TcpClientView, TcpClientViewModel>();
        }
    }
}