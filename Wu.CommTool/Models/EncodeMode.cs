using Prism.Mvvm;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// 编码模式
    /// </summary>
    public class EncodeMode : BindableBase
    {
        public EncodeMode()
        {

        }


        /// <summary>
        /// Id
        /// </summary>
        public int Id { get => _Id; set => SetProperty(ref _Id, value); }
        private int _Id;


    }
}
