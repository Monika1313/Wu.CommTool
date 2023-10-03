using Prism.Mvvm;

namespace Wu.CommTool.Modules.ModbusRtu.Models
{
    /// <summary>
    /// 子消息
    /// </summary>
    public class MessageSubContent : BindableBase
    {
        public MessageSubContent(string content, Enums.ModbusRtuMessageType type)
        {
            Content = content;
            Type = type;
        }
        //public MessageSubContent(string content, Brush brush)
        //{
        //    Content = content;
        //    Brush = brush;
        //}

        /// <summary>
        /// 子消息内容
        /// </summary>
        public string Content { get => _Content; set => SetProperty(ref _Content, value); }
        private string _Content = string.Empty;

        ///// <summary>
        ///// 消息展示的颜色
        ///// </summary>
        //public Brush Brush { get => _Brush; set => SetProperty(ref _Brush, value); }
        //private Brush _Brush = Brushes.Black;

        /// <summary>
        /// 消息类型
        /// </summary>
        public Enums.ModbusRtuMessageType Type { get => _Type; set => SetProperty(ref _Type, value); }
        private Enums.ModbusRtuMessageType _Type;
    }
}
