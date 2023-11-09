using Wu.CommTool.Modules.ModbusRtu.Models;

namespace Wu.CommTool.Modules.ModbusRtu.ViewModels.DesignViewModels
{
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
            ModbusRtuModel.ShowReceiveMessage("这是一条接收的信息", new ModbusRtuFrame("01030BCE0002A7D0").GetmessageWithErrMsg());
            ModbusRtuModel.ShowSendMessage("这是一条发送的信息", new ModbusRtuFrame("031000000002043F8CCCCDA17D").GetmessageWithErrMsg());
        }
    }
}
