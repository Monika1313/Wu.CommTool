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

namespace MqttnetServer.Model
{
    public class MqttUser : BindableBase
    {
        public string clientId { get; set; }
        public string userName { get; set; }
        public string passWord { get; set; }


        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginTime { get => _LoginTime; set => SetProperty(ref _LoginTime, value); }
        private DateTime _LoginTime;

        /// <summary>
        /// definity
        /// </summary>
        public DateTime LastDataTime { get => _LastDataTime; set => SetProperty(ref _LastDataTime, value); }
        private DateTime _LastDataTime;
    }
}
