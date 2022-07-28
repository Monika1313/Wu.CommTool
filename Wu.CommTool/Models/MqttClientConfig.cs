using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Models
{
    public class MqttClientConfig : BindableBase
    {

        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get => _ClientId; set => SetProperty(ref _ClientId, value); }
        private string _ClientId = "ClientId";

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get => _UserName; set => SetProperty(ref _UserName, value); }
        private string _UserName = "UserName";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get => _Password; set => SetProperty(ref _Password, value); }
        private string _Password = "Password";

        /// <summary>
        /// 订阅的主题
        /// </summary>
        public ObservableCollection<string> SubscribeTopics { get => _SubscribeTopics; set => SetProperty(ref _SubscribeTopics, value); }
        private ObservableCollection<string> _SubscribeTopics = new();

        /// <summary>
        /// 成功订阅的主题
        /// </summary>
        public ObservableCollection<string> SubscribeSucceeds { get => _SubscribeSucceeds; set => SetProperty(ref _SubscribeSucceeds, value); }
        private ObservableCollection<string> _SubscribeSucceeds = new();

        /// <summary>
        /// 发布的主题
        /// </summary>
        public string PublishTopic { get => _PublishTopic; set => SetProperty(ref _PublishTopic, value); }
        private string _PublishTopic = string.Empty;

        /// <summary>
        /// 消息质量等级
        /// </summary>
        public QosLevel QosLevel { get => _QosLevel; set => SetProperty(ref _QosLevel, value); }
        private QosLevel _QosLevel = QosLevel.AtMostOnce;

        /// <summary>
        /// IP
        /// </summary>
        public string ServerIp { get => _ServerIp; set => SetProperty(ref _ServerIp, value); }
        private string _ServerIp = "192.168.2.211";

        /// <summary>
        /// 端口
        /// </summary>
        public int ServerPort { get => _ServerPort; set => SetProperty(ref _ServerPort, value); }
        private int _ServerPort = 1883;

        /// <summary>
        /// 是否开启
        /// </summary>
        public bool IsOpened { get => _IsOpened; set => SetProperty(ref _IsOpened, value); }
        private bool _IsOpened = false;
    }
}
