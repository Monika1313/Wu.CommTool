using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Wu.CommTool.Views
{
    /// <summary>
    /// AboutView.xaml 的交互逻辑
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hyperlink link = sender as Hyperlink;
                System.Diagnostics.Process.Start("explorer.exe", link.NavigateUri.AbsoluteUri);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
