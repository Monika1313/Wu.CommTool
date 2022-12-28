using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Wu.CommTool.Common;
using Wu.CommTool.Models;
using Wu.CommTool.Models.JsonModels;
using Wu.ViewModels;
//using Wu.Wpf.Common;

namespace Wu.CommTool.ViewModels
{
    public class JsonToolViewModel : NavigationViewModel, IDialogHostAware
    {
        #region **************************************** 字段 ****************************************
        private readonly IContainerProvider provider;
        private readonly IDialogHostService dialogHost;
        public string DialogHostName { get; set; }
        #endregion

        public JsonToolViewModel() { }
        public JsonToolViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
        {
            this.provider = provider;
            this.dialogHost = dialogHost;
            ExecuteCommand = new(Execute);
            SaveCommand = new DelegateCommand(Save);
            CancelCommand = new DelegateCommand(Cancel);
        }

        #region **************************************** 属性 ****************************************
        /// <summary>
        /// CurrentDto
        /// </summary>
        public object CurrentDto { get => _CurrentDto; set => SetProperty(ref _CurrentDto, value); }
        private object _CurrentDto = new();

        /// <summary>
        /// Json字符串 转换json对象的原始字符串
        /// </summary>
        public string JsonString { get => _JsonString; set => SetProperty(ref _JsonString, value); }
        private string _JsonString = string.Empty;

        /// <summary>
        /// Json
        /// </summary>
        public ObservableCollection<JsonHeaderLogic> JsonHeaderLogics { get => _JsonHeaderLogics; set => SetProperty(ref _JsonHeaderLogics, value); }
        private ObservableCollection<JsonHeaderLogic> _JsonHeaderLogics = new();
        #endregion


        #region **************************************** 命令 ****************************************
        /// <summary>
        /// 执行命令
        /// </summary>
        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        #endregion


        #region **************************************** 方法 ****************************************
        public void Execute(string obj)
        {
            switch (obj)
            {
                case "Search": break;
                case "Format": Format(); break;//格式化
                case "OpenDialogView": OpenDialogView(); break;
                default: break;
            }
        }


        /// <summary>
        /// 格式化json字符串
        /// </summary>
        private void Format()
        {
            if (string.IsNullOrWhiteSpace(JsonString))
                return;
            try
            {
                //json字符串转对象
                //var xx = JsonConvert.DeserializeObject<object>(JsonString);
                //var xz = JsonConvert.DeserializeObject<JToken>(JsonString);

                var jobj = JObject.Parse(JsonString);
                var li = jobj.Children().Select(c=> JsonHeaderLogic.FromJToken(c)).ToList();
                JsonHeaderLogics.Clear();
                foreach (var item in li)
                {
                    JsonHeaderLogics.Add(item);
                }
                //JsonHeaderLogics
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 打开该弹窗时执行
        /// </summary>
        public async void OnDialogOpend(IDialogParameters parameters)
        {
            if (parameters != null && parameters.ContainsKey("Value"))
            {
                //var oldDto = parameters.GetValue<EmployeeDto>("Value");
                //var getResult = await employeeService.GetSinglePersonalStorageAsync(oldDto);
                //if(getResult != null && getResult.Status)
                //{
                //    CurrentDto = getResult.Result;
                //}
            }
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

        private async void OpenDialogView()
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
        #endregion
    }
}
