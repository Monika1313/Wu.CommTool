namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels;

public class CustomFrameDesignViewModel : CustomFrameViewModel
{
    private static CustomFrameDesignViewModel _Instance = new();
    public static CustomFrameDesignViewModel Instance => _Instance ??= new();
    public CustomFrameDesignViewModel()
    {
        ModbusRtuModel = new();
        OpenDrawers.LeftDrawer = false;

        ModbusRtuModel.ShowMessage("这是一条提示信息");
        ModbusRtuModel.ShowErrorMessage("这是一条错误信息");
        ModbusRtuModel.ShowMessage("这是一条发送信息", MessageType.Send);
        ModbusRtuModel.ShowSendMessage(new ModbusRtuFrame("031000000002043F8CCCCDA17D"));
        ModbusRtuModel.ShowMessage("这是一条接收信息", MessageType.Receive);
        ModbusRtuModel.ShowReceiveMessage(new ModbusRtuFrame("01030BCE0002A7D0"));
    }
}
