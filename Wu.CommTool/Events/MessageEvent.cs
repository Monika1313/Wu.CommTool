using Prism.Events;

namespace Wu.CommTool.Events
{
    /// <summary>
    /// 消息事件 事件需要继承PubSubEvent<T>
    /// </summary>
    public class MessageEvent : PubSubEvent<MessageModel>
    {
    }

    public class MessageModel
    {
        /// <summary>
        /// 过滤器
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }
}
