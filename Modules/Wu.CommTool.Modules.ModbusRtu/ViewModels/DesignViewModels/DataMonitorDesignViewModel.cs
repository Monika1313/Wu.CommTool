namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels
{
    public class DataMonitorDesignViewModel : DataMonitorViewModel
    {
        private static DataMonitorDesignViewModel _Instance = new();
        public static DataMonitorDesignViewModel Instance => _Instance ??= new();
        public DataMonitorDesignViewModel()
        {

        }
    }
}
