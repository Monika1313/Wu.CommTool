namespace Wu.CommTool.Models;

/// <summary>
/// 系统导航菜单
/// </summary>
public partial class MenuBar : ObservableObject
{
    /// <summary>
    /// 菜单图标
    /// </summary>
    [ObservableProperty]
    string? icon;

    /// <summary>
    /// 菜单名称
    /// </summary>
    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    /// <summary>
    /// 菜单命名空间
    /// </summary>
    private string? nameSpace;
}
