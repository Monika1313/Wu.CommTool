namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels
{
    public class CustomFrameDesignViewModel : CustomFrameViewModel
    {
        private static CustomFrameDesignViewModel _Instance = new();
        public static CustomFrameDesignViewModel Instance => _Instance ??= new();
        public CustomFrameDesignViewModel()
        {

        }
    }
}
