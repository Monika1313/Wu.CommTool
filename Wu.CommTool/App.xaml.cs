using log4net;
using Newtonsoft.Json;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.IO;
using System.Windows;
using Wu.CommTool.Common;
using Wu.CommTool.Dialogs.Views;
using Wu.CommTool.Models;
using Wu.CommTool.ViewModels;
using Wu.CommTool.ViewModels.DialogViewModels;
using Wu.CommTool.Views;
using Wu.CommTool.Views.Dialogs;
using Wu.CommTool.Modules.ConvertTools;
using Wu.CommTool.Modules.About;
using Wu.CommTool.Modules.ModbusTcp;
using Wu.CommTool.Modules.JsonTool;
using Wu.CommTool.Modules.Message;

namespace Wu.CommTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static AppConfig AppConfig { get; set; } = new();
        public static string ConfigDict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs");
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            //设置该软件的工作目录为当前软件目录
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var xx = Directory.GetCurrentDirectory();
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
            containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();                                           //首页
            containerRegistry.RegisterForNavigation<ModbusRtuView, ModbusRtuViewModel>();                                   //ModbusRtu
            containerRegistry.RegisterForNavigation<ModbusRtuAutoSearchDeviceView, ModbusRtuAutoSearchDeviceViewModel>();   //消息提示窗口
            containerRegistry.RegisterForNavigation<ModbusRtuAutoResponseDataEditView, ModbusRtuAutoResponseDataEditViewModel>();//ModbusRtu 自动应答编辑界面
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();                                               //消息提示窗口
            containerRegistry.RegisterForNavigation<MqttView, MqttViewModel>();                                             //Mqtt
            containerRegistry.RegisterForNavigation<MqttServerView, MqttServerViewModel>();                                 //MqttServer
            containerRegistry.RegisterForNavigation<MqttClientView, MqttClientViewModel>();                                 //MqttClient
            containerRegistry.RegisterForNavigation<JsonToolView, JsonToolViewModel>();                                     //Json工具界面
            //弹窗界面
            containerRegistry.RegisterForNavigation<JsonDataView, JsonDataViewModel>();                                     //Json数据查看界面
        }

        /// <summary>
        /// 初始化完成
        /// </summary>
        protected override void OnInitialized()
        {
            //初始化窗口
            if (Current.MainWindow.DataContext is IConfigureService service)
                service.Configure();
            Current.MainWindow.Width = AppConfig!.WinWidth;
            Current.MainWindow.Height = AppConfig.WinHeight;
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
        }

    }
}
