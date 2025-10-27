namespace Wu.CommTool.Modules.MrtuSlave.ViewModels.DesignViewModels;

public class MrtuSlaveDesignViewModel : MrtuSlaveViewModel
{
    private static MrtuSlaveDesignViewModel _Instance = new();
    public static MrtuSlaveDesignViewModel Instance => _Instance ??= new();
    public MrtuSlaveDesignViewModel()
    {
        OpenDrawers.LeftDrawer = false;
    }
}
