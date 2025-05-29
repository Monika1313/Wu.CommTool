using Wu.CommTool.DynamicTheme.Core;
using Wu.CommTool.Modules.TcpClient;
using Wu.CommTool.Modules.TcpServer;

namespace Wu.CommTool;

public partial class App
{
    public static AppConfig AppConfig { get; set; } = new();
    public static string ConfigDict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs");
    public static readonly ILog log = LogManager.GetLogger(typeof(App));
    //public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

    protected override void OnStartup(StartupEventArgs e)
    {
        //设置该软件的工作目录为当前软件目录
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        //var xx = Directory.GetCurrentDirectory();
        //指定log4net的配置文件
        log4net.Config.XmlConfigurator.Configure(new FileInfo(Directory.GetCurrentDirectory() + "/Configs/Log4netConfig/log4net.config"));
        log.Info("App启动中...");
        //读取配置文件
        try
        {
            string configStr = Common.Utils.ReadJsonFile(Path.Combine(ConfigDict, "AppConfig.jsonAppConfig"));
            if (!string.IsNullOrWhiteSpace(configStr))
            {
                AppConfig = JsonConvert.DeserializeObject<AppConfig>(configStr)!;
            }
        }
        catch (Exception ex)
        {
            log.Info("读取App配置文件失败", ex);
        }

        base.OnStartup(e);
    }

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="containerRegistry"></param>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        //注册自定义对话主机服务
        containerRegistry.Register<IDialogHostService, DialogHostService>();

        //注册页面
        containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();//消息提示窗口
        containerRegistry.RegisterForNavigation<AutoUpdaterView, AutoUpdaterViewModel>();//自动更新窗口
        //containerRegistry.RegisterForNavigation<ProgressView>();
        //containerRegistry.RegisterInstance<ILog>(LogManager.GetLogger(typeof(App)));
        //RegisterSingleton

        //注册主题管理
        containerRegistry.RegisterSingleton<ThemeManager>();
    }

    /// <summary>
    /// 初始化完成
    /// </summary>
    protected override void OnInitialized()
    {
        //初始化窗口
        if (Current.MainWindow.DataContext is IConfigureService service)
            service.Configure();

        //限制窗口的大小在工作区域范围内,避免软件在不同分辨率的电脑,使用同一个配置文件导致显示异常
        Current.MainWindow.Width = Math.Min(AppConfig!.WinWidth, SystemParameters.WorkArea.Size.Width);
        Current.MainWindow.Height = Math.Min(AppConfig.WinHeight, SystemParameters.WorkArea.Size.Height);
        if (AppConfig.IsMaximized)
            Current.MainWindow.WindowState = WindowState.Maximized;

        base.OnInitialized();
    }

    /// <summary>
    /// 模块加载
    /// </summary>
    /// <param name="moduleCatalog"></param>
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<ConvertToolsModule>();  //转换工具模块
        moduleCatalog.AddModule<AboutModule>();         //关于模块
        moduleCatalog.AddModule<ModbusTcpModule>();     //ModbusTcp模块
        moduleCatalog.AddModule<JsonToolModule>();      //Json工具模块
        moduleCatalog.AddModule<MessageModule>();       //弹窗消息模块
        moduleCatalog.AddModule<MqttServerModule>();    //Mqtt服务器模块
        moduleCatalog.AddModule<MqttClientModule>();    //Mqtt客户端模块
        moduleCatalog.AddModule<ModbusRtuModule>();     //ModbusRtu模块
        moduleCatalog.AddModule<NetworkToolModule>();   //网络工具模块
        moduleCatalog.AddModule<TcpServerModule>();   //Tcp服务器模块
        moduleCatalog.AddModule<TcpClientModule>();   //Tcp客户端模块
    }
}
