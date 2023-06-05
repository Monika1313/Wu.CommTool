using System.Text.RegularExpressions;

namespace Wu.CommTool.Shared.Common
{
    public static class Utils
    {
        /// <summary>
        /// 判断是否仅含16进制字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsHexString(string str)
        {
            string val = str.Replace(" ", "");
            //验证字符串仅包含16进制字符 至少一个字符
            if (Regex.IsMatch(val, "^[0-9A-Fa-f]+$"))
                return true;
            return false;
        }

        /// <summary>
        /// 判断是否为16位(包含1-4位16进制字符)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool Is16BitHexString(string str)
        {
            string val = str.Replace(" ", "");
            //验证字符串仅包含1-4个16进制字符
            if (Regex.IsMatch(val, "^[0-9A-Fa-f]{1,4}$"))
                return true;
            return false;
        }

        /// <summary>
        /// 判断是否为包含32位(包含1-8位16进制字符)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool Is32BitHexString(string str)
        {
            string val = str.Replace(" ", "");
            //验证字符串仅包含1-8个16进制字符
            if (Regex.IsMatch(val, "^[0-9A-Fa-f]{1,8}$"))
                return true;
            return false;
        }

        /// <summary>
        /// 判断是否为64位(包含1-8位16进制字符)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool Is64BitHexString(string str)
        {
            string val = str.Replace(" ", "");
            //验证字符串仅包含1-16个16进制字符
            if (Regex.IsMatch(val, "^[0-9A-Fa-f]{1,16}$"))
                return true;
            return false;
        }




    }
}
