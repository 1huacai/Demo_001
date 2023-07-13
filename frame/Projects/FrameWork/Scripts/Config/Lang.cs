using CoreFrameWork;
using FrameWork;
using System;
using System.Collections.Generic;
public class Lang
{
    public static Dictionary<string, string> LanguageDic;
    public static Dictionary<string, string> LanguagePackageDic;

    public static void InitLanguage(string str)
    {
        LanguageDic = new Dictionary<string, string>();
        ParsetLang(str, LanguageDic);
    }

    public static void InitLanguagePackage(string str)
    {
        LanguagePackageDic = new Dictionary<string, string>();
        ParsetLang(str, LanguagePackageDic);
    }

    public static void InitLanguage(byte[] bytes)
    {
        BinaryBuffer bf = new BinaryBuffer(bytes);
        PraseLanguage(ref LanguageDic, bf);
    }

    public static void InitLanguagePackage(byte[] bytes)
    {
        BinaryBuffer bf = new BinaryBuffer(bytes);
        PraseLanguage(ref LanguagePackageDic, bf);
    }

    private static void PraseLanguage(ref Dictionary<string, string> dic, BinaryBuffer bf)
    {
        if (bf == null)
            return;
        byte[] decrpyt_key;
        bf.Read(out decrpyt_key);
        bf.Decrypt(decrpyt_key, bf.Position);
        var len = bf.ReadInt32();
        if (dic == null)
            dic = new Dictionary<string, string>(len);
        else
            dic.Clear();
        string key;
        string value;
        for (int i = 0; i < len; i++)
        {
            key = bf.ReadString();
            value = bf.ReadString();
            dic.Add(key, value);
        }
    }

    private static void ParsetLang(string lang, Dictionary<string, string> dir)
    {
        string[] lines = lang.Split('\n');
        char[] splitChar = { '=' };
        for (int i = 0, max = lines.Length; i < max; ++i)
        {
            lines[i] = lines[i].Trim();
            if (lines[i].StartsWith("//") || string.IsNullOrEmpty(lines[i]))
                continue;
            string[] items = lines[i].Split(splitChar, 2);
            if (items.Length < 2)
            {
                Log.Warning(lines[i]);
                continue;
            }
            string key = items[0].Trim();
            if (dir.ContainsKey(key))
                Log.Warning("配置出错，已经包含Key为" + key + "的文本配置！");
            else
                dir.Add(key, items[1].Replace("\\n", "\n"));
        }
    }


    /// <summary>
    /// 获取程序用语言配置 -> LanguagePackage.txt
    ///</summary>
    public static string get(string key, params object[] str)
    {
        if (LanguagePackageDic != null && LanguagePackageDic.ContainsKey(key))
        {
            string _temp = LanguagePackageDic[key];
            try
            {
                if (null != str && str.Length > 0)
                {
                    return string.Format(_temp, str);
                }
                else
                {
                    return _temp;
                }
            }
            catch (Exception e)
            {
                Log.Error(key);
                Log.Error(e.ToString());
                return key + "Exception";
            }
        }
        else
        {
            return "(" + key + ")cannot find";
        }
    }

    /// <summary>
    /// 获取策划用语言配置 -> Language.txt
    ///</summary>
    public static string getLang(string key, params object[] str)
    {
        if (LanguageDic != null && LanguageDic.ContainsKey(key))
        {
            string _temp = LanguageDic[key];
            if (null != str && str.Length > 0)
            {
                return string.Format(_temp, str);
            }
            else
            {
                return _temp;
            }
        }
        else
        {
            return get(key, str);
        }
    }

    /// <summary>
    /// 获取策划用语言配置 -> Language.txt
    ///</summary>
    public static string getLangValue(string key)
    {
        if (LanguageDic != null && LanguageDic.ContainsKey(key))
        {
            return LanguageDic[key];
        }
        else
        {
            return "(" + key + ")cannot find";
        }
    }

    public static bool IsExistLanguagePackageKey(string key)
    {
        if (LanguagePackageDic != null && LanguagePackageDic.ContainsKey(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool IsExistLanguageKey(string key)
    {
        if (LanguageDic != null && LanguageDic.ContainsKey(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static void Clear()
    {
        if (LanguageDic != null) LanguageDic.Clear();
        if (LanguagePackageDic != null) LanguagePackageDic.Clear();
    }
}
