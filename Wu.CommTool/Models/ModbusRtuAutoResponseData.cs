using Prism.Mvvm;

namespace Wu.CommTool.Models
{
    public class ModbusRtuAutoResponseData : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }
        private string _Name = string.Empty;

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get => _Priority; set => SetProperty(ref _Priority, value); }
        private int _Priority;

        /// <summary>
        /// 匹配模板
        /// </summary>
        public string MateTemplate { get => _MateTemplate; set => SetProperty(ref _MateTemplate, value); }
        private string _MateTemplate = string.Empty;

        /// <summary>
        /// 应答模板
        /// </summary>
        public string ResponseTemplate { get => _ResponseTemplate; set => SetProperty(ref _ResponseTemplate, value); }
        private string _ResponseTemplate = string.Empty;

        /// <summary>
        /// 匹配模板是否为正则表达式
        /// </summary>
        public bool IsRegular { get => _IsRegular; set => SetProperty(ref _IsRegular, value); }
        private bool _IsRegular = false;
    }
}
