
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Wu.CommTool.Shared.Common
{
    public static class Utils
    {
        /// <summary>
        /// 判断是否为16进制字符串
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static bool IsHexString(string hexString)
        {
            //十六进制发送时，发送框数据进行十六进制数据正则校验
            if (Regex.IsMatch(hexString, "^[0-9A-Fa-f]+$"))
            {
                //校验成功
                return true;
            }
            //校验失败
            return false;
        }
    }
}
