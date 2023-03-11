using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
