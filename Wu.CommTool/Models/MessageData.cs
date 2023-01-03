using Prism.Mvvm;
using System;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// 消息数据
    /// </summary>
    public class MessageData : BindableBase
    {
        public MessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info)
        {
            this.Type = Type;
            this.Content = Content;
            this.Time = dateTime;
        }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get => _Time; set => SetProperty(ref _Time, value); }
        private DateTime _Time;

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType Type { get => _Type; set => SetProperty(ref _Type, value); }
        private MessageType _Type;

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get => _Content; set => SetProperty(ref _Content, value); }
        private string _Content = string.Empty;
    }
}
