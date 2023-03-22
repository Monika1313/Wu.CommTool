using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Wu.CommTool.Modules.ConvertTools.Views;
using Wu.CommTool.Core;
using Wu.CommTool.Modules.ConvertTools.ViewModels;

namespace Wu.CommTool.Modules.ConvertTools
{
    public class ConvertToolsModule : IModule
    {
        private readonly IRegionManager regionManager;

        public ConvertToolsModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        /// <summary>
        /// 模块初始化完成事件
        /// </summary>
        /// <param name="containerProvider"></param>
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ConvertToolsView, ConvertToolsViewModel>();         //注册转换工具主界面
            containerRegistry.RegisterForNavigation<TimestampConvertView, TimestampConvertViewModel>(); //注册时间戳转换页面
            containerRegistry.RegisterForNavigation<ValueConvertView, ValueConvertViewModel>();         //注册值转换页面
        }
    }
}