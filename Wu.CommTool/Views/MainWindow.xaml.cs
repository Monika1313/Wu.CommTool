using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Wu.CommTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //最小化
            btnMin.Click += (s, e) =>
            {
                this.WindowState = WindowState.Minimized;
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
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
