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
    /// Mqtt的主题 需要主题作为属性才能实现修改通知
    /// </summary>
    public class MqttTopic : BindableBase
    {
        public MqttTopic( string topic)
        {
            Topic = topic;
        }

        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get => _Topic; set => SetProperty(ref _Topic, value); }
        private string _Topic = string.Empty;
    }
}
