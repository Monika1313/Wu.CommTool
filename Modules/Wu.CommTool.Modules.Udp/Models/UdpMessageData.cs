using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Modules.Udp.Models;

public partial class UdpMessageData : MessageData
{
    public UdpMessageData(string Content, DateTime dateTime, MessageType Type = MessageType.Info, string Title = "") : base(Content, dateTime, Type, Title)
    {
    }


    [ObservableProperty] string remoteEndPoint;




}
