using Prism.Ioc;
using System.Windows;
using Wu.ComTool.Common;
using Wu.ComTool.ViewModels;
using Wu.ComTool.Views;

namespace Wu.ComTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //注册页面
            containerRegistry.RegisterForNavigation<ComToolView, ComToolViewModel>();//首页页面
        }

        /// <summary>
        /// 初始化完成
        /// </summary>
        protected override void OnInitialized()
        {
            //初始化窗口
            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
                service.Congifure();
            base.OnInitialized();   
        }
    }
}
