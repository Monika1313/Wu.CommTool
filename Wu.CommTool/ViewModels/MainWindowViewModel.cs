using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Wu.Comm.Common;
using Wu.Comm.Extensions;
using Wu.Comm.Models;
using Wu.Comm.Views;

namespace Wu.Comm.ViewModels
{
    public class MainWindowViewModel : BindableBase, IConfigureService
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        private string _Title = "串口调试工具";
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;

        public MainWindowViewModel()
        {

        }
        public MainWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            CreateMenuBar();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);

            GoBackCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoBack)
                    journal.GoBack();
            });
            GoForwarCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoForward)
                    journal.GoForward();
            });
        }

        #region 属性
        /// <summary>
        /// 主菜单
        /// </summary>
        public ObservableCollection<MenuBar> MenuBars { get => _MenuBars; set => SetProperty(ref _MenuBars, value); }
        private ObservableCollection<MenuBar> _MenuBars;
        #endregion

        #region 命令
        /// <summary>
        /// 导航命令
        /// </summary>
        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }

        /// <summary>
        /// 导航返回
        /// </summary>
        public DelegateCommand GoBackCommand { get; private set; }

        /// <summary>
        /// 导航前进
        /// </summary>
        public DelegateCommand GoForwarCommand { get; private set; }

        #endregion

        private void Execute(string obj)
        {
            switch (obj)
            {
                default:
                    break;
            }
        }

        /// <summary>
        /// definity
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }


        public void Congifure()
        {
            this.regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ComToolView");//导航至页面
        }


        /// <summary>
        /// 创建主菜单
        /// </summary>
        void CreateMenuBar()
        {
            MenuBars = new ObservableCollection<MenuBar>();
            MenuBars.Add(new MenuBar() { Icon = "TransitConnectionVariant", Title = "Modbus Rtu 调试", NameSpace = nameof(ComToolView) });
        }

        /// <summary>
        /// 窗口导航
        /// </summary>
        /// <param name="obj"></param>
        private void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
                return;
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.NameSpace, back =>
            {
                journal = back.Context.NavigationService.Journal;
            });
        }

    }
}
