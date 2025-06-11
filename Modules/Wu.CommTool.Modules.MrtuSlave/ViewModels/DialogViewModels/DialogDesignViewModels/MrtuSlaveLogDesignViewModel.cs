namespace Wu.CommTool.Modules.MrtuSlave.ViewModels.DialogViewModels.DialogDesignViewModels;

public class MrtuSlaveLogDesignViewModel : MrtuSlaveLogViewModel
{
    private static MrtuSlaveLogDesignViewModel _Instance = new();
    public static MrtuSlaveLogDesignViewModel Instance => _Instance ??= new();
    public MrtuSlaveLogDesignViewModel()
    {

    }
}
