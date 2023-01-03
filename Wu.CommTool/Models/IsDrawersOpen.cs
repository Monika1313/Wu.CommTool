using Prism.Mvvm;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// 页面的抽屉是否打开
    /// </summary>
    public class IsDrawersOpen : BindableBase
    {
        /// <summary>
        /// 右侧抽屉是否打开
        /// </summary>
        public bool IsRightDrawerOpen { get => _IsRightDrawerOpen; set => SetProperty(ref _IsRightDrawerOpen, value); }
        private bool _IsRightDrawerOpen;

        /// <summary>
        /// 左侧抽屉是否打开
        /// </summary>
        public bool IsLeftDrawerOpen { get => _IsLeftDrawerOpen; set => SetProperty(ref _IsLeftDrawerOpen, value); }
        private bool _IsLeftDrawerOpen;

        /// <summary>
        /// 上端抽屉是否打开
        /// </summary>
        public bool IsTopDrawerOpen { get => _IsTopDrawerOpen; set => SetProperty(ref _IsTopDrawerOpen, value); }
        private bool _IsTopDrawerOpen;

        /// <summary>
        /// 底部抽屉是否打开
        /// </summary>
        public bool IsBottomDrawerOpen { get => _IsBottomDrawerOpen; set => SetProperty(ref _IsBottomDrawerOpen, value); }
        private bool _IsBottomDrawerOpen;

    }
}
