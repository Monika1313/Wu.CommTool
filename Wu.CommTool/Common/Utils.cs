using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Common
{
    public static class Utils
    {
        /// <summary>
        /// 将序列化的json字符串内容写入Json文件，并且保存
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="jsonConents">Json内容</param>
        public static void WriteJsonFile(string path, string jsonConents)
        {
            //清除旧的文本
            using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.SetLength(0);
            }
            using FileStream fs = new(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using StreamWriter sw = new(fs, Encoding.UTF8);
            sw.WriteLine(jsonConents);
        }

        /// <summary>
        /// 获取到本地的Json文件并且解析返回对应的json字符串
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <returns>Json内容</returns>
        public static string ReadJsonFile(string filepath)
        {
            string json = string.Empty;
            using (FileStream fs = new(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using StreamReader sr = new(fs, Encoding.UTF8);
                json = sr.ReadToEnd().ToString();
            }
            return json;
        }
    }
}
