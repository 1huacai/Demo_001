using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BuildVSProj
{
    public static bool Start(string projectDir)
    {
        string vsPath = getVSPath();
        if (string.IsNullOrEmpty(vsPath) || !vsPath.EndsWith("devenv.exe"))
            return false;
        if (File.Exists("log.txt"))
            File.Delete("log.txt");
        try
        {
            string _args = @"F:\MyProject\Int\Demo\Demo.sln /rebuild Debug /project " + projectDir + " /out log.txt";
            using (Process vsProc = Process.Start(vsPath, _args))
            {
                vsProc.StartInfo.RedirectStandardError = true;
                vsProc.StartInfo.RedirectStandardOutput = true;
                vsProc.StartInfo.UseShellExecute = false;
                vsProc.StartInfo.CreateNoWindow = true;
                vsProc.Start();
                vsProc.WaitForExit();
            }
            return true;
            //Debug.Log(File.ReadAllText("log.txt", Encoding.GetEncoding("GB2312")));
            //File.Delete("log.txt");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
        }
        return false;
    }

    private static string getVSPath()
    {
        string vsPath;
        vsPath = PlayerPrefs.GetString("BuildGameProj_VsPath", "");
        if (vsPath.Length < 1 || !File.Exists(vsPath))
        {
            if (EditorUtility.DisplayDialog("设置VS运行文件[devenv.exe]的路径", @"VS2017参考：C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE", "ok", "cancel"))
            {
                vsPath = EditorUtility.OpenFilePanel("Open Visual Studio ----选择VS的运行文件----名为: [devenv.exe]", "", "*.exe");
                PlayerPrefs.SetString("BuildGameProj_VsPath", vsPath);
            }
        }
        return vsPath;
    }

    public static void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.ActiveEditorTracker));
        var type = assembly.GetType("UnityEditorInternal.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}