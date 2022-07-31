namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class AboutDesignViewModel : AboutViewModel
    {
        private static AboutDesignViewModel _Instance = new();
        public static AboutDesignViewModel Instance => _Instance ??= new();
        public AboutDesignViewModel()
        {

        }
    }
}
