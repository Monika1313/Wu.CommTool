namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class MqttClientDesignViewModel : MqttClientViewModel
    {
        private static MqttClientDesignViewModel _Instance;
        public static MqttClientDesignViewModel Instance => _Instance ??= new();
        public MqttClientDesignViewModel()
        {

        }
    }
}
