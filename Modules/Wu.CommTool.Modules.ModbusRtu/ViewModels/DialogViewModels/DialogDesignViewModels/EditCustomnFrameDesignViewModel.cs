namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels;

public class EditCustomnFrameDesignViewModel : EditCustomnFrameViewModel
{
    private static EditCustomnFrameDesignViewModel _Instance = new();
    public static EditCustomnFrameDesignViewModel Instance => _Instance ??= new();
    public EditCustomnFrameDesignViewModel()
    {

    }
}
