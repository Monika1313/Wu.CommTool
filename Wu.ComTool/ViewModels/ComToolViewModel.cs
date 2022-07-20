using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.IO.Ports;

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
