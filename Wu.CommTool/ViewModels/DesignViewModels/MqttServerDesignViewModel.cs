using MqttnetServer.Model;

namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class MqttServerDesignViewModel : MqttServerViewModel
    {
        private static MqttServerDesignViewModel _Instance = new();
        public static MqttServerDesignViewModel Instance => _Instance ??= new();
        public MqttServerDesignViewModel()
        {
            //IsDrawersOpen.IsLeftDrawerOpen = true;
            ShowMessage("这是一条提示信息");
            ShowErrorMessage("这是一条错误信息");
            ShowReceiveMessage("这是一条接收到的信息");
            ShowSendMessage("这是一条发送的信息");

            IsDrawersOpen.IsRightDrawerOpen = true;
            MqttUsers.Add(new MqttUser());
        }
    }
}
