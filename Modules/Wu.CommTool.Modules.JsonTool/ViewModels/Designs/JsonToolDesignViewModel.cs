namespace Wu.CommTool.Modules.JsonTool.ViewModels.Designs
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
