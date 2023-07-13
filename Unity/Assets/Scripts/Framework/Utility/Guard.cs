using System;
using System.Collections.Generic;


/// <summary>
/// 守卫
/// </summary>
public static class Guard
{
    /// <summary>
    /// 断言异常
    /// </summary>
    /// <param name="expr">时候异常</param>
    /// <param name="msg">异常描述</param>
    /// <returns></returns>
    [System.Diagnostics.DebuggerNonUserCode]
    public static bool AssertException(bool expr, string msg)
    {
#if DEBUG
        System.Diagnostics.Debug.Assert(expr);
        return expr;
#else
        if (expr)
            return true;

        throw new Exception(msg);
#endif
    }

    /// <summary>
    /// 验证一个条件,并在该协定的条件失败时引发异常。
    /// </summary>
    /// <typeparam name="TException">异常</typeparam>
    /// <param name="condition">条件</param>
    [System.Diagnostics.DebuggerNonUserCode]
    public static void Requires<TException>(bool condition) where TException : Exception, new()
    {
        if (condition)
        {
            return;
        }
        throw new TException();
    }

    /// <summary>
    /// 不为空或者null
    /// </summary>
    /// <param name="argumentValue">参数值</param>
    /// <param name="argumentName">参数名</param>
    [System.Diagnostics.DebuggerNonUserCode]
    public static void NotEmptyOrNull(string argumentValue, string argumentName)
    {
        if (string.IsNullOrEmpty(argumentValue))
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    /// <summary>
    /// 长度大于0
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="argumentValue">参数值</param>
    /// <param name="argumentName">参数名</param>
    [System.Diagnostics.DebuggerNonUserCode]
    public static void CountGreaterZero<T>(IList<T> argumentValue, string argumentName)
    {
        if (argumentValue.Count <= 0)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    /// <summary>
    /// 元素部位空或者null
    /// </summary>
    /// <param name="argumentValue">参数值</param>
    /// <param name="argumentName">参数名</param>
    [System.Diagnostics.DebuggerNonUserCode]
    public static void ElementNotEmptyOrNull(IList<string> argumentValue, string argumentName)
    {
        foreach (var val in argumentValue)
        {
            if (string.IsNullOrEmpty(val))
            {
                throw new ArgumentNullException(argumentName, "Argument element can not be Empty or Null.");
            }
        }
    }

    /// <summary>
    /// 内容不为空
    /// </summary>
    /// <param name="argumentValue">参数值</param>
    /// <param name="argumentName">参数名</param>
    [System.Diagnostics.DebuggerNonUserCode]
    public static void NotNull(object argumentValue, string argumentName)
    {
        if (argumentValue == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }
}
