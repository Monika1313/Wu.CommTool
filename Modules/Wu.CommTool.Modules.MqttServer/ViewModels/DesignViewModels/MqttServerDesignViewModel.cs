namespace Wu.CommTool.Modules.MqttServer.ViewModels.DesignViewModels
{
    public class MqttServerDesignViewModel : MqttServerViewModel
    {
        private static MqttServerDesignViewModel _Instance = new();
        public static MqttServerDesignViewModel Instance => _Instance ??= new();
        public MqttServerDesignViewModel()
        {
            IsDrawersOpen.LeftDrawer = true;
        }
    }
}
