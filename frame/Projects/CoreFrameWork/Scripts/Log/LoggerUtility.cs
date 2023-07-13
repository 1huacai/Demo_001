using System.Collections;

namespace CoreFrameWork
{
    public interface LoggerUtility
    {
        void Debug(string message, bool output = true, bool isWrite = true);
        void Info(string message, bool output = true, bool isWrite = true);
        void Warning(string message, bool output = true, bool isWrite = true);
        void Error(string message, bool output = true, bool isWrite = true);
    }
}
