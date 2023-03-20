using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Wu.CommTool.Core;
using Wu.CommTool.Core.Mvvm;
using Wu.CommTool.Modules.ConvertTools.Views;

namespace Wu.CommTool.Modules.ConvertTools.ViewModels
{
    public class ConvertToolsViewModel : RegionViewModelBase
    {
        private readonly IRegionManager regionManager;

        public ConvertToolsViewModel(IRegionManager regionManager) : base(regionManager)
        {
            this.regionManager = regionManager;
            ExecuteCommand = new DelegateCommand<string>(Execute);
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "Test1": Test1(); break;
                default:
                    break;
            }
        }

        private void Test1()
        {
            this.regionManager.RequestNavigate(RegionNames.ConvertToolsViewRegionName, nameof(TimestampConvertView), back =>
            {
                if (back.Error != null)
                {

                }
            });
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //this.regionManager.RequestNavigate(RegionNames.ConvertToolsViewRegionName, nameof(TimestampConvertView), back =>
            //{
            //    if (back.Error != null)
            //    {

            //    }
            //});
        }


        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }
    }
}
