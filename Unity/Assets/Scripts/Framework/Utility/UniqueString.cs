using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 全局字符串池管理扩展
/// </summary>
public class UniqueString
{
    private static Dictionary<string, string> m_strings = new Dictionary<string, string>();

    //removable = false 意味着字符串将被添加到全局字符串池中
    //这么做有两个好处，一个是节省了内存 （重复字符串越多，内存节省量越大），另一个好处是降低了字符串比较的开销 （如果两个字符串引用一致，就不用逐字符比较内容了）
    public static string Intern(string str, bool removable = true)
    {
        if (str == null)
            return null;

        string ret = IsInterned(str);
        if (ret != null)
            return ret;

        if (removable)
        {
            m_strings.Add(str, str);
            return str;
        }
        else
        {
            return string.Intern(str);
        }
    }

    public static string IsInterned(string str)
    {
        if (str == null)
            return null;

        string ret = string.IsInterned(str);
        if (ret != null)
            return ret;

        if (m_strings.TryGetValue(str, out ret))
            return ret;

        return null;
    }

    public static void Clear()
    {
        m_strings.Clear();
    }
}
