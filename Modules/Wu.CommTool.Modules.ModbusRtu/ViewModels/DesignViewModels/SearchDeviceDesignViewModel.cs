namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels
{
    public class SearchDeviceDesignViewModel : SearchDeviceViewModel
    {
        private static SearchDeviceDesignViewModel _Instance = new();
        public static SearchDeviceDesignViewModel Instance => _Instance ??= new();
        public SearchDeviceDesignViewModel()
        {

        }
    }
}
