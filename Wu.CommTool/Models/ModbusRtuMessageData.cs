using System;
using System.Collections.ObjectModel;
using Wu.CommTool.Shared.Eunms;
using Wu.CommTool.Shared.Models;

namespace Wu.CommTool.Models
{
    /// <summary>
    /// ModbusRtu界面消息展示
    /// </summary>
    public class ModbusRtuMessageData : MessageData
    {
        public ModbusRtuMessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
        {
        }

        /// <summary>
        /// 子消息
        /// </summary>
        public ObservableCollection<MessageSubContent> MessageSubContents { get => _MessageSubContents; set => SetProperty(ref _MessageSubContents, value); }
        private ObservableCollection<MessageSubContent> _MessageSubContents = new();

    }
}
