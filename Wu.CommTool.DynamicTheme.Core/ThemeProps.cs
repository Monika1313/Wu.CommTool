using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Wu.CommTool.DynamicTheme.Core;

public class ThemeProps
{
    #region Background
    public static Brush GetBackground(DependencyObject obj)
    {
        return (Brush)obj.GetValue(BackgroundProperty);
    }

    public static void SetBackground(DependencyObject obj, Brush value)
    {
        obj.SetValue(BackgroundProperty, value);
    }

    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached("Background", typeof(Brush), typeof(ThemeProps), new PropertyMetadata(null, OnBackgroundPropertyChanged));

    private static void OnBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //目前仅支持SolidColorBrush,后续可以扩展
        if (d is FrameworkElement element && e.NewValue is SolidColorBrush newBrush)
        {
            AnimateBrushProperty(element, newBrush, BackgroundProperty.Name);
        }
    }
    #endregion

    #region Foreground
    public static Brush GetForeground(DependencyObject obj)
    {
        return (Brush)obj.GetValue(ForegroundProperty);
    }

    public static void SetForeground(DependencyObject obj, Brush value)
    {
        obj.SetValue(ForegroundProperty, value);
    }

    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached("Foreground", typeof(Brush), typeof(ThemeProps), new PropertyMetadata(null, OnForegroundPropertyChanged));

    private static void OnForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //目前仅支持SolidColorBrush,后续可以扩展
        if (d is FrameworkElement element && e.NewValue is SolidColorBrush newBrush)
        {
            AnimateBrushProperty(element, newBrush, ForegroundProperty.Name);
        }
    }
    #endregion

    #region BorderBrush
    public static Brush GetBorderBrush(DependencyObject obj)
    {
        return (Brush)obj.GetValue(BorderBrushProperty);
    }

    public static void SetBorderBrush(DependencyObject obj, Brush value)
    {
        obj.SetValue(BorderBrushProperty, value);
    }

    public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.RegisterAttached("BorderBrush", typeof(Brush), typeof(ThemeProps), new PropertyMetadata(null, OnBorderBrushPropertyChanged));

    private static void OnBorderBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //目前仅支持SolidColorBrush,后续可以扩展
        if (d is FrameworkElement element && e.NewValue is SolidColorBrush newBrush)
        {
            AnimateBrushProperty(element, newBrush, BorderBrushProperty.Name);
        }
    }
    #endregion

    #region Fill
    public static Brush GetFill(DependencyObject obj)
    {
        return (Brush)obj.GetValue(FillProperty);
    }

    public static void SetFill(DependencyObject obj, Brush value)
    {
        obj.SetValue(FillProperty, value);
    }

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.RegisterAttached("Fill", typeof(Brush), typeof(ThemeProps), new PropertyMetadata(null, OnFillPropertyChanged));

    private static void OnFillPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //目前仅支持SolidColorBrush,后续可以扩展
        if (d is FrameworkElement element && e.NewValue is SolidColorBrush newBrush)
        {
            AnimateBrushProperty(element, newBrush, FillProperty.Name);
        }
    }
    #endregion

    /// <summary>
    /// 颜色改变动画
    /// </summary>
    /// <param name="element"></param>
    /// <param name="newBrush"></param>
    /// <param name="proertyName"></param>
    private static void AnimateBrushProperty(FrameworkElement element, SolidColorBrush newBrush, string proertyName)
    {
        var property = element.GetType().GetProperty(proertyName);

        if (property == null) return;

        var currentBrush = property.GetValue(element) as SolidColorBrush;

        if (currentBrush == null || currentBrush.IsFrozen)
        {
            currentBrush = new SolidColorBrush(newBrush.Color);
            property.SetValue(element, currentBrush);
        }

        currentBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation()
        {
            To = newBrush.Color,
            Duration = TimeSpan.FromSeconds(0.3)
        });
    }
}
