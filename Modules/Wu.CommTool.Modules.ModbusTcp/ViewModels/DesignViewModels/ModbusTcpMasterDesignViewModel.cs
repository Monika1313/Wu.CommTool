namespace Wu.CommTool.Modules.ModbusTcp.ViewModels.DesignViewModels;

public class ModbusTcpMasterDesignViewModel : ModbusTcpMasterViewModel
{
    private static ModbusTcpMasterDesignViewModel _Instance = new();
    public static ModbusTcpMasterDesignViewModel Instance => _Instance ??= new();
    public ModbusTcpMasterDesignViewModel()
    {
        MtcpMaster = new();
        OpenDrawers.LeftDrawer = false;
        MtcpMaster.ShowMessage("这是一条提示信息");
        MtcpMaster.ShowErrorMessage("这是一条错误信息");
        MtcpMaster.ShowMessage("这是一条发送信息", MessageType.Send);
        //MtcpMaster.ShowSendMessage(new ModbusRtuFrame("031000000002043F8CCCCDA17D"));
        MtcpMaster.ShowMessage("这是一条接收信息", MessageType.Receive);
        //MtcpMaster.ShowReceiveMessage(new ModbusRtuFrame("01030BCE0002A7D0"));
    }
}
