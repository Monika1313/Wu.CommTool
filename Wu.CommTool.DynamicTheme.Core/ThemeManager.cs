using System.Windows;

namespace Wu.CommTool.DynamicTheme.Core;

/// <summary>
/// 动态主题管理
/// 参考视频 https://www.bilibili.com/video/BV1FN41eHE7e
/// </summary>
public class ThemeManager
{
    private readonly Dictionary<string, ResourceDictionary> themes = [];

    /// <summary>
    /// 注册主题
    /// </summary>
    /// <param name="themeName">主题名</param>
    /// <param name="assemblyName">包名</param>
    /// <param name="resourcePath">资源文件名</param>
    public void RegisterTheme(string themeName, string assemblyName, string resourcePath)
    {
        string uri = $"/{assemblyName};component/{resourcePath}";
        ResourceDictionary resource = new()
        {
            Source = new Uri(uri, UriKind.RelativeOrAbsolute)
        };
        themes.Add(themeName, resource);
    }

    /// <summary>
    /// 应用主题
    /// </summary>
    /// <param name="themeName"></param>
    public void ApplyTheme(string themeName)
    {
        ResourceDictionary resource = themes[themeName];
        //先清空已经添加的主题
        foreach (var x in themes)
        {
            Application.Current.Resources.MergedDictionaries.Remove(x.Value);
        }
        Application.Current.Resources.MergedDictionaries.Add(resource);
    }
}
