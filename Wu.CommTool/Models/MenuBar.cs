namespace Wu.CommTool.Models;

/// <summary>
/// 系统导航菜单
/// </summary>
public class MenuBar : BindableBase
{
    private string? icon;
    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? Icon
    {
        get { return icon; }
        set { icon = value; }
    }

    private string? title;
    /// <summary>
    /// 菜单名称
    /// </summary>
    public string? Title
    {
        get { return title; }
        set { title = value; }
    }

    private string? nameSpace;
    /// <summary>
    /// 菜单命名空间
    /// </summary>
    public string? NameSpace
    {
        get { return nameSpace; }
        set { nameSpace = value; }
    }

}
