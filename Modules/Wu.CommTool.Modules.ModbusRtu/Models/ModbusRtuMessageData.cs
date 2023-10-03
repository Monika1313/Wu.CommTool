using System;
using System.Collections.ObjectModel;

namespace Wu.CommTool.Modules.ModbusRtu.Models
{
    /// <summary>
    /// ModbusRtu界面消息展示
    /// </summary>
    public class ModbusRtuMessageData : MessageData
    {
        public ModbusRtuMessageData(string Content, DateTime dateTime, Enums.MessageType Type = Enums.MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
        {
        }

        /// <summary>
        /// 子消息
        /// </summary>
        public ObservableCollection<MessageSubContent> MessageSubContents { get => _MessageSubContents; set => SetProperty(ref _MessageSubContents, value); }
        private ObservableCollection<MessageSubContent> _MessageSubContents = new();

    }
}
