using System.Text;
using UnityEngine;

public class EditorHelper
{
    public static string AppPath
    {
        get
        {
            if (string.IsNullOrEmpty(APP_PATH))
            {
                APP_PATH = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            }
            return APP_PATH;
        }
    }
    private static string APP_PATH;

    /// <summary>
    /// 调用命令行
    /// </summary>
    /// <param name="command">命令</param>
    /// <param name="argument">参数</param>
    public static void ProcessCommand(string command, string argument)
    {
        System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo(command)
        {
            Arguments = argument,
            CreateNoWindow = true,
            ErrorDialog = true,
            UseShellExecute = false
        };
        if (start.UseShellExecute)
        {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        }
        else
        {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = Encoding.UTF8;
            start.StandardErrorEncoding = Encoding.UTF8;
        }

        System.Diagnostics.Process p = System.Diagnostics.Process.Start(start);

        if (!start.UseShellExecute)
        {
            string info = p.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(info))
            {
                Debug.Log(info);
            }
            string err = p.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(err))
            {
                Debug.LogError(err);
            }
        }

        p.WaitForExit();
        p.Close();
    }
}
