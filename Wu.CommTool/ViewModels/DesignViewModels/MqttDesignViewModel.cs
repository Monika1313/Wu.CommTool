namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class MqttDesignViewModel : MqttViewModel
    {
        private static MqttDesignViewModel _Instance = new();
        public static MqttDesignViewModel Instance => _Instance ??= new();
        public MqttDesignViewModel()
        {

        }
    }
}
