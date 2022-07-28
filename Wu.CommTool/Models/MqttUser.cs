using Prism.Mvvm;
using System;

namespace MqttnetServer.Model
{
    /// <summary>
    /// Mqtt客户端用户
    /// </summary>
    public class MqttUser : BindableBase
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get => _ClientId; set => SetProperty(ref _ClientId, value); }
        private string _ClientId = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get => _UserName; set => SetProperty(ref _UserName, value); }
        private string _UserName = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get => _PassWord; set => SetProperty(ref _PassWord, value); }
        private string _PassWord = string.Empty;

        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginTime { get => _LoginTime; set => SetProperty(ref _LoginTime, value); }
        private DateTime _LoginTime;

        /// <summary>
        /// 最后一次消息时间
        /// </summary>
        public DateTime LastDataTime { get => _LastDataTime; set => SetProperty(ref _LastDataTime, value); }
        private DateTime _LastDataTime;
    }
}
