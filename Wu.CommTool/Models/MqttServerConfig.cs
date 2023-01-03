using Newtonsoft.Json;
using Prism.Mvvm;

namespace Wu.CommTool.Models
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
    }
}
