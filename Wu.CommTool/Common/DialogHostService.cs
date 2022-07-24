using MaterialDesignThemes.Wpf;
using Prism.Common;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wu.CommTool.Common
{
    /// <summary>
    /// 自定义对话主机服务
    /// </summary>
    public class DialogHostService : DialogService, IDialogHostService
    {
        private readonly IContainerExtension containerExtension;

        public DialogHostService(IContainerExtension containerExtension) : base(containerExtension)
        {
            this.containerExtension = containerExtension;
        }

        public async Task<IDialogResult> ShowDialog(string name, IDialogParameters parameters, string dialogHostName = "Root")
        {
            if (parameters == null)
                parameters = new DialogParameters();

            //从容器中取出弹出窗口的实例
            var content = containerExtension.Resolve<object>(name);

            //验证实例的有效性
            #region 验证实例的有效性
            if (!(content is FrameworkElement dialogContent))
                throw new NullReferenceException("A dialog's content must be a FrameworkElement...");

            //MvvmHelpers.AutowireViewModel(dialogContent);
            if (dialogContent is FrameworkElement view && view.DataContext is null && ViewModelLocator.GetAutoWireViewModel(view) is null)
                ViewModelLocator.SetAutoWireViewModel(view, true);

            if (!(dialogContent.DataContext is IDialogHostAware viewModel))
                throw new NullReferenceException("A dialog's ViewModel must implement the IDialogHostService interface");
            #endregion

            viewModel.DialogHostName = dialogHostName;

            //窗口打开事件
            DialogOpenedEventHandler eventHandler = (sender, args) =>
            {
                if (viewModel is IDialogHostAware aware)
                {
                    aware.OnDialogOpend(parameters);
                }
                //更新窗口值
                args.Session.UpdateContent(content);
            };

            //返回弹窗
            return (IDialogResult) await DialogHost.Show(dialogContent, viewModel.DialogHostName, eventHandler);
        }
    }
}
