using Newtonsoft.Json;
using Prism.Mvvm;
using Wu.CommTool.Shared.Enums.Mqtt;

namespace Wu.CommTool.Modules.MqttServer.Models
{
    public class MqttServerConfig : BindableBase
    {
        /// <summary>
        /// IP
        /// </summary>
        public string ServerIp { get => _ServerIp; set => SetProperty(ref _ServerIp, value); }
        private string _ServerIp = "192.168.1.10";

        /// <summary>
        /// 端口
        /// </summary>
        public int ServerPort { get => _ServerPort; set => SetProperty(ref _ServerPort, value); }
        private int _ServerPort = 1883;

        /// <summary>
        /// 是否开启
        /// </summary>
        [JsonIgnore]
        public bool IsOpened { get => _IsOpened; set => SetProperty(ref _IsOpened, value); }
        private bool _IsOpened = false;

        /// <summary>
        /// 接收消息的格式
        /// </summary>
        public MqttPayloadType ReceivePaylodType { get => _ReceivePaylodType; set => SetProperty(ref _ReceivePaylodType, value); }
        private MqttPayloadType _ReceivePaylodType = MqttPayloadType.Plaintext;

        /// <summary>
        /// 发送消息的格式
        /// </summary>
        public MqttPayloadType SendPaylodType { get => _SendPaylodType; set => SetProperty(ref _SendPaylodType, value); }
        private MqttPayloadType _SendPaylodType = MqttPayloadType.Plaintext;
    }
}
