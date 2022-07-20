using Prism.Mvvm;

namespace Wu.ComTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        private string _Title = "串口调试工具";

        public MainWindowViewModel()
        {

        }
    }
}
