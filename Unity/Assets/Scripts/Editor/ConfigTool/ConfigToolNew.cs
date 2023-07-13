using CoreFrameWork;
using FrameWork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using T4Template.ConfigTemplates;
using UnityEditor;
using UnityEngine;

public class ConfigToolNew
{
    public static bool BuildDllAfterConver = EditorPrefs.GetBool("BuildDllAfterConver", false);
    const string LANGUAGE_KEY = "wfeonoi@jfowKIIks&ixFUCK!jj!";
    public static string Need_Auto_Bytes = "NeedAutoBytes";
    private static byte[] _key;
    private static byte[] Key
    {
        get
        {
            if (_key == null)
                _key = Encoding.UTF8.GetBytes(LANGUAGE_KEY);
            return Encoding.UTF8.GetBytes(LANGUAGE_KEY);
        }
    }
    public static bool GetAutoBytes()
    {
        return PlayerPrefs.GetInt(Need_Auto_Bytes) == 1 ? true : false;
    }
    public static void SetAutoBytes()
    {
        if (PlayerPrefs.GetInt(Need_Auto_Bytes) == 0)
            PlayerPrefs.SetInt(Need_Auto_Bytes, 1);
        else
            PlayerPrefs.SetInt(Need_Auto_Bytes, 0);
    }
    [MenuItem("Tools/ConfigTools/转换CS后自动编译")]
    public static void TrigerAutoBuild()
    {
        BuildDllAfterConver = !BuildDllAfterConver;
        EditorPrefs.SetBool("BuildDllAfterConver", BuildDllAfterConver);
        Menu.SetChecked("Tools/ConfigTools/转换CS后自动编译", BuildDllAfterConver);
    }

    [MenuItem("Tools/ConfigTools/转换CS后自动编译", true)]
    public static bool TrigerAutoBuildCheck()
    {
        BuildDllAfterConver = EditorPrefs.GetBool("BuildDllAfterConver", false);
        Menu.SetChecked("Tools/ConfigTools/转换CS后自动编译", BuildDllAfterConver);
        return true;
    }

    [MenuItem("Tools/ConfigTools/转换ProjectConfig.cs")]
    public static void ConvertDemoConfig()
    {
        try
        {
            GinerateCS(@"../Project/", false);
            EditorUtility.DisplayProgressBar("Building", "Building Project.dll...", 1.0f);
            if (BuildDllAfterConver)
            {
                if (BuildVSProj.Start(@"../Project/Project.csproj"))
                {
                    AssetDatabase.Refresh();
                    GinerateConfig2Bytes();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    public static void GinerateBattleCS()
    {
        try
        {
            GinerateCS(@"../BattleSystem/", true);//ThreeKingdoms/Config/
            EditorUtility.DisplayProgressBar("Building", "Building BattleSystem.dll...", 1.0f);
            if (BuildDllAfterConver)
            {
                if (BuildVSProj.Start(@"../BattleSystem/BattleSystem.csproj"))
                {
                    AssetDatabase.Refresh();
                    GinerateConfig2Bytes();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    [MenuItem("Tools/ConfigTools/转换Txt为Bytes")]
    public static void GinerateConfig2Bytes()
    {
        try
        {
            ConfigTxt2BytesInDirectory(Path.Combine(Application.dataPath, "Resources/Config"));

        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("提示", "转配置失败!", "确定");
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    [MenuItem("Tools/ConfigTools/战斗配置表查重")]
    public static void FilterBattleConfig()
    {
        try
        {
            FilterDuplicateID(Path.Combine(Application.dataPath, "Resources/Config/BattleSystem/Bullets"));
            FilterDuplicateID(Path.Combine(Application.dataPath, "Resources/Config/BattleSystem/Effects"));
            FilterDuplicateID(Path.Combine(Application.dataPath, "Resources/Config/BattleSystem/Skills"));
            Debug.Log("检查完毕！");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    /*[MenuItem("Tools/ConfigTools/测试转换Txt为Bytes")]
    public static void GinerateBattleSingleConfigBytes()
    {
        try
        {
            ConfigTxt2BytesInDirectory(Path.Combine(Application.dataPath, "Resources/Config/Game/AgreeMentConfig.txt"));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }*/

    public static void GinerateLang2Bytes()
    {
        //var key = Encoding.UTF8.GetBytes(LANGUAGE_KEY);
        Lang.InitLanguage((Resources.Load<TextAsset>("Config/Language/Language").text));
        BinaryBuffer bf = new BinaryBuffer(10 * 1024 * 1024);//10MB
        //bf.Write(key);
        bf.Write(Lang.LanguageDic.Count);
        foreach (var item in Lang.LanguageDic)
        {
            bf.Write(item.Key);
            bf.Write(item.Value);
        }
        //bf.Encrypt(key);
        //byte[] temp = new byte[bf.Position];
        //temp[0] = (byte)(key.Length >> 8);
        //temp[1] = (byte)(key.Length);
        //Buffer.BlockCopy(key, 0, temp, 2, key.Length);
        //Buffer.BlockCopy(bf.Buffer, key.Length + 2, temp, key.Length + 2, temp.Length - key.Length - 2);
        //File.WriteAllBytes("Assets/Resources/ConfigByte/Game/Language.bytes", temp);
        File.WriteAllBytes("Assets/Resources/ConfigByte/Language/Language.bytes", Encrypt(bf));
        Lang.InitLanguagePackage((Resources.Load<TextAsset>("Config/Language/LanguagePackage").text));
        bf.Position = 0;
        //bf.Write(key);
        bf.Write(Lang.LanguagePackageDic.Count);
        foreach (var item in Lang.LanguagePackageDic)
        {
            bf.Write(item.Key);
            bf.Write(item.Value);
        }
        //bf.Encrypt(key);
        //temp = new byte[bf.Position];
        //temp[0] = (byte)(key.Length >> 8);
        //temp[1] = (byte)(key.Length);
        //Buffer.BlockCopy(key, 0, temp, 2, key.Length);
        //Buffer.BlockCopy(bf.Buffer, key.Length + 2, temp, key.Length + 2, temp.Length - key.Length - 2);
        //File.WriteAllBytes("Assets/Resources/ConfigByte/Game/LanguagePackage.bytes", temp);
        File.WriteAllBytes("Assets/Resources/ConfigByte/Language/LanguagePackage.bytes", Encrypt(bf));

        AssetDatabase.Refresh();
    }

    private static byte[] Encrypt(BinaryBuffer bf)
    {
        var key = Key;
        byte[] temp = new byte[bf.Position + key.Length + 2];
        temp[0] = (byte)(key.Length >> 8);
        temp[1] = (byte)(key.Length);
        Buffer.BlockCopy(key, 0, temp, 2, key.Length);
        bf.Encrypt(key);
        Buffer.BlockCopy(bf.Buffer, 0, temp, key.Length + 2, bf.Position);
        return temp;
    }

    private static void GinerateCS(string cspath, bool convertDouble)
    {
        var configCSFiles = Directory.GetFiles(cspath, "*Config.CS", SearchOption.AllDirectories);
        int count = 0;
        for (int i = 0; i < configCSFiles.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Hold on", "GinerateCS...", (float)i / configCSFiles.Length);
            var csContent = File.ReadAllText(configCSFiles[i]).Replace(" []", "[]");
            var match = Regex.Match(csContent, @"(?<=class\s)\w+Config(?=\s*\:.*Config)");
            if (!match.Success)
                continue;
            var classnameindex = match.Index;
            var classname = match.Value;
            StringBuilder cssb = new StringBuilder(2048);
            if (!csContent.Contains("using CoreFrameWork;"))
                cssb.Append("using CoreFrameWork;");
            if (convertDouble)
            {
                using (StringReader sr = new StringReader(csContent))
                {
                    while (sr.Peek() > 0)
                    {
                        var temp = sr.ReadLine();
                        match = Regex.Match(temp, @"public\s+readonly\s+double[\[\]]*|public\s+readonly\s+List<\s*double\s*>");
                        if (match.Success)
                            temp = temp.Replace(match.Value, match.Value.Replace("double", "VarDouble").Replace("[]", "A"));
                        cssb.AppendLine(temp);
                    }
                }
                csContent = cssb.ToString();
            }
            else
            {
                cssb.Append(csContent);
            }
            Type type = Type.GetType("BattleSystem." + classname + ",BattleSystem");
            if (type == null)
                type = Type.GetType("Project." + classname + ",Project");
            if (type == null)
            {
                Debug.LogError("无法在程序集中找到类型：" + classname + "，是不是没有更新程序集？");
                continue;
            }
            match = Regex.Match(csContent, @"(#region GinerateByTool)[\s\S]+(#endregion GinerateByTool!!)");
            if (match.Success)
                cssb.Replace(match.Value, new ConfigTemplate(type, convertDouble).TransformText());
            else
            {
                int temp = 0;
                for (int j = classnameindex; j < csContent.Length; j++)
                {
                    if (csContent[j] == '{')
                    {
                        temp++;
                    }
                    else if (csContent[j] == '}')
                    {
                        temp--;
                        if (temp == 0)
                        {
                            temp = j;
                            break;
                        }
                    }
                }
                cssb.Insert(temp, new ConfigTemplate(type, convertDouble).TransformText() + "\r\n    ");
            }
            File.WriteAllText(configCSFiles[i], cssb.ToString());
            count++;
        }
        Debug.Log("Total convert:" + count);
    }

    internal static void ConfigTxt2BytesInDirectory(string path)
    {
        string[] allPath;
        if (path.EndsWith(".txt"))
            allPath = new string[1] { path };
        else
            allPath = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
        if (allPath.Length == 0)
        {
            Debug.LogError("can not find config txt files in directory:" + path);
            return;
        }
        //配置表转二进制
        ConfigTxt2BytesInDirectory(allPath);
        //语言表转二进制
        GinerateLang2Bytes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        for (int i = 0; i < allPath.Length; i++)
        {
            var bytefile = allPath[i].Replace("\\", "/").Replace("/Config/", "/ConfigByte/").Replace(".txt", ".bytes");

            //加密
            if (bytefile.Contains("BattleSystem/") || bytefile.Contains("Game/") || bytefile.Contains("Language/"))
            {
                File.WriteAllBytes(bytefile, EncryptyFile(File.ReadAllBytes(bytefile), 0xDC));
            }

        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 前两位为配置表id个数，后边2位为此字段占位长度...
    /// </summary>
    /// <param name="allPath"></param>
    internal static void ConfigTxt2BytesInDirectory(IList<string> allPath)
    {
        //Lang.InitLanguage(Resources.Load<TextAsset>("Config/Language/Language").text);
        var tableCharArray = "\t".ToCharArray();
        BinaryBuffer bf = new BinaryBuffer(1024 * 1024);//1MB
        for (int i = 0; i < allPath.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Hold on", "GinerateBytes...", (float)i / allPath.Count);
            var bytefile = allPath[i].Replace("\\", "/").Replace("/Config/", "/ConfigByte/").Replace(".txt", ".bytes");
            if (!Directory.Exists(Path.GetDirectoryName(bytefile)))
                Directory.CreateDirectory(Path.GetDirectoryName(bytefile));
            string assemblyName = "Project";
            var filename = Path.GetFileNameWithoutExtension(allPath[i]);
            var typefullname = filename + ",Project";
            var cs_type = Type.GetType(typefullname);
            if (filename == "ConfigLevelVar")
                cs_type = Type.GetType("FrameWork.ConfigLevelVar,FrameWork");
            if (cs_type == null)
            {
                typefullname = assemblyName + "." + typefullname;
                cs_type = Type.GetType(typefullname);
                if (cs_type == null)
                {
                    if (assemblyName == "Project")
                        cs_type = Type.GetType(typefullname.Replace("Project", "FrameWork"));
                    if (cs_type == null)
                    {
                        if (bytefile.Contains("Language/"))
                            continue;
                        Debug.Log("can not find type:" + filename + ",path" + allPath[i]);
                        continue;
                    }
                }
            }
            cs_type.GetFields(BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Public).GroupBy(t => t.Name).Where(t => t.Count() > 1).All(t => { Debug.LogError(string.Format("[{0}]中定义了和父类重复的字段：{1}", cs_type.FullName, t.Key)); return true; });
            var txtlines = File.ReadAllLines(allPath[i]);
            string[] fieldName = txtlines[2].Split(tableCharArray, StringSplitOptions.RemoveEmptyEntries);
            if (!fieldName.Contains("id"))
            {
                throw new Exception("Config必须要有id字段!!然而[" + filename + "]中没有！！");
            }
            HashSet<long> allId = new HashSet<long>();
            short count = 0;
            bf.Position = 2;
            int backupPosition = 0;
            for (int j = 3; j < txtlines.Length; j++)
            {
                var fieldData = txtlines[j].Split(tableCharArray);
                if (fieldData.Length != fieldName.Length)
                {
                    Debug.LogError("field data length not matched in line:" + (j + 1) + " in file:" + Path.GetFileName(allPath[i]) + "\n" + txtlines[j]);
                    //break;
                }
                if (string.IsNullOrEmpty(fieldData[0]))
                    continue;
                fieldData[0] = fieldData[0].Replace("_", "-");
                long _id = -1;
                if (!long.TryParse(fieldData[0], out _id))
                {
                    Debug.LogError(filename + "的id配错了，id=" + fieldData[0]);
                }
                if (!allId.Add(_id))
                    Debug.LogError(filename + "中包含重复id=" + long.Parse(fieldData[0]));
                backupPosition = bf.Position;
                bf.Position += 2;//预留一个short位
                var len = Write2Bytes(cs_type, fieldName, fieldData, bf);
                bf.Position = backupPosition;
                bf.Write(len);
                bf.Position += len;
                count++;
            }
            backupPosition = bf.Position;
            //byte[] temp = new byte[bf.Position];
            bf.Position = 0;
            bf.Write(count);
            //Buffer.BlockCopy(bf.Buffer, 0, temp, 0, temp.Length);
            //File.WriteAllBytes(bytefile, temp);
            bf.Position = backupPosition;


            byte[] _array = Encrypt(bf);

            File.WriteAllBytes(bytefile, _array);
        }
    }

    private static short Write2Bytes(Type type, string[] fieldName, string[] fieldData, BinaryBuffer bf)
    {
        int temp = bf.Position;
        Config instance = type.Assembly.CreateInstance(type.FullName) as Config;
        if (instance == null)
        {
            Debug.LogError("can not create config instance with name:" + type.FullName);
            return 0;
        }
        for (int j = 0; j < fieldData.Length; j++)
        {
            try
            {
                FieldInfo fieldInfo = type.GetField(fieldName[j], BindingFlags.Instance | BindingFlags.GetField | BindingFlags.Public);
                if (fieldInfo == null)
                {
                    Debug.LogWarning(string.Format("Field [{0}] is not declared in {1}", fieldName[j], type.Name));
                    continue;
                }
                Type fieldType = fieldInfo.FieldType;
                object o;
                //处理字段被赋初值的数据
                if (fieldData[j].Equals(""))
                {
                    o = fieldInfo.GetValue(instance);
                    if (o != null || IsConfigClass(fieldType))
                    {
                        fieldInfo.SetValue(instance, o);
                        continue;
                    }
                }
                if (fieldData[j].Contains("@") && !fieldData[j].Contains("@level:"))
                {
                    string tempstr = "";
                    fieldData[j] = fieldData[j].Replace("@", "");
                    string[] str = fieldData[j].Split('+');
                    for (int a = 0; a < str.Length; a++)
                    {
                        tempstr += Lang.getLangValue(str[a]);
                        if (a != str.Length - 1)
                            tempstr += '+';
                    }
                    fieldData[j] = tempstr;
                }
                o = ParseConfigField(instance, fieldData[j], fieldType);
                fieldInfo.SetValue(instance, o);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString() + " type:" + type.Name + " id:" + fieldData[0]);
                throw;
            }
        }
        try
        {
            instance.Serialize(bf);
            temp = bf.Position - temp;
        }
        catch (Exception e)
        {
            Debug.LogError(" type:" + type.Name + " id:" + fieldData[0]);
            throw;
        }
        return (short)temp;
    }

    public static object ParseConfigField(Config instance, string data, Type t)
    {
        //处理非数组类型数据
        if (!t.IsArray)
        {
            //处理list泛型
            if (t.IsGenericType)
            {
                Type ttl = t.GetGenericArguments()[0];
                Type tl = typeof(List<>).MakeGenericType(ttl);
                if (data.Equals(""))
                    return Activator.CreateInstance(tl);
                IList listConfig = (IList)Activator.CreateInstance(tl);
                string[] strData = data.Split('+');
                object o;
                for (int i = 0; i < strData.Length; i++)
                {
                    o = _parseBase(strData[i], ttl);
                    if (o == null)
                        return null;
                    listConfig.Add(o);
                }
                return listConfig;
            }
            //修改基本类型数据
            return _parseBase(data, t);
        }
        //字符串数据转换为指定类型数组数据
        if (t.Name.Contains("[][][]"))
        {
            Type tt3 = t.GetElementType().GetElementType().GetElementType();
            if (data.Equals(""))
                return Array.CreateInstance(t.GetElementType(), 0);

            string[] line = data.Split('&');
            string[][][] str3 = new string[line.Length][][];
            for (int i = 0; i < line.Length; i++)
            {
                var v2 = line[i].Split('|');
                str3[i] = new string[v2.Length][];
                for (int a = 0; a < v2.Length; a++)
                {
                    str3[i][a] = v2[a].Split('+');
                }
            }
            Array arrConfig3 = Array.CreateInstance(t.GetElementType(), line.Length);
            object odata;
            for (int a = 0; a < line.Length; a++)
            {
                Array arrConfig2 = Array.CreateInstance(t.GetElementType().GetElementType(), line[a].Split('|').Length);
                for (int i = 0; i < str3[a].Length; i++)
                {
                    Array tempElem = Array.CreateInstance(tt3, str3[a][i].Length);
                    for (int j = 0; j < str3[a][i].Length; j++)
                    {
                        odata = _parseBase(str3[a][i][j], tt3);
                        if (odata == null)
                            return null;
                        tempElem.SetValue(odata, j);
                    }
                    arrConfig2.SetValue(tempElem, i);
                }
                arrConfig3.SetValue(arrConfig2, a);
            }
            return arrConfig3;
        }
        else if (t.Name.Contains("[][]"))
        {
            Type tt2 = t.GetElementType().GetElementType();
            if (data.Equals(""))
                return Array.CreateInstance(t.GetElementType(), 0);
            string[] line = data.Split('|');
            string[][] str2 = new string[line.Length][];
            for (int i = 0; i < line.Length; i++)
                str2[i] = line[i].Split('+');

            Array arrConfig2 = Array.CreateInstance(t.GetElementType(), line.Length);
            object odata;
            for (int i = 0; i < line.Length; i++)
            {
                Array tempElem = Array.CreateInstance(tt2, str2[i].Length);
                for (int j = 0; j < str2[i].Length; j++)
                {
                    odata = _parseBase(str2[i][j], tt2);
                    if (odata == null)
                        return null;
                    tempElem.SetValue(odata, j);
                }
                arrConfig2.SetValue(tempElem, i);
            }
            return arrConfig2;
        }
        if (t.Name.Contains("[]"))
        {
            Type tt1 = t.GetElementType();
            if (data.Equals(""))
                return Array.CreateInstance(tt1, 0);
            string[] str1 = data.Split('+');
            int n = str1.Length;
            Array arrConfig1 = Array.CreateInstance(tt1, n);
            object odata;
            for (int i = 0; i < n; i++)
            {
                odata = _parseBase(str1[i], tt1);
                if (odata == null)
                    return null;
                arrConfig1.SetValue(odata, i);
            }
            return arrConfig1;
        }
        return null;
    }

    private static object _parseBase(string data, Type type)
    {
        //字符串数据转换为指定类型枚举数据
        if (type.IsEnum)
            return data.Equals("") ? Enum.GetNames(type).GetValue(0) : Enum.Parse(type, data);
        //处理自定义类型数据
        if (IsConfigClass(type))
        {
            long id;
            if (!long.TryParse(data, out id))
            {
                Log.Error("引用的Config类型" + type.Name + "的id（" + data + "）格式有误，返回空值");
                return null;
            }
            string typename = type.FullName;
            if (type.IsAbstract)
            {
                var alltypes = type.Assembly.GetExportedTypes();
                for (int i = 0; i < alltypes.Length; i++)
                {
                    if (!alltypes[i].IsAbstract && (alltypes[i].BaseType == type || alltypes[i].BaseType.BaseType == type))
                    {
                        typename = alltypes[i].FullName;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(typename))
                {
                    Debug.LogError("cannot find a class inherit from class " + type.Name);
                    return null;
                }
            }
            Config c = (Config)type.Assembly.CreateInstance(typename);
            c.ref_SetField_Inst_Public("id", id);
            //c.GetType().GetField("id").SetValue(c, id);
            return c;
        }
        if (data.Equals(""))
            return type.Name.Equals("String") ? "" : type.Assembly.CreateInstance(type.FullName);
        try
        {
            switch (type.Name)
            {
                case "String":
                    return data;
                case "Byte":
                    return byte.Parse(data);
                case "SByte":
                    return sbyte.Parse(data);
                case "Int16":
                    data = data.Replace('_', '-');
                    return short.Parse(data);
                case "UInt16":
                    data = data.Replace('_', '-');
                    return ushort.Parse(data);
                case "Int32":
                    data = data.Replace('_', '-');
                    return int.Parse(data);
                case "UInt32":
                    data = data.Replace('_', '-');
                    return uint.Parse(data);
                case "Int64":
                    data = data.Replace('_', '-');
                    return long.Parse(data);
                case "UInt64":
                    data = data.Replace('_', '-');
                    return ulong.Parse(data);
                case "Char":
                    return char.Parse(data);
                case "Boolean":
                    return data.ToLower() == "true" || data == "1";
                case "Single":
                    data = data.Replace('_', '-');
                    return float.Parse(data);
                case "Double":
                    data = data.Replace('_', '-');
                    return double.Parse(data);
                case "VarDouble":
                    return VarDouble.Pare(data);
                case "VarDoubleA":
                    return VarDoubleA.Pare(data);
                case "VarDoubleAA":
                    return VarDoubleAA.Pare(data);
            }
        }
        catch (Exception)
        {
            return null;
        }
        return null;
    }

    public static bool IsConfigClass(Type type)
    {
        if (type.IsInterface)
            return false;
        while (type != typeof(object))
        {
            if (type == typeof(Config))
                return true;
            type = type.BaseType;
        }
        return false;
    }

    public static void FilterDuplicateID(string directoryPath)
    {
        var allPath = Directory.GetFiles(directoryPath, "*.txt", SearchOption.AllDirectories);
        if (allPath.Length == 0)
            return;
        Dictionary<long, string> temp = new Dictionary<long, string>();

        for (int i = 0; i < allPath.Length; i++)
        {
            var path = allPath[i];
            var alllines = File.ReadAllLines(path);
            for (int j = 3; j < alllines.Length; j++)
            {
                if (string.IsNullOrEmpty(alllines[j]))
                    continue;
                var idstring = alllines[j].Substring(0, alllines[j].IndexOf("\t"));
                var id = long.Parse(idstring);
                try
                {
                    temp.Add(id, Path.GetFileName(path));
                }
                catch (Exception)
                {
                    Debug.LogError(string.Format("id={0} 同时存在于配置表 {1} {2}", id, temp[id], Path.GetFileName(path)));
                }
            }
        }
    }



    private static byte[] EncryptyFile(byte[] array, int code)
    {


        return AssetBundleTool.EncryptyFile(array, code);
    }
}

public static class ConfigExtends
{
    public static void ImportStrInfo<T>(this ConfigManager manager, string path) where T : Config
    {
        manager.ImportBinaryInfo<T>(UnityEngine.Resources.Load<TextAsset>(path.Replace("\\", "/").Replace("/Config/", "/ConfigByte/").Replace(".txt", ".bytes")).bytes);
    }
}

public class ProcessTxtAutomatic : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (ConfigToolNew.GetAutoBytes())
        {
            //List<string> paths = new List<string>();
            //paths.AddRange(importedAssets.Where(t => t.Contains("Resources/Config/") && t.EndsWith(".txt")));
            //paths.AddRange(movedAssets.Where(t => t.Contains("Resources/Config/") && t.EndsWith(".txt")));
            List<string> paths = importedAssets.Where(t => t.Contains("Resources/Config/") && t.EndsWith(".txt")).Concat(movedAssets.Where(t => t.Contains("Resources/Config/") && t.EndsWith(".txt"))).ToList();
            if (paths.Count > 0)
            {
                //    foreach (var item in paths)
                //    {
                //        using (var strem = File.Open(item, FileMode.Open))
                //        {
                //            if (ConfigEncoding.getEncoding(strem, Encoding.Default) != Encoding.UTF8)
                //            {
                //                needGen = true;
                //                break;
                //            }
                //        }
                //    }

                // if (needGen)
                //{
                //    ConfigEncoding.Conver();

                //}

                try
                {
                    ConfigToolNew.FilterBattleConfig();
                    ConfigToolNew.GinerateLang2Bytes();
                    //ConfigToolNew.ConfigTxt2BytesInDirectory(paths);
                    ConfigToolNew.GinerateConfig2Bytes();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}