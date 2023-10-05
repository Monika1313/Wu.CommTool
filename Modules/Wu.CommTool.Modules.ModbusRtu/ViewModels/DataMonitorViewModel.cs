using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using Wu.CommTool.Modules.ModbusRtu.Models;
using Wu.ViewModels;
using Wu.Wpf.Common;
using Wu.Wpf.Models;

namespace Wu.CommTool.Modules.ModbusRtu.ViewModels
{
    public class DataMonitorViewModel : NavigationViewModel, IDialogHostAware
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        public string DialogHostName { get; set; }
        #endregion

        public DataMonitorViewModel() { }
        public DataMonitorViewModel(IContainerProvider provider, IDialogHostService dialogHost,ModbusRtuModel modbusRtuModel) : base(provider)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;
            this.ModbusRtuModel = modbusRtuModel;

            ExecuteCommand = new(Execute);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
            ModburRtuDataWriteCommand = new DelegateCommand<ModbusRtuData>(ModburRtuDataWrite);
        }

      

        #region **************************************** 属性 ****************************************
        /// <summary>
        /// CurrentDto
        /// </summary>
        public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
        private object _CurrentDto = new();

        /// <summary>
        /// ModbusRtuModel
        /// </summary>
        public ModbusRtuModel ModbusRtuModel { get => _ModbusRtuModel; set => SetProperty(ref _ModbusRtuModel, value); }
        private ModbusRtuModel _ModbusRtuModel;

        /// <summary>
        /// OpenDrawers
        /// </summary>
        public OpenDrawers OpenDrawers { get => _OpenDrawers; set => SetProperty(ref _OpenDrawers, value); }
        private OpenDrawers _OpenDrawers =new();
        #endregion


        #region **************************************** 命令 ****************************************
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }

        /// <summary>
        /// ModburRtu数据写入
        /// </summary>
        public DelegateCommand<ModbusRtuData> ModburRtuDataWriteCommand { get; private set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": Search(); break;
                case "Clear": ModbusRtuModel.MessageClear(); break;
                case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;
                case "OpenRightDrawer": OpenDrawers.RightDrawer = true; break;
                case "OpenDialogView": OpenDialogView(); break;

                case "OpenAutoRead": ModbusRtuModel.OpenAutoRead(); break;
                case "CloseAutoRead": ModbusRtuModel.CloseAutoRead(); break;

                case "GetComPorts": ModbusRtuModel.GetComPorts(); break;    //查找串口
                case "OpenCom":                                             //打开串口
                    ModbusRtuModel.OpenCom();
                    break;
                case "CloseCom":
                    ModbusRtuModel.CloseCom();                              //关闭串口
                    break;
                case "OperateFilter": OperateFilter(); break;
                default: break;
            }
        }

        private void OperateFilter()
        {
            ModbusRtuModel.OperateFilter();
        }

        /// <summary>
        /// 导航至该页面触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //Search();
        }

        /// <summary>
        /// 打开该弹窗时执行
        /// </summary>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters != null && parameters.ContainsKey("Value"))
            {
                //var oldDto = parameters.GetValue<Dto>("Value");
                //var getResult = await employeeService.GetSinglePersonalStorageAsync(oldDto);
                //if(getResult != null && getResult.Status)
                //{
                //    CurrentDto = getResult.Result;
                //}
            }
        }


        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            if (!DialogHost.IsDialogOpen(DialogHostName))
                return;
            //添加返回的参数
            DialogParameters param = new DialogParameters();
            param.Add("Value", CurrentDto);
            //关闭窗口,并返回参数
            DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel()
        {
            //若窗口处于打开状态则关闭
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.No));
        }

        /// <summary>
        /// 弹窗
        /// </summary>
        private void OpenDialogView()
        {
            try
            {
                DialogParameters param = new()
                {
                    { "Value", CurrentDto }
                };
                //var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        private async void Search()
        {
            try
            {
                UpdateLoading(true);

            }
            catch (Exception ex)
            {
                //aggregator.SendMessage($"{ex.Message}", "Main");
            }
            finally
            {
                UpdateLoading(false);
            }
        }

        private void ModburRtuDataWrite(ModbusRtuData data)
        {
            ModbusRtuModel.ModburRtuDataWrite(data);
        }
        #endregion
    }
}
