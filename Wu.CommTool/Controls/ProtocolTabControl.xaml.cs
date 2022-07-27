using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wu.CommTool.Controls
{
    /// <summary>
    /// ProtocolTabControl.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolTabControl : UserControl
    {
        public ProtocolTabControl()
        {
            InitializeComponent();
        }



        [Category("Wu")]
        [Description("注释说明")]
        public string Ip
        {
            get { return (string)GetValue(IpProperty); }
            set { SetValue(IpProperty, value); }
        }
        public static readonly DependencyProperty IpProperty =
                    DependencyProperty.Register("Ip", typeof(string), typeof(ProtocolTabControl), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));




    }
}
