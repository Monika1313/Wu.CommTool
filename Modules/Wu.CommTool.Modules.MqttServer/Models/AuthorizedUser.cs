namespace Wu.CommTool.Modules.MqttServer.Model;

/// <summary>
/// Mqtt客户端用户
/// </summary>
public partial class AuthorizedUser : ObservableObject
{
    /// <summary>
    /// 用户名
    /// </summary>
    [ObservableProperty] string userName = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [ObservableProperty] string password = string.Empty;
}
