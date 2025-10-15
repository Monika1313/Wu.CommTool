namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels
{
    public class EditUartCustomnFrameDesignViewModel : EditUartCustomnFrameViewModel
    {
        private static EditUartCustomnFrameDesignViewModel _Instance = new();
        public static EditUartCustomnFrameDesignViewModel Instance => _Instance ??= new();
        public EditUartCustomnFrameDesignViewModel()
        {

        }
    }
}
