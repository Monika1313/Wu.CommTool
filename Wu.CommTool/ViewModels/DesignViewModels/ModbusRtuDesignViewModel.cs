namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class ModbusRtuDesignViewModel : ModbusRtuViewModel
    {
        private static ModbusRtuDesignViewModel _Instance = new();
        public static ModbusRtuDesignViewModel Instance => _Instance ??= new();
        public ModbusRtuDesignViewModel()
        {
            IsDrawersOpen.IsLeftDrawerOpen = false;
            IsDrawersOpen.IsRightDrawerOpen = false;
            ShowMessage("这是一条提示信息");
            ShowErrorMessage("这是一条错误信息");
            ShowReceiveMessage("这是一条接收的信息");
            ShowSendMessage("这是一条发送的信息");
            TransitionerIndex = 0;
            ModbusRtuFunIndex= 1;
        }
    }
}
