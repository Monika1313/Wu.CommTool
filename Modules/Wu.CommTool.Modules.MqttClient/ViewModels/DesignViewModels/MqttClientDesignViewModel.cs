namespace Wu.CommTool.Modules.MqttClient.ViewModels.DesignViewModels;

public class MqttClientDesignViewModel : MqttClientViewModel
{
    private static MqttClientDesignViewModel _Instance = new();
    public static MqttClientDesignViewModel Instance => _Instance ??= new();
    public MqttClientDesignViewModel()
    {
        IsDrawersOpen.LeftDrawer = false;
    }
}
