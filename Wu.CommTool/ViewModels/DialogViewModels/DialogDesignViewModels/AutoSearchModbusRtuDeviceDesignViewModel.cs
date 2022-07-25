namespace Wu.CommTool.ViewModels.DialogViewModels.DialogDesignViewModels
{
    public class AutoSearchModbusRtuDeviceDesignViewModel : AutoSearchModbusRtuDeviceViewModel
    {
        private static AutoSearchModbusRtuDeviceDesignViewModel _Instance;
        public static AutoSearchModbusRtuDeviceDesignViewModel Instance => _Instance ??= new();
        public AutoSearchModbusRtuDeviceDesignViewModel()
        {

        }
    }
}
