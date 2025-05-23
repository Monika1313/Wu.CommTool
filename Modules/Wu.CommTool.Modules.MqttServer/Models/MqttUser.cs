namespace Wu.CommTool.Modules.MqttServer.Model;

/// <summary>
/// Mqtt客户端用户
/// </summary>
public partial class MqttUser : ObservableObject
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    [ObservableProperty] string clientId = string.Empty;

    /// <summary>
    /// 用户名
    /// </summary>
    [ObservableProperty] string userName = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [ObservableProperty] string passWord = string.Empty;

    /// <summary>
    /// 登录时间
    /// </summary>
    [ObservableProperty] DateTime loginTime;

    /// <summary>
    /// 最后一次消息时间
    /// </summary>
    [ObservableProperty] DateTime lastDataTime;

    /// <summary>
    /// 订阅的主题
    /// </summary>
    [ObservableProperty] ObservableCollection<MqttSubedTopic> mqttSubedTopics = [];
}
