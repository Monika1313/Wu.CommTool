namespace Wu.CommTool.ViewModels.DialogViewModels.DialogDesignViewModels
{
    public class ModbusRtuAutoResponseDataEditDesignViewModel : ModbusRtuAutoResponseDataEditViewModel
    {
        private static ModbusRtuAutoResponseDataEditDesignViewModel _Instance = new();
        public static ModbusRtuAutoResponseDataEditDesignViewModel Instance => _Instance ??= new();
        public ModbusRtuAutoResponseDataEditDesignViewModel()
        {

        }
    }
}
