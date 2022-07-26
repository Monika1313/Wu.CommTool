namespace Wu.CommTool.ViewModels.DialogViewModels.DialogDesignViewModels
{
    public class ModbusRtuAutoSearchDeviceDesignViewModel : ModbusRtuAutoSearchDeviceViewModel
    {
        private static ModbusRtuAutoSearchDeviceDesignViewModel _Instance;
        public static ModbusRtuAutoSearchDeviceDesignViewModel Instance => _Instance ??= new();
        public ModbusRtuAutoSearchDeviceDesignViewModel()
        {

        }
    }
}
