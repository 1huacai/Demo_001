using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
public class InjiectExample : EditorWindow
{
    private static string m_iFixDllPath = "IFixDll";
    private static string m_pluginDllPath = "Assets/Resources/Plugins";

    private static string[] m_allDll = new string[]{
       "Demo.dll",
       "BattleSystem.dll"
    };

    //[MenuItem("InjectFix/拷贝dll文件")]
    //static void CloneInjectFixDll()
    //{
    //    string _fiToPath = Path.Combine(System.Environment.CurrentDirectory, m_iFixDllPath);
    //    string _fiFromPath = Path.Combine(System.Environment.CurrentDirectory, m_pluginDllPath);
    //    CopyFile(_fiToPath, _fiFromPath);
    //    Debug.LogError(string.Format("从{0}备份dll到{1}成功", _fiFromPath, _fiToPath));
    //}
    [MenuItem("InjectFix/测试热更代码")]
    static void RevertInjectFixDll()
    {
        string _fiToPath = Path.Combine(System.Environment.CurrentDirectory, m_pluginDllPath);
        string _fiFromPath = Path.Combine(System.Environment.CurrentDirectory, m_iFixDllPath);

        CopyFile(_fiToPath, _fiFromPath);
        Debug.LogError(string.Format("从{0}还原dll到{1}成功", _fiFromPath, _fiToPath));
        AssetDatabase.Refresh();
    }

    private static void CopyFile(string _fiToPath, string _fiFromPath)
    {
        if (!Directory.Exists(_fiToPath))
        {
            Directory.CreateDirectory(_fiToPath);
        }
        FileInfo _from;
        for (int i = 0; i < m_allDll.Length; i++)
        {
            _from = new FileInfo(Path.Combine(_fiFromPath, m_allDll[i]));
            _from.CopyTo(Path.Combine(_fiToPath, m_allDll[i]), true);
        }
    }
    private static string def = "INJECTFIX";
    [MenuItem("InjectFix/添加宏定义")]
    static void AddInjectFix()
    {
        string injectfix = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetTarget());
        if (injectfix.Equals(string.Empty))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetTarget(), def);
        }
        else if (!injectfix.Contains(def))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetTarget(), injectfix + ";" + def);
        }
        AssetDatabase.Refresh();
    }
    private static BuildTargetGroup GetTarget()
    {

#if UNITY_ANDROID
        return BuildTargetGroup.Android;
#endif

#if UNITY_IPHONE
     return BuildTargetGroup.iOS;
#endif


#if UNITY_STANDALONE_WIN
        return BuildTargetGroup.Standalone;
#endif
        return BuildTargetGroup.Standalone;
    }
    [MenuItem("InjectFix/删除宏定义")]
    static void RemoveXlua()
    {
        string injectfix = PlayerSettings.GetScriptingDefineSymbolsForGroup(GetTarget());
        int _index = injectfix.IndexOf(def);
        if (_index < 0)
        {
            return;
        }
        if (_index > 0)//如果不在第一个  把前边的分号删掉
        {
            _index -= 1;
        }
        int _length = def.Length;
        if (injectfix.Length > _length)//如果长度大于当前长度，才有分号
        {
            _length += 1;
        }
        injectfix = injectfix.Remove(_index, _length);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(GetTarget(), injectfix);
        AssetDatabase.Refresh();
    }
    [MenuItem("InjectFix/删除patch")]
    private static void GinerateCS()
    {
        GinerateCS(@"../Demo/", false);
        GinerateCS(@"../BattleSystem/", false);
    }

    private static void GinerateCS(string cspath, bool convertDouble)
    {
        var configCSFiles = Directory.GetFiles(cspath, "*.CS", SearchOption.AllDirectories);
        int count = 0;
        for (int i = 0; i < configCSFiles.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Hold on", "GinerateCS...", (float)i / configCSFiles.Length);
            //var csContent = File.ReadAllText(configCSFiles[i]).Replace(" []", "[]");
            //var match = Regex.Match(File.ReadAllText(configCSFiles[i]), @"\[IFix.Patch\]");
            //if (match.Success)
            //{
            //    var test = Regex.Replace(File.ReadAllText(configCSFiles[i]), @"\[IFix.Patch\]", "");
            //    File.WriteAllText(configCSFiles[i], test);
            //}
            Regex reg = new Regex(@"\[IFix.(.+)\]");
            Match match1 = reg.Match(File.ReadAllText(configCSFiles[i]));
            if (match1.Success)
            {
                var test = Regex.Replace(File.ReadAllText(configCSFiles[i]), @"\[IFix.(.+)\]", "");
                File.WriteAllText(configCSFiles[i], test);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }
}