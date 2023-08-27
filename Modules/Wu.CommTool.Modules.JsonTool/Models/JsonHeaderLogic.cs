using Newtonsoft.Json.Linq;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wu.CommTool.Modules.JsonTool.Models
{
    public class JsonHeaderLogic : BindableBase
    {
        //用于界面绑定的属性定义
        /// <summary>
        /// Header
        /// </summary>
        public string Header { get => _Header; set => SetProperty(ref _Header, value); }
        private string _Header;

        /// <summary>
        /// 显示值
        /// </summary>
        public string DispValue { get => _DispValue; set => SetProperty(ref _DispValue, value); }
        private string _DispValue;

        /// <summary>
        /// Name和Value显示在同一行
        /// </summary>
        public bool InLine { get => _InLine; set => SetProperty(ref _InLine, value); }
        private bool _InLine;



        public IEnumerable<JsonHeaderLogic> Children { get; private set; }

        public JToken Token { get; private set; }






        //内部构造函数，使用FromJToken来创建JsonHeaderLogic
        JsonHeaderLogic(JToken token, string header, IEnumerable<JsonHeaderLogic> children)
        {
            Token = token;
            Header = header;
            Children = children;

            //根据类型不同 显示不同的格式
            var type = Token.GetType();
            if (typeof(JProperty).IsAssignableFrom(type))
            {
                var jcontainer = (JContainer)Token;
                var jp = (JProperty)jcontainer;
                if (jp.Value.Type.Equals(JTokenType.String))
                {
                    InLine = true;
                    DispValue = $"\"{jp.Value}\"";
                }
                else if (jp.Value.Type.Equals(JTokenType.Float) || jp.Value.Type.Equals(JTokenType.Integer))
                {
                    InLine = true;
                    DispValue = $"{jp.Value}";
                }
                else if (jp.Value.Type.Equals(JTokenType.Array))
                {
                    InLine = true;
                    DispValue = $"{jp.Value.ToString().Replace("\r\n", "")}";
                }
            }

        }




        //外部的从JToken创建JsonHeaderLogic的方法
        public static JsonHeaderLogic FromJToken(JToken jtoken)
        {
            if (jtoken == null)
            {
                throw new ArgumentNullException("jtoken");
            }
            var type = jtoken.GetType();

            if (typeof(JValue).IsAssignableFrom(type))
            {
                var jvalue = (JValue)jtoken;
                var value = jvalue.Value;
                if (value == null)
                    value = "<null>";
                return new JsonHeaderLogic(jvalue, value.ToString(), null);
            }
            else if (typeof(JContainer).IsAssignableFrom(type))
            {
                var jcontainer = (JContainer)jtoken;
                var children = jcontainer.Children().Select(c => FromJToken(c));
                string header = string.Empty;

                //数组将合并显示
                bool IsMerge = typeof(JArray).IsAssignableFrom(type)
                    && children is not null
                    && (children.First().Token.Type == JTokenType.Float
                        || children.First().Token.Type == JTokenType.Integer);

                //若属性仅一个值则显示在同一行
                bool IsValue = false;

                if (typeof(JProperty).IsAssignableFrom(type))
                {
                    header = ((JProperty)jcontainer).Name;
                    var jp = (JProperty)jcontainer;
                    if (jp.Value.Type.Equals(JTokenType.String))
                    {
                        IsValue = true;
                        //header = $"{jp.Name} : \"{jp.Value}\"";
                    }
                    else if (jp.Value.Type.Equals(JTokenType.Float) || jp.Value.Type.Equals(JTokenType.Integer))
                    {
                        IsValue = true;
                        //header = $"{jp.Name} : {jp.Value}";
                    }
                    else if (jp.Value.Type.Equals(JTokenType.Array))
                    {
                        IsValue = true;
                    }
                    header = ((JProperty)jcontainer).Name;
                }
                else if (typeof(JArray).IsAssignableFrom(type))
                {
                    header = $"[ {children.Count()} ]";
                    if (IsMerge)
                    {
                        string str = string.Empty;
                        foreach (var item in children)
                        {
                            str += $"{item.Header}, ";
                        }
                        str = str[..^2];
                        header = $"[ {str} ]";
                    }
                }
                else if (typeof(JObject).IsAssignableFrom(type))
                    header = $"{{ {children.Count()} }}";
                else
                    throw new Exception("不支持的JContainer类型");

                if (IsMerge || IsValue)
                {
                    return new JsonHeaderLogic(jcontainer, header, null);
                }
                else
                {
                    return new JsonHeaderLogic(jcontainer, header, children);
                }
            }
            else
            {
                throw new Exception("不支持的JToken类型");
            }
        }
    }
}
