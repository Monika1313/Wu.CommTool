using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;

namespace Wu.CommTool.Views;

public partial class MainWindow : Window
{
    public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

    public MainWindow()
    {
        InitializeComponent();

        //最小化
        btnMin.Click += (s, e) =>
        {
            WindowState = WindowState.Minimized;
        };
        //最大化
        btnMax.Click += (s, e) =>
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        };
        //关闭
        btnClose.Click += async (s, e) =>
        {
            this.Close();
        };
        
        menuBar.SelectionChanged += (s, e) =>
        {
            drawerHost.IsLeftDrawerOpen = false;
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Environment.Exit(0);
    }

    //关闭前先保存配置
    protected override void OnClosing(CancelEventArgs e)
    {
        //关闭时保存配置文件
        try
        {
            //配置文件目录
            string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs");
            Wu.Utils.IoUtil.Exists(dict);

            //若已经最大化, 使用最大化前的大小
            if (!WindowState.Equals(WindowState.Normal))
            {
                //获取最大化或最小化前的窗口大小
                Rect rb = this.RestoreBounds;
                App.AppConfig.WinWidth = rb.Width;
                App.AppConfig.WinHeight = rb.Height;
            }
            else
            {
                App.AppConfig.WinWidth = this.Width > SystemParameters.WorkArea.Size.Width ? SystemParameters.WorkArea.Size.Width : this.Width;
                App.AppConfig.WinHeight = this.Height > SystemParameters.WorkArea.Size.Height ? SystemParameters.WorkArea.Size.Height : this.Height;
            }
            //是否最大化
            App.AppConfig.IsMaximized = this.WindowState.Equals(WindowState.Maximized);

            //将当前的配置序列化为json字符串
            var content = JsonConvert.SerializeObject(App.AppConfig);
            //保存文件
            Common.Utils.WriteJsonFile(Path.Combine(dict, "AppConfig.jsonAppConfig"), content);
        }
        catch (Exception ex)
        {
            log.Error(ex.Message);
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
    }
}
