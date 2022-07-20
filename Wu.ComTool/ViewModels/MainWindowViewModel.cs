using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Wu.ComTool.Common;
using Wu.ComTool.Extensions;

namespace Wu.ComTool.ViewModels
{
    public class MainWindowViewModel : BindableBase, IConfigureService
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        private string _Title = "串口调试工具";
        private readonly IRegionManager regionManager;

        public MainWindowViewModel()
        {

        }
        public MainWindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ExecuteCommand = new DelegateCommand<string>(Execute);
        }

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



    }
}
