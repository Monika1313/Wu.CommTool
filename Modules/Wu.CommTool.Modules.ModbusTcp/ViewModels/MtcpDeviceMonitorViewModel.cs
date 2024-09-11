using Microsoft.Win32;

namespace Wu.CommTool.Modules.ModbusTcp.ViewModels;

public partial class MtcpDeviceMonitorViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private static readonly ILog log = LogManager.GetLogger(typeof(MtcpDeviceMonitorViewModel));
    private readonly string mtcpDeviceManagerConfigFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MtcpDeviceMonitorConfig");
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public MtcpDeviceMonitorViewModel() { }
    public MtcpDeviceMonitorViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        //Task.Run(() => GetDefaultConfig());
    }

    /// <summary>
    /// 导航至该页面时执行
    /// </summary>
    /// <param name="navigationContext"></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {

    }

    /// <summary>
    /// 打开该弹窗时执行
    /// </summary>
    public void OnDialogOpened(IDialogParameters parameters)
    {

    }
    #endregion


    #region **************************************** 属性 ****************************************
    public string DialogHostName { get; set; }

    /// <summary>
    /// ModbusTcp设备管理
    /// </summary>
    [ObservableProperty]
    MtcpDeviceManager mtcpDeviceManager = new();

    [ObservableProperty]
    OpenDrawers openDrawers = new();
    #endregion **************************************** 属性 ****************************************


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            case "Search": Search(); break;
            case "OpenDialogView": OpenDialogView(); break;
            default: break;
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        //添加返回的参数
        DialogParameters param = new();
        //param.Add("Value", CurrentDto);
        //关闭窗口,并返回参数
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
    }

    [RelayCommand]
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
                //{ "Value", CurrentDto }
            };
            //var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    private void Search()
    {

    }
    #endregion

    /// <summary>
    /// 读取默认配置文件 若无则生成
    /// </summary>
    private void GetDefaultConfig()
    {
        //导入默认自动应答配置
        try
        {
            var filePath = Path.Combine(mtcpDeviceManagerConfigFolder, "Default.jsonMtDM");

            if (File.Exists(filePath))
            {
                var obj = JsonConvert.DeserializeObject<MtcpDeviceManager>(Core.Common.Utils.ReadJsonFile(filePath));
                if (obj != null)
                {
                    MtcpDeviceManager = obj;
                    MtcpDeviceManager.SelectedMtcpDevice = MtcpDeviceManager.MtcpDevices.FirstOrDefault();
                }
            }
            else
            {
                //文件不存在则生成默认配置 
                InitialDefaultData();
                var content = JsonConvert.SerializeObject(MtcpDeviceManager);       //将当前的配置序列化为json字符串
                Core.Common.Utils.WriteJsonFile(filePath, content);                     //保存文件
            }
        }
        catch { }
    }

    protected void InitialDefaultData()
    {
        MtcpDeviceManager = new MtcpDeviceManager();
        MtcpDeviceManager.MtcpDevices.Add(new MtcpDevice() { Name = "测试设备1" });
        //var device = MtcpDeviceManager.MtcpDevices[0];
        //device.MtcpDatas.Add(new MtcpData() { Name = "测点1", RegisterAddr = 0, MtcpDataType = MtcpDataType.Float });
        //device.MtcpDatas.Add(new MtcpData() { Name = "测点2", RegisterAddr = 2, MtcpDataType = MtcpDataType.Short });
        //device.MtcpDatas.Add(new MtcpData() { Name = "测点3", RegisterAddr = 10, MtcpDataType = MtcpDataType.uInt });
        //MtcpDeviceManager.SelectedMtcpDevice = device;
    }

    /// <summary>
    /// 导入配置文件
    /// </summary>
    [RelayCommand]
    private void ImportConfig()
    {
        try
        {
            //配置文件目录
            Wu.Utils.IoUtil.Exists(mtcpDeviceManagerConfigFolder);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                                              //对话框标题
                Filter = "json files(*.jsonMtDM)|*.jsonMtDM",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                InitialDirectory = mtcpDeviceManagerConfigFolder
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            var x = JsonConvert.DeserializeObject<MtcpDeviceManager>(xx);
            MtcpDeviceManager = x;
            MtcpDeviceManager.SelectedMtcpDevice = MtcpDeviceManager.MtcpDevices.FirstOrDefault();
            HcGrowlExtensions.Success("配置文件导入成功");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"配置文件导入失败...{ex.Message}");
        }
    }

    /// <summary>
    /// 导出配置文件
    /// </summary>
    [RelayCommand]
    private void ExportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(mtcpDeviceManagerConfigFolder);                                                                   //验证文件夹是否存在, 不存在则创建
            SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",                             //对话框标题
                Filter = "json files(*.jsonMtDM)|*.jsonMtDM",                 //文件格式过滤器
                FilterIndex = 1,                                            //默认选中的过滤器
                FileName = "Default",                                       //默认文件名
                DefaultExt = "jsonMtDM",                                     //默认扩展名
                InitialDirectory = mtcpDeviceManagerConfigFolder,                                    //指定初始的目录
                OverwritePrompt = true,                                     //文件已存在警告
                AddExtension = true,                                        //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            var content = JsonConvert.SerializeObject(MtcpDeviceManager);    //将当前的配置序列化为json字符串
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);              //保存文件
            HcGrowlExtensions.Success("配置文件导出成功");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"配置文件导出失败...{ex.Message}");
        }
    }

    /// <summary>
    /// 配置设备
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private async Task ConfigMtcpDevice(MtcpDevice MtcpDevice)
    {
        try
        {
            DialogParameters param = new()
            {
                { "Value", MtcpDevice }
            };
            //var dialogResult = await dialogHost.ShowDialog(nameof(ConfigMtcpDeviceView), param, nameof(ModbusRtuView));

            //if (dialogResult.Result == ButtonResult.OK)
            //{
            //    //更新配置
            //}
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }


    [RelayCommand]
    [property: JsonIgnore]
    private async Task EditMtcpDevice(MtcpDevice obj)
    {
        //try
        //{
        //    if (obj == null)
        //    {
        //        return;
        //    }
        //    DialogParameters param = new()
        //    {
        //        { "Value", obj }
        //    };
        //    var dialogResult = await dialogHost.ShowDialog(nameof(MtcpDeviceEditView), param, nameof(MtcpDeviceMonitorView));
        //}
        //catch (Exception ex)
        //{
        //    HcGrowlExtensions.Warning(ex.Message);
        //}
    }


    [RelayCommand]
    [property: JsonIgnore]
    private async Task OpenMtcpDataEditView(MtcpData obj)
    {
        //try
        //{
        //    if (obj == null)
        //    {
        //        return;
        //    }
        //    DialogParameters param = new()
        //    {
        //        { "Value", obj }
        //    };
        //    var dialogResult = await dialogHost.ShowDialog(nameof(MtcpDataEditView), param, nameof(MtcpDeviceMonitorView));
        //}
        //catch (Exception ex)
        //{
        //    HcGrowlExtensions.Warning(ex.Message);
        //}
    }

    [RelayCommand]
    [property: JsonIgnore]
    private async Task OpenMtcpDeviceManagerLogView()
    {
        //try
        //{
        //    DialogParameters param = new()
        //    {
        //        { "Value", MtcpDeviceManager }
        //    };
        //    var dialogResult = await dialogHost.ShowDialog(nameof(MtcpDeviceManagerLogView), param, nameof(MtcpDeviceMonitorView));
        //}
        //catch (Exception ex)
        //{
        //    HcGrowlExtensions.Warning(ex.Message);
        //}
    }
}
