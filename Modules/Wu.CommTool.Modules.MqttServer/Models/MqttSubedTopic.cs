namespace Wu.CommTool.Modules.MqttServer.Models;

/// <summary>
/// 订阅的主题
/// </summary>
public partial class MqttSubedTopic : ObservableObject
{
    /// <summary>
    /// 主题
    /// </summary>
    [ObservableProperty]
    string topic = string.Empty;

    /// <summary>
    /// Parent
    /// </summary>
    [ObservableProperty]
    MqttUser parent;
}
