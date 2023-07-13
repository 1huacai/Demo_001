using System.Text;

namespace XCore.Utils
{
    /// <summary>
    /// 字符串合并工具
    /// </summary>
    public class TextUtils
    {
        /// <summary>
        /// 线程同步锁
        /// </summary>
        private static readonly object LOCK = new object();
        /// <summary>
        /// 公用字符串构造器
        /// </summary>
        private static readonly StringBuilder SB = new StringBuilder();

        /// <summary>
        /// 输出数组中的每个元素ToString信息，以“,”分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static string ToString<T>(T[] objs)
        {
            lock (LOCK)
            {
                return ToString(objs, 0, objs.Length, SB);
            }
        }

        /// <summary>
        /// 输出数组中的每个元素ToString信息，以“,”分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToString<T>(T[] objs, int index, int length)
        {
            lock (LOCK)
            {
                return ToString(objs, index, length, SB);
            }
        }

        /// <summary>
        /// 输出数组中的每个元素ToString信息，以“,”分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="length"></param>
        /// <param name="sb"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string ToString<T>(T[] objs, int index, int length, StringBuilder sb)
        {
            sb.Length = 0;
            if (objs == null)
            {
                return null;
            }
            for (int i = index; i < length; i++)
            {
                sb.Append(objs[i]);
                if (i < length - 1)
                {
                    sb.Append(',');
                }
            }
            return sb.ToString();
        }

        private TextUtils()
        {
        }
    }
}
