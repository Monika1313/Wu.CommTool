namespace Wu.CommTool.ViewModels.DesignViewModels
{
    public class JsonToolDesignViewModel : JsonToolViewModel
    {
        private static JsonToolDesignViewModel _Instance;
        public static JsonToolDesignViewModel Instance => _Instance ??= new JsonToolDesignViewModel();
        public JsonToolDesignViewModel()
        {

        }
    }
}
