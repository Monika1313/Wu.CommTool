namespace Wu.CommTool.Modules.CryptoTool;

public class CryptoToolModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<CryptoToolView, CryptoToolViewModel>();
        containerRegistry.RegisterForNavigation<Base64ToolView, Base64ToolViewModel>();
        containerRegistry.RegisterForNavigation<AesToolView, AesToolViewModel>();
        containerRegistry.RegisterForNavigation<Sm4ToolView, Sm4ToolViewModel>();
    }
}
