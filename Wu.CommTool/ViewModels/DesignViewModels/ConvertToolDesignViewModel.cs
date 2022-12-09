namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class ConvertToolDesignViewModel : ConvertToolViewModel
    {
        private static ConvertToolDesignViewModel _Instance = new();
        public static ConvertToolDesignViewModel Instance => _Instance ??= new();
        public ConvertToolDesignViewModel()
        {
            
        }
    }
}
