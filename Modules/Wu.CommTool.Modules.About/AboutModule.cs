namespace Wu.CommTool.Modules.About;

public class AboutModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {

    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<AboutView, AboutViewModel>();         //注册页面
        containerRegistry.RegisterForNavigation<SupportView, SupportViewModel>();         //注册页面
    }
}