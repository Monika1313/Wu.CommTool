﻿namespace Wu.CommTool.Modules.MqttClient.Models;

public partial class MqttClientConfig : ObservableObject
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    [ObservableProperty]
    private string clientId = "ClientId";

    /// <summary>
    /// 用户名
    /// </summary>
    [ObservableProperty]
    private string userName = "UserName";

    /// <summary>
    /// 密码
    /// </summary>
    [ObservableProperty]
    private string password = "Password";

    /// <summary>
    /// 订阅的主题
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MqttTopic> subscribeTopics = [];

    /// <summary>
    /// 成功订阅的主题
    /// </summary>
    [ObservableProperty]
    [property:JsonIgnore]
    private ObservableCollection<string> subscribeSucceeds = [];

    /// <summary>
    /// 发布的主题
    /// </summary>
    [ObservableProperty]
    private string publishTopic = string.Empty;

    /// <summary>
    /// 消息质量等级
    /// </summary>
    [ObservableProperty]
    private QosLevel qosLevel = QosLevel.AtMostOnce;

    /// <summary>
    /// 是否为保留消息
    /// </summary>
    [ObservableProperty]
    private bool isRetain;

    /// <summary>
    /// 服务器IP
    /// </summary>
    [ObservableProperty]
    private string serverIp = "192.168.1.10";

    /// <summary>
    /// 服务器端口
    /// </summary>
    [ObservableProperty]
    private int serverPort = 1883;

    /// <summary>
    /// 是否开启
    /// </summary>
    [ObservableProperty]
    [property:JsonIgnore]
    private bool isOpened = false;

    /// <summary>
    /// 自动重新连接
    /// </summary>
    [ObservableProperty]
    private bool autoReconnect;

    /// <summary>
    /// 接收消息的格式
    /// </summary>
    [ObservableProperty]
    private MqttPayloadType receivePaylodType = MqttPayloadType.Plaintext;

    /// <summary>
    /// 发送消息的格式
    /// </summary>
    [ObservableProperty]
    private MqttPayloadType sendPaylodType = MqttPayloadType.Plaintext;

    #region 加密
    /// <summary>
    /// SSL/TLS加密
    /// </summary>
    [ObservableProperty]
    bool encrypt;

    /// <summary>
    /// CA文件
    /// </summary>
    [ObservableProperty]
    string caFile;
    #endregion

}
