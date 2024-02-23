namespace Wu.CommTool.Modules.Message.ViewModels.Designs;

public class MsgDesignViewModel : MsgViewModel
{
    private static MsgDesignViewModel _Instance = new();
    public static MsgDesignViewModel Instance => _Instance ??= new();
    public MsgDesignViewModel()
    {

    }
}
