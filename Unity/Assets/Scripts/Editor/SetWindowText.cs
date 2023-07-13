using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEditor;

[InitializeOnLoad]
public class SetWindowText
{
    #region WIN32API
    delegate bool EnumWindowsCallBack(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32", CharSet = CharSet.Unicode)]
    static extern bool SetWindowTextW(IntPtr hwnd, string title);
    [DllImport("user32")]
    static extern int EnumWindows(EnumWindowsCallBack lpEnumFunc, IntPtr lParam);
    [DllImport("user32")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr lpdwProcessId);
    #endregion
    static IntPtr myWindowHandle;
    static SetWindowText()
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;

    }
    void OnDestroy()
    {
        EditorApplication.update -= Update;
    }
    public static void Update()
    {

        IntPtr handle = (IntPtr)System.Diagnostics.Process.GetCurrentProcess().Id;  //获取进程ID
        EnumWindows(new EnumWindowsCallBack(EnumWindCallback), handle);     //枚举查找本窗口
        SetWindowTextW(myWindowHandle, Application.unityVersion+"    " + GetPlatform() + "   工程路径：" +Application.dataPath+"     进程Pid："+ System.Diagnostics.Process.GetCurrentProcess().Id); //设置窗口标题
    }

    static bool EnumWindCallback(IntPtr hwnd, IntPtr lParam)
    {
        IntPtr pid = IntPtr.Zero;
        GetWindowThreadProcessId(hwnd, ref pid);
        if (pid == lParam)  //判断当前窗口是否属于本进程
        {
            myWindowHandle = hwnd;
            return false;
        }
        return true;
    }
    static string GetPlatform()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
          return "IOS";
#else
        return "PC";
#endif
    }
}