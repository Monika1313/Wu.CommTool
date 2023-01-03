using Prism.Commands;
using Prism.Services.Dialogs;

namespace Wu.CommTool.Common
{
    public interface IDialogHostAware
    {
        /// <summary>
        /// 窗口名称
        /// </summary>
        public string DialogHostName { get; set; }

        /// <summary>
        /// 打开过程中执行
        /// </summary>
        /// <param name="parameters">接收参数</param>
        void OnDialogOpend(IDialogParameters parameters);

        /// <summary>
        /// 确定
        /// </summary>
        DelegateCommand SaveCommand { get; set; }

        /// <summary>
        /// 取消
        /// </summary>
        DelegateCommand CancelCommand { get; set; }
    }
}
