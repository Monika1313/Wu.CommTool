using System;
using System.Collections.ObjectModel;
using Wu.CommTool.Modules.ModbusRtu.Enums;

namespace Wu.CommTool.Modules.ModbusRtu.Models
{
    /// <summary>
    /// ModbusRtu界面消息展示
    /// </summary>
    public class ModbusRtuMessageData : MessageData
    {
        #region 构造函数

        /// <summary>
        /// modbusRtu帧
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="dateTime"></param>
        /// <param name="Type"></param>
        /// <param name="frame"></param>
        public ModbusRtuMessageData(string Content, DateTime dateTime, MessageType Type, ModbusRtuFrame frame) : base(Content, dateTime, Type, "")
        {
            ModbusRtuFrame = frame;
        } 
        #endregion

        /// <summary>
        /// Modbus帧
        /// </summary>
        public ModbusRtuFrame ModbusRtuFrame { get => _ModbusRtuFrame; set => SetProperty(ref _ModbusRtuFrame, value); }
        private ModbusRtuFrame _ModbusRtuFrame;

        /// <summary>
        /// 子消息
        /// </summary>
        public ObservableCollection<MessageSubContent> MessageSubContents { get => _MessageSubContents; set => SetProperty(ref _MessageSubContents, value); }
        private ObservableCollection<MessageSubContent> _MessageSubContents = new();

    }
}
