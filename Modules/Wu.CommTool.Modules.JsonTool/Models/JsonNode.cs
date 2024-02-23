namespace Wu.CommTool.Modules.JsonTool.Models;

/// <summary>
/// Json格式化
/// </summary>
public class JsonNode : BindableBase
{
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

    /// <summary>
    /// 是否展开子项 默认展开
    /// </summary>
    public bool IsExpand { get => _IsExpand; set => SetProperty(ref _IsExpand, value); }
    private bool _IsExpand = true;


    /// <summary>
    /// 类型
    /// </summary>
    public JNodeType JNodeType { get => _JNodeType; set => SetProperty(ref _JNodeType, value); }
    private JNodeType _JNodeType = JNodeType.JProperty;


    public IEnumerable<JsonNode> Children { get; private set; }

    public JToken Token { get; private set; }





    #region 构造函数
    //内部构造函数，使用FromJToken来创建JsonHeaderLogic
    JsonNode(JToken token, string header, IEnumerable<JsonNode> children)
    {
        Token = token;
        //var xx = Token.Type;
        Header = header;
        Children = children;

        //根据类型不同 显示不同的格式
        var type = Token.GetType();

        if (typeof(JObject).IsAssignableFrom(type))
        {
            JNodeType = JNodeType.JObject;
        }
        else if (typeof(JProperty).IsAssignableFrom(type))
        {
            JNodeType = JNodeType.JProperty;
        }
        else if (typeof(Array).IsAssignableFrom(type))
        {
            JNodeType = JNodeType.JArray;
        }
        else
        {
            JNodeType = JNodeType.JValue;
        }


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
    #endregion







    //通过JToken实例化
    public static JsonNode FromJToken(JToken jtoken)
    {
        //TODO 当数组值为空时 会异常

        if (jtoken == null)
            throw new ArgumentNullException("jtoken");

        var type = jtoken.GetType();//获取JToken的类型

        /* type的可能值
         * JObject: 表示 JSON 对象。例如，{"key": "value"}。
         * JArray: 表示 JSON 数组。例如，[1, 2, 3]。
         * JValue: 表示 JSON 中的基本值，如字符串、数字、布尔值等。例如，"hello" 或 42。
         * JProperty: 表示 JSON 对象中的属性。例如，"key": "value" 中的 "key" 和 "value"。
         */

        //JValue 基本值 如 1 "1" 
        if (typeof(JValue).IsAssignableFrom(type))
        {
            var jvalue = (JValue)jtoken;
            var value = jvalue.Value;
            value ??= "<null>";
            return new JsonNode(jvalue, value.ToString(), null);
        }
        //JContainer含多种类型
        else if (typeof(JContainer).IsAssignableFrom(type))
        {
            var jcontainer = (JContainer)jtoken;
            var children = jcontainer.Children().Select(c => FromJToken(c));
            string header = string.Empty;

            //Todo 若数组内容不是JValue 则需要再分子节点
            //数组将合并显示
            bool IsMerge = typeof(JArray).IsAssignableFrom(type)
                && children is not null
                && (children.First().Token.Type == JTokenType.Float
                    || children.First().Token.Type == JTokenType.Integer);

            //若属性仅一个值则显示在同一行
            bool IsValue = false;

            //JProperty  如"key": "value"
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
            //JArray 如[1,2,3]
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
            //JObject 如{}
            else if (typeof(JObject).IsAssignableFrom(type))
                header = $"{{ {children.Count()} }}";
            else
                throw new Exception("不支持的JContainer类型");

            //终端节点
            if (IsMerge || IsValue)
            {
                return new JsonNode(jcontainer, header, null);
            }
            //仍有子节点
            else
            {
                return new JsonNode(jcontainer, header, children);
            }
        }
        else
        {
            throw new Exception("不支持的JToken类型");
        }
    }
}
