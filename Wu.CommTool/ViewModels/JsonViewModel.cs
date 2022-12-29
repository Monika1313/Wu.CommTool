using HandyControl.Controls;
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

            JsonString = @"{""ClientID"":""MQ00110001000001"",""pver"":1,""time"":""2022-12-29 10:11:00"",""type"":""dataUpload"",""data"":{""dataUploadReq"":{""devStatus"":{""timeStart"":""2022-12-13 15:31:08"",""timeRun"":15},""swsData"":{""timeBegin"":""2022-12-29 10:11:00"",""gap"":{""unit"":1,""val"":5},""dataNum"":12,""db"":{""dya"":[231.500,231.500,231.400,230.800,232.100,232.100,231.900,232.100,232.000,232.200,232.200],""dyb"":[230.100,229.500,230.100,230.400,230.500,230.700,230.700,230.000,229.900,229.700,229.900],""dyc"":[233.900,233.700,233.700,233.000,233.900,233.800,233.800,233.800,233.600,233.200,233.400],""dla"":[2.300,2.330,2.170,2.260,2.330,2.310,2.300,2.550,2.380,2.480,2.510],""dlb"":[2.090,2.050,1.980,2.210,2.140,2.170,2.190,2.300,2.080,2.180,2.210],""dlc"":[2.500,2.450,2.370,2.500,2.470,2.480,2.490,2.640,2.460,2.470,2.520],""ct"":[1,1,1,1,1,1,1,1,1,1,1],""pt"":[1,1,1,1,1,1,1,1,1,1,1],""pl"":[49.990,49.990,49.990,49.990,49.970,49.950,49.950,49.960,49.950,49.960,49.960],""yggl"":[0.819,0.815,0.767,0.827,0.833,0.831,0.830,0.901,0.825,0.860,0.874],""wggl"":[-0.128,-0.125,-0.117,-0.132,-0.129,-0.129,-0.129,-0.142,-0.128,-0.134,-0.136],""glys"":[0.988,0.988,0.989,0.988,0.988,0.988,0.988,0.988,0.988,0.988,0.988],""ygdd"":[325.090,325.090,325.090,325.090,325.090,325.090,325.100,325.100,325.100,325.100,325.100],""wgdd"":[54.590,54.590,54.590,54.590,54.590,54.590,54.590,54.590,54.590,54.590,54.590]},""jz_1"":{""jsyl"":[0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000],""csyl"":[0.350,0.340,0.350,0.350,0.350,0.350,0.350,0.350,0.350,0.350,0.350],""sdyl"":[0.350,0.350,0.350,0.350,0.350,0.350,0.350,0.350,0.350,0.350,0.350],""ssll"":[0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000],""ljll"":[0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000]},""bj"":{""sygz"":[0,0,0,0,0,0,0,0,0,0,0],""dbgz"":[0,0,0,0,0,0,0,0,0,0,0],""kzxtgz"":[0,0,0,0,0,0,0,0,0,0,0],""gsycgz"":[0,0,0,0,0,0,0,0,0,0,0],""cscygz"":[0,0,0,0,0,0,0,0,0,0,0]},""sb_1_1"":{""zt"":[1,1,1,1,1,1,1,1,1,1,1],""pl"":[45.060,44.130,45.240,45.150,45.150,45.150,45.140,44.870,45.280,45.260,45.310],""yxsj"":[378,378,378,378,378,378,378,378,378,378,378],""lxyxsj"":[1,1,1,1,1,1,1,1,1,1,1],""gzcs"":[0,0,0,0,0,0,0,0,0,0,0],""gz"":[0,0,0,0,0,0,0,0,0,0,0],""szdzt"":[1,1,1,1,1,1,1,1,1,1,1]},""sb_1_2"":{""zt"":[0,0,0,0,0,0,0,0,0,0,0],""pl"":[0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000,0.000],""yxsj"":[0,0,0,0,0,0,0,0,0,0,0],""lxyxsj"":[1,1,1,1,1,1,1,1,1,1,1],""gzcs"":[2,2,2,2,2,2,2,2,2,2,2],""gz"":[1,1,1,1,1,1,1,1,1,1,1],""szdzt"":[1,1,1,1,1,1,1,1,1,1,1]}}}}}";
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
                MessageBox.Show(ex.Message);
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
