using DryIoc;
using Microsoft.Win32;
using Prism.Ioc;
using System.Xml.Linq;
using Wu.CommTool.Core.Views;

namespace Wu.CommTool.Modules.ModbusTcp.ViewModels;

public partial class MtcpDeviceMonitorViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private static readonly ILog log = LogManager.GetLogger(typeof(MtcpDeviceMonitorViewModel));
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public MtcpDeviceMonitorViewModel() { }
    public MtcpDeviceMonitorViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        Task.Run(() => GetDefaultConfig());
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
            case "OpenRightDrawer": OpenDrawers.RightDrawer = true; break;
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


    #region 配置文件
    /// <summary>
    /// 配置文件夹路径
    /// </summary>
    private readonly string configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\MtcpDeviceMonitorConfig");

    /// <summary>
    /// 配置文件扩展名
    /// </summary>
    private readonly string configExtension = "jsonMtDM";

    /// <summary>
    /// 当前配置文件名称
    /// </summary>
    public string CurrentConfigName => Path.GetFileNameWithoutExtension(CurrentConfigFullName);

    /// <summary>
    /// 当前配置文件完整路径
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentConfigName))]
    string currentConfigFullName = string.Empty;

    /// <summary>
    /// 读取默认配置文件 若无则生成
    /// </summary>
    private void GetDefaultConfig()
    {
        //导入默认自动应答配置
        try
        {
            var filePath = Path.Combine(configDirectory, $"Default.{configExtension}");
            CurrentConfigFullName = filePath;
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
                InitialDefaultData();//文件不存在则生成默认配置 
                Wu.Utils.IoUtil.Exists(configDirectory);
                var content = JsonConvert.SerializeObject(MtcpDeviceManager);      //将当前的配置序列化为json字符串
                Core.Common.Utils.WriteJsonFile(filePath, content);                     //保存文件
            }
        }
        catch { }
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
            Wu.Utils.IoUtil.Exists(configDirectory);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                                              //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                InitialDirectory = configDirectory
            };

            if (dlg.ShowDialog() != true)
                return;
            CurrentConfigFullName = dlg.FileName;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            var x = JsonConvert.DeserializeObject<MtcpDeviceManager>(xx);
            MtcpDeviceManager = x;
            MtcpDeviceManager.SelectedMtcpDevice = MtcpDeviceManager.MtcpDevices.FirstOrDefault();
            HcGrowlExtensions.Success($"导入配置:{CurrentConfigName}");
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
            Wu.Utils.IoUtil.Exists(configDirectory);                                                                   //验证文件夹是否存在, 不存在则创建
            SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",                                       //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",    //文件格式过滤器
                FilterIndex = 1,                                                    //默认选中的过滤器
                FileName = "Default",                                               //默认文件名
                DefaultExt = configExtension,                                       //默认扩展名
                InitialDirectory = configDirectory,                                 //指定初始的目录
                OverwritePrompt = true,                                             //文件已存在警告
                AddExtension = true,                                                //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            CurrentConfigFullName = sfd.FileName;
            var content = JsonConvert.SerializeObject(MtcpDeviceManager);     //将当前的配置序列化为json字符串
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);                //保存文件
            HcGrowlExtensions.Success($"导出配置:{CurrentConfigName}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"配置文件导出失败...{ex.Message}");
        }
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(MtcpDeviceManager);
            //保存文件
            Core.Common.Utils.WriteJsonFile(CurrentConfigFullName, content);
            HcGrowlExtensions.Success($"保存配置:{CurrentConfigName}");
            //RefreshQuickImportList();
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"保存配置失败 {ex.Message}");
        }
    }
    #endregion

    /// <summary>
    /// 配置设备
    /// </summary>
    [RelayCommand]
    [property: JsonIgnore]
    private async Task ConfigMtcpDevice(MtcpDevice MtcpDevice)
    {
        try
        {
            //DialogParameters param = new()
            //{
            //    { "Value", MtcpDevice }
            //};
            //var dialogResult = await dialogHost.ShowDialog(nameof(MtcpDeviceEditView), param, nameof(ModbusTcpView));

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
        try
        {
            if (obj == null)
            {
                return;
            }
            DialogParameters param = new()
            {
                { "Value", obj }
            };
            var dialogResult = await dialogHost.ShowDialog(nameof(MtcpDeviceEditView), param, nameof(MtcpDeviceMonitorView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
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
    private void OpenCurrentMtcpDeviceLogView()
    {
        try
        {
            //DialogParameters param = new()
            //{
            //    { "Value", MtcpDeviceManager }
            //};
            //var dialogResult = await dialogHost.ShowDialog(nameof(MtcpDeviceManagerLogView), param, nameof(MtcpDeviceMonitorView));

            #region 以非模态窗口显示
            var content = provider.Resolve<MtcpLogView>();//从容器中取出实例
           
            //验证实例的有效性
            #region 验证实例的有效性
            if (!(content is FrameworkElement dialogContent))
                throw new NullReferenceException("A dialog's content must be a FrameworkElement...");

            if (dialogContent is FrameworkElement view && view.DataContext is null && ViewModelLocator.GetAutoWireViewModel(view) is null)
                ViewModelLocator.SetAutoWireViewModel(view, true);

            if (!(dialogContent.DataContext is IDialogHostAware viewModel))
                throw new NullReferenceException("A dialog's ViewModel must implement the IDialogHostService interface");
            #endregion

            //没有选中项则选中第一个
            if (MtcpDeviceManager.SelectedMtcpDevice == null)
            {
                MtcpDeviceManager.SelectedMtcpDevice = MtcpDeviceManager.MtcpDevices.FirstOrDefault();
            }

            DialogParameters parameters = new()
            {
                { "Value", MtcpDeviceManager.SelectedMtcpDevice }
            };

            var window = new Window()
            {
                Content = dialogContent,
                Name = nameof(MtcpLogView),
                Width = 700,
                Height = 500,
                WindowStartupLocation= WindowStartupLocation.CenterScreen
            };

            //var window = new TemplateWindow()
            //{
            //    Content = dialogContent,
            //    Name = nameof(MtcpLogView),
            //    Width = 700,
            //    Height = 500,
            //};
            window.Show();// 显示窗口
            if (viewModel is IDialogHostAware aware)
            {
                aware.OnDialogOpened(parameters);
            }
            #endregion

        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }
}
