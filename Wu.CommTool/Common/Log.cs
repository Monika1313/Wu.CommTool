using System;
using log4net;
using log4net.Config;
using log4net.Repository;
using System.IO;

namespace Wu.CommTool.Common
{
    /// <summary>
    /// 日志
    /// </summary>
    public static class Log
    {
        private static ILoggerRepository repository { get; set; }
        private static ILog _Loginfo;
        private static ILog _Logerror;

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="repositoryName">仓库名</param>
        /// <param name="configFile">配置文件路径</param>
        public static void Configure(string repositoryName = "LogRepository", string configFile = "Configs/Log4netConfig/log4net.config")
        {
            repository = LogManager.CreateRepository(repositoryName);
            XmlConfigurator.Configure(repository, new FileInfo(configFile));
            _Logerror = LogManager.GetLogger(repositoryName, "logerror");
            _Loginfo = LogManager.GetLogger(repositoryName, "loginfo");
        }

        /// <summary>
        /// 信息日志
        /// </summary>
        private static ILog LogInfo
        {
            get
            {
                if (_Loginfo == null)
                {
                    Configure();
                }
                return _Loginfo;
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        private static ILog LogError
        {
            get
            {
                if (_Logerror == null)
                {
                    Configure();
                }
                return _Logerror;
            }
        }



        public static void Info(string msg)
        {
            LogInfo.Info(msg);
        }

        public static void Warn(string msg)
        {
            LogInfo.Warn(msg);
        }

        public static void Error(string msg)
        {
            LogError.Error(msg);
        }
        public static void Info(object ex)
        {
            LogInfo.Info(ex);
        }

        public static void Debug(object message, Exception ex)
        {
            LogInfo.Debug(message, ex);
        }

        public static void Warn(object message, Exception ex)
        {
            LogInfo.Warn(message, ex);
        }

        public static void Error(object message, Exception ex)
        {
            LogError.Error(message, ex);
        }

        public static void LogErrorInfo(Exception ex, object message)
        {
            LogError.Error(message, ex);
        }
        public static void Info(object message, Exception ex)
        {
            LogInfo.Info(message, ex);
        }
    }
}
