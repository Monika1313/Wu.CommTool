namespace Wu.CommTool.Core.Behavior;

/// <summary>
/// 令控件的子项获取焦点的同时获取焦点
/// 例如:ListBoxItem内的TextBox获取焦点时, ListBoxItem无法获取焦点
/// 用法:xaml页面 控件添加属性 AutoSelectWhenAnyChildGetsFocus.Enabled="True"
/// </summary>
public class AutoSelectWhenAnyChildGetsFocus
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled",
        typeof(bool),
        typeof(AutoSelectWhenAnyChildGetsFocus),
        new UIPropertyMetadata(false, Enabled_Changed));

    public static bool GetEnabled(DependencyObject obj) { return (bool)obj.GetValue(EnabledProperty); }
    public static void SetEnabled(DependencyObject obj, bool value) { obj.SetValue(EnabledProperty, value); }

    private static void Enabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var attachEvents = (bool)e.NewValue;
        var targetUiElement = (UIElement)sender;

        if (attachEvents)
        {
            targetUiElement.IsKeyboardFocusWithinChanged += TargetUiElement_IsKeyboardFocusWithinChanged;
        }
        else
        {
            targetUiElement.IsKeyboardFocusWithinChanged -= TargetUiElement_IsKeyboardFocusWithinChanged;
        }
    }

    static void TargetUiElement_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var targetUiElement = (UIElement)sender;

        if (targetUiElement.IsKeyboardFocusWithin)
        {
            Selector.SetIsSelected(targetUiElement, true);
        }
    }
}
