using HandyControl.Controls;
using System.Windows.Controls;
using Wu.CommTool.Modules.TcpClient.Models;

namespace Wu.CommTool.Modules.TcpClient.ViewModels;

public partial class TcpClientViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public TcpClientViewModel() { }
    public TcpClientViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
    {
        this.provider = provider;
        this.dialogHost = dialogHost;
        Task.Run(GetDefaultConfig);
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

    [ObservableProperty] object currentDto = new();

    [ObservableProperty] TcpClientModel tcpClientModel = new();

    [ObservableProperty] OpenDrawers openDrawers = new();


    #endregion **************************************** 属性 ****************************************


    #region **************************************** 方法 ****************************************
    [RelayCommand]
    private void Execute(string obj)
    {
        switch (obj)
        {
            case "OpenDialogView": OpenDialogView(); break;
            case "OpenLeftDrawer": OpenDrawers.LeftDrawer = true; break;
            default: break;
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (!DialogHost.IsDialogOpen(DialogHostName))
            return;
        DialogParameters param = new()
        {
            { "Value", CurrentDto }
        };
        DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));//关闭窗口,并返回参数
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
                { "Value", CurrentDto }
            };
            //var dialogResult = await dialogHost.ShowDialog(nameof(DialogView), param, nameof(CurrentView));
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning(ex.Message);
        }
    }
    #endregion

    #region 配置文件
    /// <summary>
    /// 配置文件夹路径
    /// </summary>
    private readonly string configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\TcpClientConfig");

    /// <summary>
    /// 配置文件扩展名 Json TcpClient
    /// </summary>
    private readonly string configExtension = "jtc";

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
    /// 导出配置文件
    /// </summary>
    [RelayCommand]
    private void ExportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);
            SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",                                              //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                FileName = "Default",                                           //默认文件名
                DefaultExt = configExtension,                                     //默认扩展名
                InitialDirectory = configDirectory,                //指定初始的目录
                OverwritePrompt = true,                                                  //文件已存在警告
                AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            CurrentConfigFullName = sfd.FileName;
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(TcpClientModel);
            //保存文件
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);
            HcGrowlExtensions.Success($"导出配置:{CurrentConfigName}");
            //RefreshQuickImportList();
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning($"配置导出失败 {ex.Message}");
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
            var content = JsonConvert.SerializeObject(TcpClientModel);
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

    /// <summary>
    /// 导入配置文件
    /// </summary>
    [RelayCommand]
    private void ImportConfig()
    {
        try
        {
            Wu.Utils.IoUtil.Exists(configDirectory);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                      //对话框标题
                Filter = $"json files(*.{configExtension})|*.{configExtension}",          //文件格式过滤器
                FilterIndex = 1,                                     //默认选中的过滤器
                InitialDirectory = configDirectory
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            var x = JsonConvert.DeserializeObject<TcpClientModel>(xx);
            TcpClientModel = x;
            HcGrowlExtensions.Success($"导入配置:{CurrentConfigName}");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Success(ex.Message);
        }
    }

    /// <summary>
    /// 读取默认配置文件 若无则生成
    /// </summary>
    private void GetDefaultConfig()
    {
        //从默认配置文件中读取配置
        try
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\TcpClientConfig\Default.jtc");
            CurrentConfigFullName = filePath;
            if (File.Exists(filePath))
            {
                var x = JsonConvert.DeserializeObject<TcpClientModel>(Core.Common.Utils.ReadJsonFile(filePath));
                if (x != null)
                {
                    TcpClientModel = x;
                }
            }
            else
            {
                //文件不存在则生成默认配置 
                TcpClientModel = new TcpClientModel();
                //在默认文件目录生成默认配置文件
                Wu.Utils.IoUtil.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\TcpClientConfig"));
                var content = JsonConvert.SerializeObject(TcpClientModel);       //将当前的配置序列化为json字符串
                Core.Common.Utils.WriteJsonFile(filePath, content);                     //保存文件
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"配置文件读取失败:{ex.Message}");
        }
    }
    #endregion
}
