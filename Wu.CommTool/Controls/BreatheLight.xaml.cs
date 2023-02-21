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
            SetCurrentValue(OnColorProperty, Brushes.LawnGreen);
            SetCurrentValue(OffColorProperty, Brushes.Red);
            SetCurrentValue(MarginProperty,new Thickness(5));
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
        public Brush OnColor
        {
            get { return (Brush)GetValue(OnColorProperty); }
            set { SetValue(OnColorProperty, value); }
        }
        public static readonly DependencyProperty OnColorProperty =
                    DependencyProperty.Register("OnColor", typeof(Brush), typeof(BreatheLight),
                        new FrameworkPropertyMetadata(default(Brush),
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("Wu")]
        [Description("Off时的颜色")]
        public Brush OffColor
        {
            get { return (Brush)GetValue(OffColorProperty); }
            set { SetValue(OffColorProperty, value); }
        }
        public static readonly DependencyProperty OffColorProperty =
                    DependencyProperty.Register("OffColor", typeof(Brush), typeof(BreatheLight),
                        new FrameworkPropertyMetadata(default(Brush),
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("Wu")]
        [Description("Margin外边距")]
        public new Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }
        public static new readonly DependencyProperty MarginProperty =
                    DependencyProperty.Register("Margin", typeof(Thickness), typeof(BreatheLight),
                        new FrameworkPropertyMetadata(default(Thickness),
                        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
    }
}
