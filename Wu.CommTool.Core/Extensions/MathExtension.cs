namespace Wu.CommTool.Core.Extensions;

/// <summary>
/// System.Math 扩展
/// </summary>
public static class MathExtension
{

    public static double Round2(this double value)
    {
        return Math.Round(value, 2);
    }

    public static double Round3(this double value)
    {
        return Math.Round(value, 3);
    }
}
