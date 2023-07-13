using System.Text;

namespace ResourceLoad
{
    public class StringUtils
    {
        private static readonly object LOCK = new object();

        public static readonly StringBuilder SB = new StringBuilder();

        public static string ToString<T>(params T[] args)
        {
            lock (LOCK)
            {
                SB.Clear();
                if (args == null)
                    return null;

                for (int i = 0; i < args.Length; i++)
                {
                    SB.Append(args[i]);
                }

                return SB.ToString();
            }
        }

        public static new string ToString()
        {
            return SB.ToString();
        }

        public static void Clear()
        {
            SB.Clear();
        }

        public static StringBuilder Append<T>(params T[] args)
        {
            lock (LOCK)
            {
                if (args == null)
                    return SB;

                for (int i = 0; i < args.Length; i++)
                {
                    SB.Append(args[i]);
                }
                return SB;
            }
        }
    }

}
