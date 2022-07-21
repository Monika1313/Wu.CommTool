using Prism.Ioc;
using System.Windows;
using Wu.Comm.Common;
using Wu.Comm.ViewModels;
using Wu.Comm.Views;

namespace Wu.Comm
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
