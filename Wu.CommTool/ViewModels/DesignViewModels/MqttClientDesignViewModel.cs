namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class MqttClientDesignViewModel : MqttClientViewModel
    {
        private static MqttClientDesignViewModel _Instance = new();
        public static MqttClientDesignViewModel Instance => _Instance ??= new();
        public MqttClientDesignViewModel()
        {
            IsDrawersOpen.IsLeftDrawerOpen = true;
            MqttClientConfig.SubscribeTopics.Add("Topic1");
            MqttClientConfig.SubscribeTopics.Add("Topic2");
            MqttClientConfig.SubscribeTopics.Add("Topic3");
            MqttClientConfig.SubscribeTopics.Add("Topic4");
            MqttClientConfig.SubscribeTopics.Add("Topic5");
        }
    }
}
