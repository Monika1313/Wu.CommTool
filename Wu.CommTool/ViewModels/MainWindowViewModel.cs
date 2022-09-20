using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Wu.CommTool.Common;
using Wu.CommTool.Extensions;
using Wu.CommTool.Models;
using Wu.CommTool.Views;

namespace Wu.CommTool.ViewModels
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

        /// <summary>
        /// 是否最大化
        /// </summary>
        public bool IsMaximized { get => _IsMaximized; set => SetProperty(ref _IsMaximized, value); }
        private bool _IsMaximized = false;
        #endregion

        #region 命令
        /// <summary>
        /// definity
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }

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

       

        public void Configure()
        {
            this.regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(App.AppConfig.DefaultView);//导航至页面
            //this.regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(nameof(MqttServerView));//导航至页面
        }

        /// <summary>
        /// 创建主菜单
        /// </summary>
        void CreateMenuBar()
        {
            MenuBars = new ObservableCollection<MenuBar>();
            MenuBars.Add(new MenuBar() { Icon = "Bug", Title = "ModbusRtu", NameSpace = nameof(ModbusRtuView) });
            //MenuBars.Add(new MenuBar() { Icon = "LadyBug", Title = "Mqtt", NameSpace = nameof(MqttView) });
            MenuBars.Add(new MenuBar() { Icon = "Clyde", Title = "MqttServer", NameSpace = nameof(MqttServerView) });
            MenuBars.Add(new MenuBar() { Icon = "LadyBug", Title = "MqttClient", NameSpace = nameof(MqttClientView) });
            MenuBars.Add(new MenuBar() { Icon = "Clyde", Title = "关于", NameSpace = nameof(AboutView) });
        }

        /// <summary>
        /// 窗口导航
        /// </summary>
        /// <param name="obj"></param>
        private void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
                return;
            try
            {
                App.AppConfig.DefaultView = obj.NameSpace;
            }
            catch { }
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.NameSpace, back =>
            {
                journal = back.Context.NavigationService.Journal;
            });
        }
    }
}
