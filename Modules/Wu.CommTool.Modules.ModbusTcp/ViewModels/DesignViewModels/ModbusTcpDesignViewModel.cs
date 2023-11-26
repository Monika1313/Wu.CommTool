namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DesignViewModels
{
    public class ModbusTcpDesignViewModel : ModbusTcpViewModel
    {
        private static ModbusTcpDesignViewModel _Instance = new();
        public static ModbusTcpDesignViewModel Instance => _Instance ??= new();
        public ModbusTcpDesignViewModel()
        {

        }
    }
}
