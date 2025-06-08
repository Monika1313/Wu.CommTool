using Wu.CommTool.Modules.TcpServer.Models;

namespace Wu.CommTool.Modules.TcpServer.ViewModels;

public partial class TcpServerViewModel : NavigationViewModel, IDialogHostAware
{
    #region    **************************************** 字段 ****************************************
    private readonly IContainerProvider provider;
    private readonly IDialogHostService dialogHost;
    private readonly string configFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs\TcpServerConfig");
    #endregion **************************************** 字段 ****************************************


    #region **************************************** 构造函数 ****************************************
    public TcpServerViewModel() { }
    public TcpServerViewModel(IContainerProvider provider, IDialogHostService dialogHost) : base(provider)
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

    [ObservableProperty] TcpServerModel tcpServerModel = new();

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
    /// 导出配置文件
    /// </summary>
    [RelayCommand]
    private void ExportConfig()
    {
        try
        {
            //配置文件目录
            string dict = configFolder;
            Wu.Utils.IoUtil.Exists(dict);
            SaveFileDialog sfd = new()
            {
                Title = "请选择导出配置文件...",                                              //对话框标题
                Filter = "json files(*.jts)|*.jts",    //文件格式过滤器
                FilterIndex = 1,                                                         //默认选中的过滤器
                FileName = "Default",                                           //默认文件名
                DefaultExt = "jts",                                     //默认扩展名
                InitialDirectory = dict,                //指定初始的目录
                OverwritePrompt = true,                                                  //文件已存在警告
                AddExtension = true,                                                     //若用户省略扩展名将自动添加扩展名
            };
            if (sfd.ShowDialog() != true)
                return;
            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(TcpServerModel);
            //保存文件
            Core.Common.Utils.WriteJsonFile(sfd.FileName, content);
            HcGrowlExtensions.Success("配置导出完成");
        }
        catch (Exception ex)
        {
            HcGrowlExtensions.Warning("配置导出失败");
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
            //配置文件目录
            string dict = configFolder;
            Wu.Utils.IoUtil.Exists(dict);
            //选中配置文件
            OpenFileDialog dlg = new()
            {
                Title = "请选择导入配置文件...",                      //对话框标题
                Filter = "json files(*.jts)|*.jts",          //文件格式过滤器
                FilterIndex = 1,                                     //默认选中的过滤器
                InitialDirectory = dict
            };

            if (dlg.ShowDialog() != true)
                return;
            var xx = Core.Common.Utils.ReadJsonFile(dlg.FileName);
            var x = JsonConvert.DeserializeObject<TcpServerModel>(xx);
            TcpServerModel = x;
            HcGrowlExtensions.Success($"配置文件\"{Path.GetFileNameWithoutExtension(dlg.FileName)}\"导入成功");
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
            var filePath = Path.Combine(configFolder, @"Default.jts");
            if (File.Exists(filePath))
            {
                var x = JsonConvert.DeserializeObject<TcpServerModel>(Core.Common.Utils.ReadJsonFile(filePath));
                if (x != null)
                {
                    TcpServerModel = x;
                }
            }
            else
            {
                //文件不存在则生成默认配置 
                TcpServerModel = new TcpServerModel();
                //在默认文件目录生成默认配置文件
                Wu.Utils.IoUtil.Exists(configFolder);
                var content = JsonConvert.SerializeObject(TcpServerModel);       //将当前的配置序列化为json字符串
                Core.Common.Utils.WriteJsonFile(filePath, content);                     //保存文件
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"配置文件读取失败:{ex.Message}");
        }
    }
    ///// <summary>
    ///// 导入配置文件
    ///// </summary>
    ///// <param name="filePath"></param>
    //[RelayCommand]
    //private void ImportConfig(string filePath)
    //{
    //    try
    //    {
    //        var xx = Core.Common.Utils.ReadJsonFile(filePath);//读取文件
    //        var x = JsonConvert.DeserializeObject<TcpClientModel>(xx)!;//反序列化
    //        if (x == null)
    //        {
    //            Growl.Error("读取配置文件失败");
    //            return;
    //        }
    //        TcpClientModel = x;
    //        HcGrowlExtensions.Success($"配置文件\"{Path.GetFileNameWithoutExtension(filePath)}\"导入成功");
    //    }
    //    catch (Exception ex)
    //    {
    //        HcGrowlExtensions.Warning($"配置文件导入失败{ex.Message}");
    //    }
    //}
    #endregion
}
