namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DialogViewModels.DialogDesignViewModels
{
    public class EditFrameDesignViewModel : EditFrameViewModel
    {
        private static EditFrameDesignViewModel _Instance = new();
        public static EditFrameDesignViewModel Instance => _Instance ??= new();
        public EditFrameDesignViewModel()
        {

        }
    }
}
