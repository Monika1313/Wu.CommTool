using Wu.CommTool.Models;

namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class MqttClientDesignViewModel : MqttClientViewModel
    {
        private static MqttClientDesignViewModel _Instance = new();
        public static MqttClientDesignViewModel Instance => _Instance ??= new();
        public MqttClientDesignViewModel()
        {
            IsDrawersOpen.IsLeftDrawerOpen = true;
            IsDrawersOpen.IsRightDrawerOpen = false;
            MqttClientConfig.SubscribeTopics.Add(new MqttTopic("Topic1"));
            MqttClientConfig.SubscribeTopics.Add(new MqttTopic("Topic2"));
            MqttClientConfig.SubscribeTopics.Add(new MqttTopic("Topic3"));
            MqttClientConfig.SubscribeTopics.Add(new MqttTopic("Topic4"));
            MqttClientConfig.SubscribeTopics.Add(new MqttTopic("Topic5"));
            MqttClientConfig.SubscribeSucceeds.Add("Topic1");
        }
    }
}
