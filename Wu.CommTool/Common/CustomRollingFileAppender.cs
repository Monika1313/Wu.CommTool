using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net.Appender;

namespace Wu.CommTool.Common
{
    /// <summary>
    /// Log4net 自定义RollingFileAppender
    /// </summary>
    public class CustomRollingFileAppender : RollingFileAppender
    {
        /// <summary>
        /// 删除模式
        /// </summary>
        public enum DeleteMode
        {
            LastWriteTime = 0,           //最后写入时间
            CreationTime = 1,            //创建时间
            LastAccessTime = 2,          //最后修改时间
        }

        public int MaximumFileCount { get; set; }

        /// <summary>
        /// 自动删除N天之前的日志
        /// </summary>
        public int OutDateDays { get; set; }

        /// <summary>
        /// 选择删除时的匹配条件
        /// </summary>
        public DeleteMode DeleteStyle { get; set; }

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            DeleteLogFilesForOutDate();
            DeleteLogFilesForOverCount();

        }

        /// <summary>
        /// 删除当前日志所在目录下的文件
        /// </summary>
        public void DeleteLogFilesForOutDate()
        {
            try
            {
                string strBasepath = Path.GetDirectoryName(File);
                foreach (string file in Directory.GetFiles(strBasepath))
                {
                    DeleteLogFiles(file, DeleteStyle);
                }
            }
            catch { }
        }

        /// <summary>
        /// 删除超过指定数量的日志
        /// </summary>
        public void DeleteLogFilesForOverCount()
        {
            try
            {
                string strBasepath = Path.GetDirectoryName(File);
                int a = MaxSizeRollBackups;
                int b = MaximumFileCount;
                int n = a > b ? a : b;

                DirectoryInfo di = new DirectoryInfo(strBasepath);
                FileInfo[] query = di.GetFiles();
                if (n >= query.Length) return;

                if (DeleteStyle == DeleteMode.CreationTime)
                {
                    var querySort = query.OrderByDescending(o => o.CreationTime).ToArray();
                    for (int i = n; i < querySort.Length; i++)
                    {
                        try
                        {
                            System.IO.File.Delete(querySort[i].FullName);
                        }
                        catch
                        {
                        }
                    }
                }
                else if (DeleteStyle == DeleteMode.LastAccessTime)
                {
                    var querySort = query.OrderByDescending(o => o.LastAccessTime).ToArray();
                    for (int i = n; i < querySort.Length; i++)
                    {
                        try
                        {
                            System.IO.File.Delete(query[i].FullName);
                        }
                        catch
                        {
                        }
                    }
                }
                else if (DeleteStyle == DeleteMode.LastWriteTime)
                {
                    var querySort = query.OrderByDescending(o => o.LastWriteTime).ToArray();
                    for (int i = n; i < querySort.Length; i++)
                    {
                        try
                        {
                            System.IO.File.Delete(querySort[i].FullName);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// 根据时间条件删除文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="mode"></param>
        private void DeleteLogFiles(string file, DeleteMode mode = DeleteMode.LastWriteTime)
        {
            try
            {
                int odd = OutDateDays;
                if (string.IsNullOrWhiteSpace(file)) return;
                if (System.IO.File.Exists(file) == false) return;

                System.IO.FileInfo fi = new FileInfo(file);
                if (mode == DeleteMode.CreationTime && fi.CreationTime < DateTime.Now.AddDays(-odd))
                {
                    System.IO.File.Delete(file);
                }
                else if (mode == DeleteMode.LastWriteTime && fi.LastWriteTime < DateTime.Now.AddDays(-odd))
                {
                    System.IO.File.Delete(file);
                }
                else if (mode == DeleteMode.LastAccessTime && fi.LastAccessTime < DateTime.Now.AddDays(-odd))
                {
                    System.IO.File.Delete(file);
                }
            }
            catch { }
        }
    }
}
