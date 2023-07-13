using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;
using System.Reflection;

public class CreateLink : MonoBehaviour
{

    //创建需要移动到cache的文件列表
    public static void CreateDllList()
    {
        //string AssetsPath = EditorUtility.SaveFolderPanel("Select Floder To Bundle", "GameRes", "");
        string AssetsPath = Application.dataPath;
        string path = Application.dataPath + "/link.xml";
        if (File.Exists(path))
            File.Delete(path);
        List<string> list = null;

        if (string.IsNullOrEmpty(AssetsPath))
            return;

        list = getDirectoryFiles(new DirectoryInfo(AssetsPath+ "/Resources/Plugins"));
        FileStream fs = File.OpenWrite(path);
        int subLen = new DirectoryInfo(AssetsPath).FullName.Length;
        string[] arrs = AssetsPath.Split('/');
        string dllName = arrs[arrs.Length - 1];

        byte[] _head = System.Text.Encoding.UTF8.GetBytes("<linker>\r\n");
        fs.Write(_head, 0, _head.Length);
        foreach (string name in list)
        {
            if (name.ToUpper().Contains("EDITOR"))
            {
                continue;
            }
            if (name.ToUpper().Contains("ZLIB"))
            {
                continue;
            }
            string str = name.Substring(subLen + 1);
            arrs = str.Split('.');
            if (arrs[arrs.Length - 1] == "dll")
            {
                string[] arrs2 = arrs[0].Split('\\');
                str = arrs2[arrs2.Length - 1];
                string _path = name.Replace("\\", "/");
                Assembly _asm = Assembly.LoadFrom(@_path);
                byte[] _temp = System.Text.Encoding.UTF8.GetBytes("<assembly fullname=" + "\"" + _asm.FullName + "\"" + ">\r\n");
                fs.Write(_temp, 0, _temp.Length);
                foreach (Type type in _asm.GetTypes())
                {
                    if (type.FullName.Contains("+") || type.FullName.Contains("<")) { continue; }
                    string all = "<type fullname=\"" + type.FullName + "\" preserve=\"all\"/> \n"; ;
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(all);
                    fs.Write(data, 0, data.Length);
                }
                byte[] _temp1 = System.Text.Encoding.UTF8.GetBytes("</assembly>\r\n");
                fs.Write(_temp1, 0, _temp1.Length);

            }
        }
        byte[] _tail = System.Text.Encoding.UTF8.GetBytes("</linker>");
        fs.Write(_tail, 0, _tail.Length);
        fs.Flush();
        fs.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "生成成功！", "确定");
    }

    private static List<string> getDirectoryFiles(DirectoryInfo dir)
    {
        List<string> list = new List<string>();
        foreach (FileInfo info in dir.GetFiles())
        {
            if (info.FullName.EndsWith(".dll"))
                list.Add(info.FullName);
        }
        foreach (DirectoryInfo info in dir.GetDirectories())
        {
            if (info.Name == "GameRes")
                continue;

            list.AddRange(getDirectoryFiles(info));


        }
        return list;
    }

}
