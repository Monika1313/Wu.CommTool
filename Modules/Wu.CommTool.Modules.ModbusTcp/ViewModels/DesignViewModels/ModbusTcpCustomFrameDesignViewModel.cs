namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DesignViewModels
{
    public class ModbusTcpCustomFrameDesignViewModel : ModbusTcpCustomFrameViewModel
    {
        private static ModbusTcpCustomFrameDesignViewModel _Instance = new();
        public static ModbusTcpCustomFrameDesignViewModel Instance => _Instance ??= new();
        public ModbusTcpCustomFrameDesignViewModel()
        {

        }
    }
}
