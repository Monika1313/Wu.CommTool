namespace Wu.CommTool.Modules.ModbusTcp.Models;

/// <summary>
/// 有时间限制的集合,超时自动踢掉
/// </summary>
/// <typeparam name="T"></typeparam>
public class TimeLimitedList<T> : List<T>
{

    public new void Add(T item)
    {

    }
}
