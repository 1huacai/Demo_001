using System;
using System.Collections.Generic;
using UnityEngine;

public class AllFileMD5 : ScriptableObject
{

    //记录AB包的MD5
    public Dictionary<string, string> MD5Dic;
    public string[] Paths;
    public string[] Md5s;

    public Dictionary<string, string> ABMD5Dic;
    public string[] ABPaths;
    public string[] ABMd5s;

    public enum EMd5State
    {
        modify,
        delete,
        newObj,
        linkChild,
        fixUpdate,
        lostUpdate,//丢失重打
    }

    public void SaveABData()
    {
        if (ABMD5Dic == null || ABMD5Dic.Count == 0)
            return;

        ABPaths = new string[ABMD5Dic.Count];
        ABMd5s = new string[ABMD5Dic.Count];
        int index = 0;
        foreach (KeyValuePair<string, string> each in ABMD5Dic)
        {
            ABPaths[index] = each.Key;
            ABMd5s[index] = each.Value;
            index++;
        }
    }

    public void SaveData()
    {
        if (MD5Dic == null || MD5Dic.Count == 0)
            return;

        Paths = new string[MD5Dic.Count];
        Md5s = new string[MD5Dic.Count];
        int index = 0;
        foreach (KeyValuePair<string, string> each in MD5Dic)
        {
            Paths[index] = each.Key;
            Md5s[index] = each.Value;
            index++;
        }
    }

    public void LoadData()
    {
        if (Paths == null || Paths.Length == 0)
            return;

        MD5Dic = new Dictionary<string, string>();
        for (int i = 0; i < Paths.Length; i++)
        {
            MD5Dic.Add(Paths[i], Md5s[i]);
        }

        //
        if (ABPaths == null || ABPaths.Length == 0)
            return;

        ABMD5Dic = new Dictionary<string, string>();
        for (int i = 0; i < ABPaths.Length; i++)
        {
            ABMD5Dic.Add(ABPaths[i], ABMd5s[i]);
        }

    }
}

