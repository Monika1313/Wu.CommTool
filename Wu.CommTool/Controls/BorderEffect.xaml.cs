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
    /// BorderEffect.xaml 的交互逻辑
    /// </summary>
    public partial class BorderEffect : UserControl
    {
        public BorderEffect()
        {
            InitializeComponent();
        }


        [Category("property")]
        [Description("")]
        public int MyProperty
        {
            get { return (int)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DependencyProperty MyPropertyProperty =
                    DependencyProperty.Register("MyProperty", typeof(int), typeof(BorderEffect),
                        new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));


    }
}
