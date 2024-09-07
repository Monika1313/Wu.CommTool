namespace Wu.CommTool.Modules.MqttServer.Models;

public partial class MqttServerConfig : ObservableObject
{
    /// <summary>
    /// IP
    /// </summary>
    [ObservableProperty]
    string serverIp = "127.0.0.1";

    /// <summary>
    /// 端口
    /// </summary>
    [ObservableProperty]
    int serverPort = 1883;

    /// <summary>
    /// 是否开启
    /// </summary>
    [ObservableProperty]
    [property:JsonIgnore]
    bool isOpened = false;

    /// <summary>
    /// 接收消息的格式
    /// </summary>
    [ObservableProperty]
    MqttPayloadType receivePaylodType = MqttPayloadType.Plaintext;

    #region 发布消息设置
    /// <summary>
    /// 发送消息的格式
    /// </summary>
    [ObservableProperty]
    MqttPayloadType sendPaylodType = MqttPayloadType.Plaintext;

    /// <summary>
    /// 发布的主题
    /// </summary>
    [ObservableProperty]
    string publishTopic = string.Empty;

    /// <summary>
    /// 消息质量等级
    /// </summary>
    [ObservableProperty]
    QosLevel qosLevel = QosLevel.AtMostOnce;

    /// <summary>
    /// 是否为保留消息
    /// </summary>
    [ObservableProperty]
    bool isRetain;
    #endregion

    #region 加密设置
    /// <summary>
    /// SSL/TLS加密
    /// </summary>
    [ObservableProperty]
    bool encrypt;

    /// <summary>
    /// PFX证书
    /// </summary>
    [ObservableProperty]
    string pfxFilePath;

    /// <summary>
    /// PFX证书密码
    /// </summary>
    [ObservableProperty]
    string pfxPassword;



    ///// <summary>
    ///// PEM证书
    ///// </summary>
    //[ObservableProperty]
    //string pemPath;

    ///// <summary>
    ///// KEY
    ///// </summary>
    //[ObservableProperty]
    //string keyPath;
    #endregion
}
