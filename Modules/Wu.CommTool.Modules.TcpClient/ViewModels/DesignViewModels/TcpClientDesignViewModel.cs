namespace Wu.CommTool.Modules.TcpClient.ViewModels.DesignViewModels
{
    public class TcpClientDesignViewModel : TcpClientViewModel
    {
        private static TcpClientDesignViewModel _Instance = new();
        public static TcpClientDesignViewModel Instance => _Instance ??= new();
        public TcpClientDesignViewModel()
        {

        }
    }
}
