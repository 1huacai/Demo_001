using System;
using System.IO;
using UnityEngine;


namespace CoreFrameWork
{
    public class LogWrite
    {
        private FileStream fs;
        private StreamWriter sw;
        private static object lockObj = new object();
        private bool isEditor = true;
        private string m_logFileName = "log_{0}.txt";

        public LogWrite()
        {
            isEditor = Application.isEditor;
            try
            {
                string path = GetLogPath();
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                string filePath = path + string.Format(this.m_logFileName, DateTime.Today.ToString("yyyyMMdd"));
                fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                sw = new StreamWriter(this.fs);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("LogWrite创建访问流异常：" + e.StackTrace);
            }

            DeleteOTLog();
        }
        public void Write(string content)
        {
            lock (lockObj)
            {
                sw.WriteLine(content);
                sw.Flush();
            }
        }

        private string _logPath;
        public string GetLogPath()
        {
            if (string.IsNullOrEmpty(_logPath))
            {
                _logPath = Application.persistentDataPath + "/log/";//Framework.ResUtils.ResPath("log/",false);
            }
            return _logPath;
        }

        public void Dispose()
        {
            lock (lockObj)
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                    sw = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }
        }

        /// <summary>
        /// 删除过期的日志文件
        /// </summary>
        private void DeleteOTLog()
        {
            try
            {
                string m_logPath = GetLogPath();
                DateTime dtNow = DateTime.Now;
                int saveDay = 2;   //2天前的日志删除
                //遍历文件夹
                DirectoryInfo theFolder = new DirectoryInfo(m_logPath);
                FileInfo[] fileInfo = theFolder.GetFiles("*.txt", SearchOption.AllDirectories);
                foreach (FileInfo tmpfi in fileInfo) //遍历文件
                {
                    TimeSpan ts = dtNow.Subtract(tmpfi.LastWriteTime);
                    if (ts.TotalDays > saveDay)//超过了保存时间，删除文件
                    {
                        tmpfi.Delete();
                    }
                }

            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
            }
        }
    }

    /// <summary>
    /// unity的辅助输出工具
    /// </summary>
    public class UnityLoggerUtility : LoggerUtility
    {
        private string filterString;
        private LogWrite writer;
        private bool isEditor;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="filterStr">只显示包含此字段的输出</param>
        public UnityLoggerUtility(string filterStr = "")
        {
            filterString = filterStr;
            if (Log.S_IsWrite)
            {
                writer = new LogWrite();
            }

            isEditor = Application.isEditor;
        }

        public void Break()
        {
            UnityEngine.Debug.Break();
        }

        private const string LogTimeStr = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 白字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Debug(string message, bool output = true, bool isWrite = true)
        {
            string content = DateTime.Now.ToString(LogTimeStr) + " " + message;
            UnityEngine.Debug.Log(message);

            if (isWrite == true && writer != null)
            {
                writer.Write(content);
            }
        }

        /// <summary>
        /// 白字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Info(string message, bool output = true, bool isWrite = true)
        {
            string content = DateTime.Now.ToString(LogTimeStr) + " " + message;
            if (output)
            {
                UnityEngine.Debug.Log(message);
            }

            if (isWrite == true && writer != null)
            {
                writer.Write(content);
            }

        }
        /// <summary>
        /// 黄字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Warning(string message, bool output = true, bool isWrite = true)
        {
            string content = DateTime.Now.ToString(LogTimeStr) + " " + message;
            if (output)
            {
                UnityEngine.Debug.LogWarning(message);
            }

            if (isWrite == true && writer != null)
            {
                writer.Write(content);
            }
        }
        /// <summary>
        /// 红字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Error(string message, bool output = true, bool isWrite = true)
        {
            string content = DateTime.Now.ToString(LogTimeStr) + " " + message;
            if (output)
            {
                UnityEngine.Debug.LogError(message);
            }

            if (isWrite == true && writer != null)
            {
                writer.Write(content);
            }
        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }

        }
    }
}
