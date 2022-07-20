using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.IO.Ports;
using Wu.ComTool.Models;

namespace Wu.ComTool.ViewModels
{
    public class ComToolViewModel : BindableBase
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        #endregion

        public ComToolViewModel() { }
        public ComToolViewModel(IContainerProvider provider)
        {
            this.provider = provider;
            ExecuteCommand = new(Execute);
        }

        #region **************************************** 属性 ****************************************
        /// <summary>
        /// 打开抽屉
        /// </summary>
        public IsDrawersOpen IsDrawersOpen { get => _IsDrawersOpen; set => SetProperty(ref _IsDrawersOpen, value); }
        private IsDrawersOpen _IsDrawersOpen = new();

        /// <summary>
        /// Com口配置
        /// </summary>
        public ComConfig ComConfig { get => _ComConfig; set => SetProperty(ref _ComConfig, value); }
        private ComConfig _ComConfig = new();
        #endregion


        #region **************************************** 命令 ****************************************
        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": GetDataAsync(); break;
                case "Add": break;
                case "OpenCom":
                    var xx = Convert.ToInt32(ComConfig.BaudRate);
                    break;
                case "ConfigCom": IsDrawersOpen.IsLeftDrawerOpen = true; break;
                default:
                    break;
            }
        }


        public async void GetDataAsync()
        {
            try
            {

            }
            catch (global::System.Exception ex)
            {

            }
            finally
            {

            }
        }
        #endregion
    }
}
