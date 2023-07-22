using log4net;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Wu.CommTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                //关闭时保存配置文件

                try
                {
                    //配置文件目录
                    string dict = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Configs");
                    Wu.Utils.IOUtil.Exists(dict);

                    //存储当前配置
                    //if (!App.AppConfig.IsMaximized)
                    //{
                    //    App.AppConfig.WinWidth = this.Width > SystemParameters.WorkArea.Size.Width ? SystemParameters.WorkArea.Size.Width : this.Width;
                    //    App.AppConfig.WinHeight = this.Height > SystemParameters.WorkArea.Size.Height ? SystemParameters.WorkArea.Size.Height : this.Height;
                    //}
                    //App.AppConfig.WinWidth = SystemParameters.WorkArea.Size.Width;
                    //App.AppConfig.WinHeight = SystemParameters.WorkArea.Size.Height;

                    //SystemParameters.WorkArea.Size.Width;//当前屏幕工作区的宽和高（除去任务栏）,它也是与设备无关的单位
                    //获取屏幕的大小 包含工作区域和任务栏
                    //SystemParameters.PrimaryScreenWidth
                    //SystemParameters.PrimaryScreenHeight

                    //若已经最大化, 使用最大化前的大小
                    if (!WindowState.Equals(WindowState.Normal))
                    {
                        //获取最大化或最小化前的窗口大小
                        Rect xxx = this.RestoreBounds;
                        App.AppConfig.WinWidth = xxx.Width;
                        App.AppConfig.WinHeight = xxx.Width;
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
                this.Close();
                Environment.Exit(0);
            };
            //移动
            ColorZone.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };
            //双击最大化
            ColorZone.MouseDoubleClick += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
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

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    e.Cancel = true;
        //}

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //if (App.AppConfig is not null && this.WindowState != WindowState.Maximized && this.WindowState != WindowState.Minimized)
            //{
            //    App.AppConfig.WinWidth = this.Width > SystemParameters.WorkArea.Size.Width ? SystemParameters.WorkArea.Size.Width : this.Width;
            //    App.AppConfig.WinHeight = this.Height > SystemParameters.WorkArea.Size.Height ? SystemParameters.WorkArea.Size.Height : this.Height;
            //}
            base.OnRenderSizeChanged(sizeInfo);
        }


    }
}
