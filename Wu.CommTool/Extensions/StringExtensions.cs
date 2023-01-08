using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 每隔n个字符插入一个字符
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <param name="interval">间隔字符数</param>
        /// <param name="value">待插入值</param>
        /// <returns>返回新生成字符串</returns>
        public static string InsertFormat(this string input, int interval, string value)
        {
            for (int i = interval; i < input.Length; i += interval + 1)
                input = input.Insert(i, value);
            return input;
        }
    }
}
