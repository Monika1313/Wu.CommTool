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
    /// 呼吸灯
    /// </summary>
    public partial class BreatheLight : UserControl
    {
        public BreatheLight()
        {
            InitializeComponent();
        }


        [Category("Wu")]
        [Description("是否呼吸")]
        public bool IsBreathing
        {
            get { return (bool)GetValue(IsBreathingProperty); }
            set { SetValue(IsBreathingProperty, value); }
        }
        public static readonly DependencyProperty IsBreathingProperty =
                    DependencyProperty.Register("IsBreathing", typeof(bool), typeof(BreatheLight),
                        new FrameworkPropertyMetadata(default(bool),
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("Wu")]
        [Description("On时的颜色")]
        public SolidColorBrush OnColor
        {
            get { return (SolidColorBrush)GetValue(OnColorProperty); }
            set { SetValue(OnColorProperty, value); }
        }
        public static readonly DependencyProperty OnColorProperty =
                    DependencyProperty.Register("OnColor", typeof(SolidColorBrush), typeof(BreatheLight),
                        new FrameworkPropertyMetadata(default(SolidColorBrush),
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("Wu")]
        [Description("Off时的颜色")]
        public SolidColorBrush OffColor
        {
            get { return (SolidColorBrush)GetValue(OffColorProperty); }
            set { SetValue(OffColorProperty, value); }
        }
        public static readonly DependencyProperty OffColorProperty =
                    DependencyProperty.Register("OffColor", typeof(SolidColorBrush), typeof(BreatheLight),
                        new FrameworkPropertyMetadata(default(SolidColorBrush),
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));






    }
}
