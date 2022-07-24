using Wu.CommTool.Events;
using Wu.CommTool.Extensions;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.ViewModels
{
    /// <summary>
    /// 窗口导航类
    /// </summary>
    public class NavigationViewModel : BindableBase,INavigationAware
    {
        private readonly IContainerProvider provider;
        public readonly IEventAggregator aggregator;//事件聚合器
        public NavigationViewModel() { }

        public NavigationViewModel(IContainerProvider provider)
        {
            this.provider = provider;
            this.aggregator = provider.Resolve<IEventAggregator>();
        }

        /// <summary>
        /// 是否重用旧窗口
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }

        /// <summary>
        /// 打开等待窗口
        /// </summary>
        /// <param name="IsOpen"></param>
        public void UpdateLoading(bool IsOpen)
        {
            aggregator.UpdateLoading(new UpdateModel()
            {
                IsOpen = IsOpen
            });
        }

        /// <summary>
        /// 发送API访问异常消息
        /// </summary>
        public void SendApiError()
        {
            aggregator.SendMessage($"Web Api 访问异常!!!", "Main");
        }
    }
}
