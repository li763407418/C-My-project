using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tool
{
    class LogHelper
    {
        #region 日志记录
        public class LogRecode
        {
            static object _Sync = new object();
            /// <summary>
            /// 
            /// </summary>
            /// <param name="logLevel">日志类型</param>
            /// <param name="message">日志信息</param>
            public static void WriteLog(LogLevel logLevel, string message)
            {

                switch (logLevel)
                {
                    case LogLevel.Debug:
                        RecodeLog(message, @"D:\调试日志");
                        break;
                    case LogLevel.Error:
                        RecodeLog(message, @"D:\错误日志");
                        break;
                    case LogLevel.Info:
                        RecodeLog(message, @"D:\提示日志");
                        break;
                    case LogLevel.Warning:
                        RecodeLog(message, @"D:\警告日志");
                        break;
                }

            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="logText">文本信息</param>
            /// <param name="path">日志路径</param>
            public static void RecodeLog(string logText, string path)
            {

                string fileName = DateTime.Now.ToString("yyyyMMddHHmm") + ".txt";

                StringBuilder filePath = new StringBuilder();
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                filePath.Append(string.Format(@"{0}\{1}", path, fileName));

                lock (_Sync)
                {
                    using (FileStream fs = new FileStream(filePath.ToString(), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter m_streamWriter = new StreamWriter(fs))
                        {
                            logText = "[" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss fff") + "] " + logText;
                            m_streamWriter.WriteLine(logText);
                            m_streamWriter.Flush();
                            m_streamWriter.Close();
                        }
                    }
                }
            }

            public enum LogLevel
            {
                Error,
                Debug,
                Warning,
                Info
            }

        }

        #endregion
    }
}
