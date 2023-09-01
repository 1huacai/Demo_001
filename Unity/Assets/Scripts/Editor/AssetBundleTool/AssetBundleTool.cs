using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Object = UnityEngine.Object;
using FrameWork.Scripts.ResourceLoad;
/// <summary>
/// 
/// </summary>
public class AssetBundleTool : MonoBehaviour
{
    public static codeType CodeType = codeType.injectFix;
    private static List<string> m_folderPathList = new List<string>();

    public static void Init()
    {
        m_folderPathList.Clear();
        LabelDic.Clear();
        AllLabelList.Clear();
    }

    public static Object CreatePrefab(string prefabName, string path)
    {
        GameObject _go = new GameObject(prefabName);
        Object _targetPrefab = PrefabUtility.CreatePrefab(path, _go);//4.7版本 这个函数会随机报错
        Debug.Log("创建预制体：" + prefabName);
        DestroyImmediate(_go);
        return _targetPrefab;
    }


    public static string GetResourceName(string resPathName)
    {
        if (string.IsNullOrEmpty(resPathName))
        {
            return resPathName;
        }
        int index = resPathName.LastIndexOf("/");
        if (index == -1)
            return resPathName.Split('.')[0];
        else
        {
            string _name = resPathName.Substring(index + 1, resPathName.Length - index - 1);
            return _name.Split('.')[0];
        }
    }
    public static string GetResourcePath(string resPathName)
    {
        if (string.IsNullOrEmpty(resPathName))
        {
            return resPathName;
        }
        int index = resPathName.LastIndexOf("/");
        if (index == -1)
            return string.Empty;
        else
        {
            return resPathName.Substring(0, index + 1);
        }
    }


    /// <summary>
    /// 是否有后缀名
    /// </summary>
    /// <param name="resPathName"></param>
    /// <returns></returns>
    public static bool HasSuffix(string resPathName)
    {
        int index = resPathName.LastIndexOf(".");
        return index != -1;
    }

    public static string GetFileSuffix(string resPathName)
    {
        int index = resPathName.LastIndexOf(".");
        if (index == -1)
            return resPathName;
        else
        {
            string _name = resPathName.Substring(index, resPathName.Length - index);
            return _name;
        }
    }
    /// <summary>
    /// 删除后缀
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string DeletSuffix(string resPathName)
    {
        int index = resPathName.LastIndexOf(".");
        if (index == -1)
            return resPathName;
        else
        {
            string _name = resPathName.Substring(0, index);
            return _name;
        }
    }

    /// <summary>
    /// 通过资源路径得到资源
    /// </summary>
    /// <param name="path">路径需要后缀名</param>
    /// <returns></returns>
    public static Object GetObjectByPath(string path)
    {
        Object _asset = AssetBundleTool.LoadMainAssetAtPath(path);
        if (null == _asset)
        {
            Debug.LogError("未找到资源：" + path);
        }
        return _asset;
    }

    public static string ToTimeFormat(DateTime date)
    {
        string _value = date.ToString("yyyy_MM_dd HH:mm:ss");
        _value = _value.Replace(" ", "_");
        _value = _value.Replace(":", "_");
        return _value;
    }

    public static void BuildFileTxt(string path, string content)
    {
        byte[] _byteArray = System.Text.Encoding.UTF8.GetBytes(content);
        BuidFile(path, _byteArray);
    }

    public static void BuidFile(string path, byte[] bytes)
    {
        FileStream _fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);

        try
        {
            _fs.Write(bytes, 0, bytes.Length);
            _fs.Flush();
            _fs.Close();
            _fs.Dispose();

        }
        catch (Exception ex)
        {
            Debug.LogError("写入文件 " + path + " 失败！" + ex.Message);
        }
        finally
        {
            _fs.Close();
        }
        _fs = null;
    }
    public static void UnloadAsset(Object asset)
    {
        if (null != asset)
        {
            Resources.UnloadAsset(asset);
        }
        AssetBundleTool.ClearCache();
    }

    public static void UnloadAssets(List<EditorAssetData> list)
    {
        if (null == list)
        {
            return;
        }
        int _count = list.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorBundleType _type = GetBundleTypeByPath(list[i].AssetPath);
            if (_type == EditorBundleType.Prefab || _type == EditorBundleType.Model)
            {

            }
            else
            {
                UnloadAsset(list[i].asset);
            }
            list[i].asset = null;
        }
    }


    public static void GetAllFolder(string path)
    {
        string[] _folderPathArray = Directory.GetDirectories(path);
        for (int i = 0; i < _folderPathArray.Length; ++i)
        {
            if (!_folderPathArray[i].Contains(".."))
            {
                GetAllFolder(_folderPathArray[i]);
                m_folderPathList.Add(_folderPathArray[i]);
            }
        }
    }
    /// <summary>
    /// 创建文件夹
    /// </summary>
    public static void CreatFolder()
    {
        int _count = m_folderPathList.Count;
        for (int i = 0; i < _count; ++i)
        {
            string _path = m_folderPathList[i].Replace("Resources", "StreamingAssets/StreamingResources");
            _path = _path.Replace('\\', '/');
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
        }

    }


    public static void copyHotFile(List<EditorAssetData> list, string path)
    {
        if (string.IsNullOrEmpty(path) || null == list)
        {
            Debug.LogError("参数为空");
            return;
        }

        int _count = list.Count;
        for (int i = 0; i < _count; ++i)
        {
            string _source = Application.dataPath + "/" + EditorPackDef.ABPathRoot + list[i].AssetBundleData.AssetPath + EditorPackDef.AssetBundleSuffix;
            string _dest = path + "/" + list[i].AssetBundleData.AssetPath + EditorPackDef.AssetBundleSuffix;

            _dest = _dest.ToLower();
            string _folderPath = GetResourcePath(_dest);
            _folderPath = _folderPath.ToLower();
            //没有文件夹，则创建
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
            File.Copy(_source, _dest, true);
        }

    }

    /// <summary>
    /// 整理数据关系
    /// </summary>
    /// <param name="dic"></param>
    public static Dictionary<string, EditorAssetData> CollateABAssetInfoRelationship(Dictionary<string, EditorAssetData> dic)
    {
        if (null == dic)
        {
            return dic;
        }

        int _count = dic.Values.Count;
        EditorAssetData[] _array = dic.Values.ToArray();

        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _info = _array[i];
            int _listCount = _info.DownDependentPathList.Count;
            for (int j = 0; j < _listCount; ++j)
            {
                string _downPath = _info.DownDependentPathList[j];
                if (dic.ContainsKey(_downPath))
                {
                    _info.AddDownDependence(dic[_downPath]);
                }
            }
            _listCount = _info.NoRecursiveDownDependentPathList.Count;
            for (int j = 0; j < _listCount; ++j)
            {
                string _downPath = _info.NoRecursiveDownDependentPathList[j];
                if (dic.ContainsKey(_downPath))
                {
                    dic[_downPath].AddUpDependence(_info);
                }
            }
        }
        return dic;
    }

    public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
    {
        TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
        TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
        TimeSpan ts3 = ts1.Subtract(ts2).Duration();
        //你想转的格式
        return ts3.TotalMilliseconds.ToString();
    }

    /// <summary>
    /// 是否为文件夹
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool IsDirectory(string filePath)
    {
        return Directory.Exists(filePath);
    }
    /// <summary>
    /// 是否为文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool IsFile(string filePath)
    {
        return File.Exists(filePath);
    }

    public static void AddDir(string srcPath)
    {
        if (!Directory.Exists(srcPath))
        {
            Directory.CreateDirectory(srcPath);
        }
    }
    public static void DeletFolder(string srcPath)
    {
        if (Directory.Exists(srcPath))
        {
            Directory.Delete(srcPath);
        }
    }

    /// <summary>
    /// 删除目录中所有文件和子目录
    /// </summary>
    /// <param name="srcPath"></param>
    public static void DelectDir(string srcPath)
    {
        if (!Directory.Exists(srcPath))
        {
            return;
        }

        try
        {

            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)            //判断是否文件夹
                {
                    DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                    subdir.Delete(true);          //删除子目录和文件
                }
                else
                {
                    File.Delete(i.FullName);      //删除指定文件
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }

    /// <summary>
    /// 获取指定后缀名的资源 后缀名必须以 .* 结尾( 例如： script.cs )
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <param name="suffix"></param>
    /// <returns></returns>
    public static List<string> GetTheResources(string resourcePath, string suffix)
    {


        if (string.IsNullOrEmpty(resourcePath) || !resourcePath.Contains(Application.dataPath))
        {
            Debug.LogError("非法路径");
            return null;
        }

        string[] files = Directory.GetFiles(resourcePath, "*.*", SearchOption.AllDirectories).Where(s =>


              s.ToLower().EndsWith(suffix)).ToArray();

        string rootPath = Application.dataPath.Replace("Assets", "");
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Replace(rootPath, "");
            files[i] = files[i].Replace(@"\", "/");
        }


        int _sameNameCount = 0;
        List<string> _listWithoutSuffix = new List<string>();
        List<string> _list = files.ToList();
        int _count = _list.Count;

        for (int i = 0; i < _count; ++i)
        {

            if (_list[i].Contains("/.."))
            {
                _list.RemoveAt(i--);
                _count--;
            }
            else
            {
                string _value = AssetBundleTool.DeletSuffix(_list[i]);

                if (!_listWithoutSuffix.Contains(_value))
                {
                    _listWithoutSuffix.Add(_value);
                }
                else
                {
                    _sameNameCount++;
                    Debug.LogError("发现同名资源：" + _value);
                }
            }
        }
        if (_sameNameCount > 0)
        {
            return null;
        }

        _list = _list.OrderBy(v => v).ToList();



        return _list;
    }


    public static List<string> GetUsefulAssets(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !filePath.Contains(Application.dataPath))
        {
            Debug.LogError("非法路径");
            return new List<string>();
        }


        string rootPath = Application.dataPath.Replace("Assets", "");
        string[] files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories).Where(s =>

                !s.ToLower().EndsWith(".cs") &&
                !s.ToLower().EndsWith(".js") &&
                !s.ToLower().EndsWith(".meta")

            ).ToArray();


        List<string> _list = files.ToList();

        for (int i = _list.Count - 1; i >= 0; --i)
        {
            _list[i] = _list[i].Replace(rootPath, "");
            _list[i] = _list[i].Replace(@"\", "/");
            if (_list[i].Contains("/.."))
            {
                _list.RemoveAt(i);
            }
        }



        return _list;
    }

    /// <summary>
    ///路径必须带后缀名
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static EditorBundleType GetBundleTypeByPath(string path)
    {
        EditorBundleType _type = EditorBundleType.None;
        switch (GetFileSuffix(path).ToLower())
        {
            case ".cs":
            case ".js":
            case ".dll":
                {
                    _type = EditorBundleType.Script;
                }
                break;
            case ".shadervariants":
            case ".shader":
                {
                    _type = EditorBundleType.Shader;
                }
                break;
            case ".ttf":
                {
                    _type = EditorBundleType.Font;
                }
                break;
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".bmp":
            case ".tga":
            case ".psd":
            case ".tif":
            case ".exr":
                {
                    _type = EditorBundleType.Texture;
                }
                break;
            case ".mat":
                {
                    _type = EditorBundleType.Material;
                }
                break;

            case ".anim":
                {
                    _type = EditorBundleType.Animation;
                }
                break;
            case ".controller":
                {
                    _type = EditorBundleType.Controller;
                }
                break;

            case ".fbx":
                {
                    _type = EditorBundleType.Model;
                }
                break;
            case "json":
            case ".txt":
            case ".bytes":
                {
                    _type = EditorBundleType.TextAsset;
                }
                break;
            case ".prefab":
                {
                    _type = EditorBundleType.Prefab;
                }
                break;
            case ".unity":
                {
                    _type = EditorBundleType.Scene;
                }
                break;
            case ".mp3":
            case ".ogg":
            case ".wav":
                {
                    _type = EditorBundleType.Audio;
                }
                break;
            case ".asset":
                {
                    _type = EditorBundleType.Asset;
                }
                break;
            default:
                {

                }
                break;
        }
        return _type;
    }

    public static AssetBigType GetAssetTypeByPath(string path)
    {
        AssetBigType _type = AssetBigType.Base;
        switch (GetFileSuffix(path).ToLower())
        {
            case ".unity":
            case ".prefab":
            case ".material":
            case ".asset":
                {
                    _type = AssetBigType.Compound;
                }
                break;
            default:
                {
                    _type = AssetBigType.Base;
                }
                break;


        }
        return _type;
    }

    /// <summary>
    /// 资源信息排序
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<EditorAssetData> OrderABAssetInfoList(List<EditorAssetData> list)
    {
        return list.OrderByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Script;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Shader;
        }).ThenByDescending(v =>
        {
            return v.IsCommonAsset;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Texture;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.TextAsset;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Font;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Material;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Audio;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Animation;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Controller;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Model;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Prefab;
        }).ThenByDescending(v =>
        {
            return v.BundleType == EditorBundleType.Scene;
        })



        .ToList();
    }

    /// <summary>
    /// 获取MD5码
    /// </summary>
    /// <param name="fpath">绝对路径</param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetFileMD5(string fpath, ref int size)
    {
        FileStream fs = new FileStream(fpath, FileMode.Open);
        MD5 md5 = MD5.Create();
        byte[] vals = md5.ComputeHash(fs);
        string ret = BitConverter.ToString(vals);
        ret = ret.Replace("-", "");
        size = (int)fs.Length;
        md5.Clear();
        fs.Close();
        fs.Dispose();
        fs = null;
        return ret;
    }


    /// <summary>
    /// 资源路径+MD5
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string buildMD5InfoStream(EditorAssetMD5Data data)
    {
        StringBuilder _sb = new StringBuilder();
        _sb.Append(data.Path);

        _sb.Append('\t');

        _sb.Append(data.MD5);
        return _sb.ToString();
    }

    /// <summary>
    /// 创建MD5数据
    /// </summary>
    /// <param name="shaderList"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string CreateMD5Text(List<EditorAssetMD5Data> AssetsList, List<EditorAssetMD5Data> AssetsMetaList, List<EditorAssetMD5Data> StreamingAssetsList)
    {
        StringBuilder _sb = new StringBuilder();

        _sb.Append("资源路径");
        _sb.Append("\t");

        _sb.Append("MD5码");
        _sb.Append("\r\n");

        int _count = AssetsList.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetMD5Data _info = AssetsList[i];

            _sb.Append(buildMD5InfoStream(_info));

            if (i != _count - 1)
            {
                _sb.Append("\r\n");
            }
        }

        _sb.Append("\r\n");
        _sb.Append(EditorPackDef.MD5Line);
        _sb.Append("\r\n");
        _count = AssetsMetaList.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetMD5Data _info = AssetsMetaList[i];

            _sb.Append(buildMD5InfoStream(_info));

            if (i != _count - 1)
            {
                _sb.Append("\r\n");
            }
        }

        _sb.Append("\r\n");
        _sb.Append(EditorPackDef.MD5Line);
        _sb.Append("\r\n");
        _count = StreamingAssetsList.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetMD5Data _info = StreamingAssetsList[i];

            _sb.Append(buildMD5InfoStream(_info));

            if (i != _count - 1)
            {
                _sb.Append("\r\n");
            }
        }
        return _sb.ToString();
    }
    /// <summary>
    /// 资源路径+是否为公共资源(1 true, 0 false)|包路径|依赖的资源
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string buildManiFestInfoStream(EditorAssetData data)
    {
        StringBuilder _sb = new StringBuilder();

        string _temp = data.AssetPath.Replace(EditorPackDef.AssetTmpPathRoot, string.Empty);
        _temp = _temp.Replace(EditorPackDef.AssetPathRoot, string.Empty);

        _sb.Append(_temp);
        _sb.Append('\t');
        _sb.Append(data.AssetType.ToString());
        _sb.Append('\t');
        _sb.Append(data.IsCommonAsset ? "1" : "0");
        _sb.Append('\t');
        _sb.Append(data.IsSolid ? "1" : "0");
        _sb.Append('\t');
        _sb.Append(data.AssetBundleData.LoadPath);

        return _sb.ToString();
    }


    /// <summary>
    /// 创建资源关系网
    /// </summary>
    /// <param name="shaderList"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string CreateManiFestText(List<EditorAssetData> list)
    {
        StringBuilder _sb = new StringBuilder();
        _sb.Append("资源路径");
        _sb.Append("\t");
        _sb.Append("资源类型");
        _sb.Append("\t");
        _sb.Append("是否为公共资源(1 true, 0 false)");
        _sb.Append("\t");
        _sb.Append("不丢包(1 true, 0 false)");
        _sb.Append("\t");
        _sb.Append("资源所属AssetBundle包名");
        _sb.Append("\r\n");

        //排序
        list = OrderABAssetInfoList(list);

        int _count = list.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _info = list[i];


            if (_info.Useless || _info.FinalAssetBundleName.Equals(string.Empty) ||
                null == _info.AssetBundleData || null == _info.AssetBundleData.AssetPath ||
                _info.AssetBundleData.AssetPath.Equals(string.Empty))
            {
                continue;
            }


            _sb.Append(buildManiFestInfoStream(_info));

            if (i != _count - 1)
            {
                _sb.Append("\r\n");
            }
        }

        return _sb.ToString();
    }




    private static string buildAssetBundleDataStream(EditorAssetData data)
    {
        StringBuilder _sb = new StringBuilder();

        _sb.Append(data.AssetBundleData.LoadPath);
        _sb.Append('\t');
        _sb.Append(data.AssetBundleData.Common);
        _sb.Append('\t');

        //所依赖的包路径
        if (null == data.AssetBundleData.AssetBundlePathList)
        {

        }
        else
        {
            for (int i = 0; i < data.AssetBundleData.AssetBundlePathList.Count; ++i)
            {

                string _path = data.AssetBundleData.AssetBundlePathList[i];

                string _temp = _path.Replace(EditorPackDef.AssetTmpPathRoot, string.Empty);
                _temp = _temp.Replace(EditorPackDef.AssetPathRoot, string.Empty);

                _sb.Append(_temp);
                if (i != data.AssetBundleData.AssetBundlePathList.Count - 1)
                {
                    _sb.Append('+');
                }
            }
        }
        _sb.Append('\t');
        _sb.Append(data.AssetBundleData.Size);
        _sb.Append('\t');
        _sb.Append(data.AssetBundleData.Version);
        _sb.Append('\t');
        _sb.Append(data.AssetBundleData.Crc);
        _sb.Append('\t');
        _sb.Append(data.AssetBundleData.CompressCrc);
        _sb.Append('\t');
        _sb.Append(data.AssetBundleData.offset);


        return _sb.ToString();
    }
    public static void CreateAssetManifest(List<EditorAssetData> list)
    {
        ManiFest _abd = ScriptableObject.CreateInstance<ManiFest>();
        int _count = list.Count;
        List<string> _abkey = new List<string>();
        List<ListData> _abvalue = new List<ListData>();

        List<string> _key = new List<string>();
        List<string> _value = new List<string>();
        List<string> _valueList = new List<string>();//记录已被解析过的资源，去重
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _info = list[i];


            if (_info.Useless || _info.FinalAssetBundleName.Equals(string.Empty) ||
                null == _info.AssetBundleData || null == _info.AssetBundleData.AssetPath ||
                _info.AssetBundleData.AssetPath.Equals(string.Empty))
            {
                continue;
            }
            _key.Add(DeletSuffix(_info.AssetPath.Replace("Assets/Resources/", "")));
            _value.Add(_info.AssetBundleData.LoadPath);
        }
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _info = list[i];

            if (_info.Useless || (_valueList.Contains(_info.AssetBundleData.LoadPath)))
            {
                continue;
            }
            _valueList.Add(_info.AssetBundleData.LoadPath);


            ListData listData = new ListData();
            //所依赖的包路径
            if (null != _info.AssetBundleData.AssetBundlePathList)
            {
                for (int j = 0; j < _info.AssetBundleData.AssetBundlePathList.Count; ++j)
                {

                    string _path = _info.AssetBundleData.AssetBundlePathList[j];

                    string _temp = _path.Replace(EditorPackDef.AssetTmpPathRoot, string.Empty);
                    _temp = _temp.Replace(EditorPackDef.AssetPathRoot, string.Empty);
                    if (listData.Data == null)
                        listData.Data = new List<string>();
                    listData.Data.Add(_temp);
                }
            }
            _abkey.Add(_info.AssetBundleData.LoadPath);
            _abvalue.Add(listData);
        }
        _abd.InitAssetBundleData(_abkey.ToArray(), _abvalue.ToArray());


        _abd.InitMainFestData(_key.ToArray(), _value.ToArray());
        AssetDatabase.CreateAsset(_abd, "Assets/Resources/AssetBundleData.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static string CreateAssetBundleDataText(List<EditorAssetData> list)
    {
        StringBuilder _sb = new StringBuilder();
        _sb.Append("包名");
        _sb.Append("\t");
        _sb.Append("是否为公共资源包(1 true, 0 false)");
        _sb.Append("\t");
        _sb.Append("所依赖的包路径");
        _sb.Append("\t");
        _sb.Append("大小(B 字节)");
        _sb.Append("\t");
        _sb.Append("版本号");
        _sb.Append("\t");
        _sb.Append("Crc");
        _sb.Append("\t");
        _sb.Append("CompressCrc");
        _sb.Append("\t");
        _sb.Append("offset");//offset

        _sb.Append("\r\n");



        List<string> _valueList = new List<string>();//记录已被解析过的资源，去重


        //排序
        list = OrderABAssetInfoList(list);

        int _count = list.Count;

        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _info = list[i];

            if (_info.Useless || _valueList.Contains(_info.AssetBundleData.LoadPath))
            {
                continue;
            }

            _sb.Append(buildAssetBundleDataStream(_info));
            _valueList.Add(_info.AssetBundleData.LoadPath);
            if (i != _count - 1)
            {
                _sb.Append("\r\n");
            }
        }
        _valueList.Clear();
        _valueList = null;


        return _sb.ToString();
    }



    private static Dictionary<string, List<string>> LabelDic = new Dictionary<string, List<string>>();
    private static List<LabelInfo> AllLabelList = new List<LabelInfo>();

    public static List<string> GetTheLabelList(string label)
    {
        List<string> _value = null;
        if (!LabelDic.TryGetValue(label, out _value))
        {
            _value = new List<string>();
            List<string> _list = AssetDatabase.FindAssets("l:" + label).ToList();
            for (int i = 0; i < _list.Count; ++i)
            {
                string _path = AssetDatabase.GUIDToAssetPath(_list[i]);
                _value.Add(_path);
            }
            _value = _value.OrderByDescending(v => v.Length).ToList();
            LabelDic.Add(label, _value);

        }

        return _value;
    }

    public static List<LabelInfo> GetLabelInfoList(string label, List<string> pathList)
    {
        List<LabelInfo> _infoList = new List<LabelInfo>();
        List<string> _list = GetTheLabelList(label);
        for (int i = 0; i < _list.Count; ++i)
        {
            _infoList.Add(new LabelInfo()
            {
                label = label,
                path = _list[i]
            });
        }
        return _infoList;
    }
    public static string GetTheLabel(string path)
    {
        if (0 == AllLabelList.Count)
        {
            AllLabelList.AddRange(GetLabelInfoList(EditorPackDef.Label_BuildSingle, GetTheLabelList(EditorPackDef.Label_BuildSingle)));
            AllLabelList.AddRange(GetLabelInfoList(EditorPackDef.Label_BuildAsFolderName, GetTheLabelList(EditorPackDef.Label_BuildAsFolderName)));
            AllLabelList.AddRange(GetLabelInfoList(EditorPackDef.Label_IsCommonAsset, GetTheLabelList(EditorPackDef.Label_IsCommonAsset)));
            AllLabelList.AddRange(GetLabelInfoList(EditorPackDef.Label_EditorOnly, GetTheLabelList(EditorPackDef.Label_EditorOnly)));

            AllLabelList = AllLabelList.OrderByDescending(v => v.path.Length).ToList();
        }
        for (int i = 0; i < AllLabelList.Count; ++i)
        {
            if (path.Contains(AllLabelList[i].path))
            {
                return AllLabelList[i].label;
            }
        }

        return string.Empty;
    }
    /// <summary> 
    /// 资源文件是否被标记为label这个标签
    /// </summary>
    /// <returns></returns>
    public static bool IstheLabel(string path, string label)
    {
        List<string> _value = GetTheLabelList(label);
        for (int i = 0; i < _value.Count; ++i)
        {
            if (path.Contains(_value[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否有指定标签
    /// </summary>
    /// <param name="path"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    public static bool HasLabel(string path, string label)
    {
        UnityEngine.Object _object = AssetBundleTool.GetObjectByPath(path);
        if (null != _object)
        {
            string[] _labelArray = AssetDatabase.GetLabels(_object);
            for (int i = 0; i < _labelArray.Length; ++i)
            {
                if (_labelArray[i].Equals(label))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static void AddLabel(Object obj, string label)
    {
        UnityEngine.Object _object = obj;
        if (null != _object)
        {
            if (label == "")
            {
                AssetBundleTool.ClearLabel(_object);
                return;
            }


            List<string> _labelList = AssetDatabase.GetLabels(obj).ToList();
            List<string> _newLabelList = new List<string>();

            string _label = string.Empty;

            for (int i = 0; i < _labelList.Count; ++i)
            {
                _label = _labelList[i];
                if (!_newLabelList.Exists(v => v == _label))
                {
                    _newLabelList.Add(_label);
                }
            }

            if (!_newLabelList.Contains(label))
            {
                _newLabelList.Add(label);
            }
            AssetDatabase.SetLabels(obj, _newLabelList.ToArray());
        }

    }
    public static void ClearLabel(Object obj)
    {
        UnityEngine.Object _object = obj;
        if (null != _object)
        {
            AssetDatabase.ClearLabels(obj);
        }

    }

    public static void ClearTheLabel(Object obj, string label)
    {
        UnityEngine.Object _object = obj;
        if (null != _object)
        {
            List<string> _newLabelList = AssetDatabase.GetLabels(_object).ToList();
            _newLabelList.Remove(label);
            AssetDatabase.SetLabels(obj, _newLabelList.ToArray());
        }

    }


    public static AssetImporter GetAtPath(string path)
    {
        return AssetImporter.GetAtPath(path);
    }


    public static string GetFileStreamValue(FileStream fs)
    {
        int fsLen = (int)fs.Length;
        byte[] heByte = new byte[fsLen];
        int r = fs.Read(heByte, 0, heByte.Length);
        string myStr = System.Text.Encoding.UTF8.GetString(heByte);
        fs.Seek(0, SeekOrigin.Begin);
        return myStr;
    }

    public static List<EditorAssetData> AddDataToList(List<EditorAssetData> targetList, EditorAssetData data)
    {
        if (!targetList.Contains(data))
        {
            targetList.Add(data);
        }
        else
        {
            Debug.LogWarning("重复的数据");
        }

        return targetList;
    }

    public static void AddListToTargetList(List<EditorAssetData> targetList, List<EditorAssetData> dataList)
    {
        if (null == targetList || null == dataList)
        {
            Debug.LogError("传入链表不能为空！");
            return;
        }
        int _count = dataList.Count;
        for (int i = 0; i < _count; ++i)
        {
            if (!targetList.Exists(v => v.AssetPath == dataList[i].AssetPath))
            {
                targetList.Add(dataList[i]);
            }
        }
    }
    public static void AddListToTargetList(List<string> targetList, List<string> dataList)
    {
        if (null == targetList || null == dataList)
        {
            Debug.LogError("传入链表不能为空！");
            return;
        }
        int _count = dataList.Count;
        for (int i = 0; i < _count; ++i)
        {
            if (!targetList.Exists(v => v == dataList[i]))
            {
                targetList.Add(dataList[i]);
            }
        }
    }
    public static void AddListToTargetList(List<string> targetList, string value)
    {
        if (null == targetList)
        {
            Debug.LogError("传入链表不能为空！");
            return;
        }

        if (!targetList.Exists(v => v == value))
        {
            targetList.Add(value);
        }

    }
    public static void AddDataToDic(Dictionary<string, EditorAssetData> targetDic, EditorAssetData data)
    {
        if (!targetDic.ContainsKey(data.AssetPath))
        {
            targetDic.Add(data.AssetPath, data);
        }
        else
        {
            Debug.LogWarning("重复的数据：" + data.AssetPath);
        }
    }

    public static void AddDicToDic(Dictionary<string, EditorAssetData> targetDic, Dictionary<string, EditorAssetData> dataDic)
    {
        if (null == targetDic || null == dataDic)
        {
            Debug.LogError("传入字典不能为空！");
            return;
        }
        KeyValuePair<string, EditorAssetData>[] _array = dataDic.ToArray();
        int _length = _array.Length;
        for (int i = 0; i < _length; ++i)
        {
            if (!targetDic.ContainsKey(_array[i].Key))
            {
                targetDic.Add(_array[i].Key, _array[i].Value);
            }
            else
            {
                Debug.LogWarning("重复的数据：" + _array[i].Key);
            }
        }

    }

    /// <summary>
    /// 获取上级引用资源
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static List<EditorAssetData> GetUpDependentAsset(EditorAssetData data)
    {

        int _count = data.UpDependenceList.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _child = data.UpDependenceList[i];
            if (!m_AssetMapList.Contains(_child))
            {
                m_AssetMapList.Add(_child);
            }
            GetUpDependentAsset(_child);
        }
        if (!m_AssetMapList.Contains(data))
        {
            m_AssetMapList.Add(data);
        }
        return m_AssetMapList;
    }

    /// <summary>
    /// 获取下级依赖资源
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static List<EditorAssetData> GetDownDependentAsset(EditorAssetData data)
    {

        int _count = data.DownDependenceDataList.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _child = data.DownDependenceDataList[i];
            if (!m_AssetMapList.Contains(_child))
            {
                m_AssetMapList.Add(_child);
            }
            GetDownDependentAsset(_child);
        }
        if (!m_AssetMapList.Contains(data))
        {
            m_AssetMapList.Add(data);
        }
        return m_AssetMapList;
    }
    private static List<string> GetAssetBundleRelationshipAsset(string path)
    {
        if (!m_targetAssetPathList.Contains(path))
        {
            m_targetAssetPathList.Add(path);
        }

        //资源关联的包的其他资源
        List<string> _targetAssetPathList = new List<string>();
        EditorAssetData _data = m_origionalAsseDatatList.Find(v => v.AssetPath == path);
        if (null == _data)
        {
            return m_targetAssetPathList;
        }
        EditorAssetBundleData _abData = _data.AssetBundleData;
        for (int i = 0; i < _abData.AssetBundlePathList.Count; ++i)
        {
            string _path = _abData.AssetBundlePathList[i];
            if (!m_targetAssetPathList.Contains(_path))
            {
                m_targetAssetPathList.Add(_path);
            }

            EditorAssetData _child = m_origionalAsseDatatList.Find(v => v.AssetPath == _path);
            if (null != _child)
            {
                int _count = _child.DownDependenceDataList.Count;
                for (int j = 0; j < _count; ++j)
                {
                    EditorAssetData _child1 = _child.DownDependenceDataList[j];
                    string _key = _child1.AssetPath;
                    if (!m_targetAssetPathList.Contains(_key))
                    {
                        GetAssetBundleRelationshipAsset(_key);
                    }
                }
            }
        }

        List<string> _targetAssetPathList_new = new List<string>();
        EditorAssetData _data_new = m_currentAssetDataList.Find(v => v.AssetPath == path);
        if (null == _data_new)
        {
            return m_targetAssetPathList;
        }
        EditorAssetBundleData _abData_new = _data_new.AssetBundleData;
        for (int i = 0; i < _abData_new.AssetBundlePathList.Count; ++i)
        {
            string _path = _abData_new.AssetBundlePathList[i];
            if (!m_targetAssetPathList.Contains(_path))
            {
                m_targetAssetPathList.Add(_path);
            }

            EditorAssetData _child = m_currentAssetDataList.Find(v => v.AssetPath == _path);
            if (null != _child)
            {
                int _count = _child.DownDependenceDataList.Count;
                for (int j = 0; j < _count; ++j)
                {
                    EditorAssetData _child1 = _child.DownDependenceDataList[j];
                    string _key = _child1.AssetPath;
                    if (!m_targetAssetPathList.Contains(_key))
                    {
                        GetAssetBundleRelationshipAsset(_key);
                    }
                }
            }
        }



        return m_targetAssetPathList;

    }


    private static List<EditorAssetData> m_origionalAsseDatatList = new List<global::EditorAssetData>();
    private static List<EditorAssetData> m_currentAssetDataList = new List<global::EditorAssetData>();
    private static List<EditorAssetData> m_AssetMapList = new List<global::EditorAssetData>();
    private static List<string> m_targetAssetPathList = new List<string>();
    public static List<EditorAssetData> GetAssetMap(Dictionary<string, EditorAssetData> AssetDataDic, List<EditorAssetData> origionalAssetDataList, List<EditorAssetData> currentAssetDataList, List<string> selectedAssetPathList)
    {
        m_origionalAsseDatatList = origionalAssetDataList;
        m_currentAssetDataList = currentAssetDataList;
        m_AssetMapList.Clear();
        m_targetAssetPathList.Clear();

        if (null == AssetDataDic || null == selectedAssetPathList || 0 == AssetDataDic.Count || 0 == selectedAssetPathList.Count)
        {
            return m_AssetMapList;
        }
        for (int i = 0; i < selectedAssetPathList.Count; ++i)
        {
            GetAssetBundleRelationshipAsset(selectedAssetPathList[i]);
        }

        for (int i = 0; i < m_targetAssetPathList.Count; ++i)
        {
            string _path = m_targetAssetPathList[i];

            EditorAssetData _data = null;
            if (AssetDataDic.TryGetValue(_path, out _data))
            {
                GetUpDependentAsset(_data);
                GetDownDependentAsset(_data);//例如：打一个mat资源，则需要打其引用的shader，texture

            }
        }
        for (int i = 0; i < m_AssetMapList.Count; ++i)
        {
            if (!AssetDataDic.ContainsKey(m_AssetMapList[i].AssetPath))
            {
                m_AssetMapList.RemoveAt(i--);

            }
        }
        return m_AssetMapList;
    }


    public static bool IsFileInUse(string fileName)
    {
        bool inUse = true;
        FileStream fs = null;
        try
        {
            fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,
            FileShare.None);
            inUse = false;
        }
        catch
        {
        }
        finally
        {
            if (fs != null)
                fs.Close();
        }
        return inUse;//true表示正在使用,false没有使用    
    }

    private static int ClearCount = 0;

    public static void SetAssetBundleName(string assetPath, string assetBundleName, string assetBundleVariant = "")
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return;
        }

        //2000次清理一次
        if (ClearCount++ > 2000)
        {
            ClearCount = 0;
            ClearCache();
            AssetDatabase.Refresh();
        }

        AssetImporter _import = AssetImporter.GetAtPath(assetPath);
        if (null != _import)
        {

            if (assetBundleName != _import.assetBundleName)
            {
                _import.assetBundleName = assetBundleName;
            }

            if (null != assetBundleName)
            {
                if (assetBundleVariant != _import.assetBundleVariant)
                {
                    _import.assetBundleVariant = assetBundleVariant;
                }
            }

        }
        else
        {
            UnityEngine.Debug.LogError("Can not find the asset with path: " + assetPath);
        }



    }
    public static void ClearAssetBundleName( string assetPath)
    {
         SetAssetBundleName(assetPath, string.Empty, string.Empty);
    }
    public static string GetAssetBundleName(string assetPath)
    {
        AssetImporter _import = AssetImporter.GetAtPath(assetPath);
        if (null != _import)
        {
            return _import.assetBundleName;
        }
        return string.Empty;
    }

    public static string GetAssetBundleName(Object obj)
    {
        if (null == obj)
        {
            return string.Empty;
        }
        return GetAssetBundleName(AssetBundleTool.GetAssetPath(obj));
    }




    #region 加密

    #region offset偏移 加密


    /// <summary>
    /// 加密指定文件夹下的 ab 
    /// </summary>
    /// <param name="filePath"></param>
    //public static void EncryptyGameFiles(string filePath)
    //{


    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();

    //    FAssetBundleDataNest _assetBundleNest = Resources.Load("AssetBundleNest") as FAssetBundleDataNest;
    //    if (null == _assetBundleNest)
    //    {
    //        Debug.LogError("未找到  AssetBundleNest，加密失败！！！");
    //        return;
    //    }

    //    Dictionary<string, FAssetBundleData> _dic = new Dictionary<string, FAssetBundleData>(_assetBundleNest.m_list.Count);
    //    for (int i = 0; i < _assetBundleNest.m_list.Count; ++i)
    //    {
    //        _dic.Add(_assetBundleNest.m_list[i].Path, _assetBundleNest.m_list[i]);
    //    }

    //    string _path = string.Empty;
    //    FAssetBundleData _fAssetBundleData = null;
    //    int offset = 0;

    //    string[] files = new string[1];
    //    if (File.Exists(filePath))
    //    {
    //        files[0] = filePath;
    //    }
    //    else
    //    {

    //        files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories).Where(s =>
    //                s.ToLower().EndsWith(".assetbundle")).ToArray();
    //    }

    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        _path = files[i];
    //        _path = _path.Replace(@"\", "/");
    //        files[i] = _path;
    //        _path = _path.Replace(Application.dataPath + "/StreamingAssets/StreamingResources/", "");
    //        _dic.TryGetValue(_path, out _fAssetBundleData);

    //        if (null == _fAssetBundleData)
    //        {
    //            if (_path.Contains("allscripts.assetbundle"))
    //            {
    //                offset = 0;

    //            }
    //            if (_path.Contains("manifest.assetbundle"))
    //            {
    //                offset = 0;
    //            }

    //        }
    //        else
    //        {
    //            offset = (int)_fAssetBundleData.offset;
    //        }
    //        Encrypty(files[i], offset);


    //    }


    //}


    public static void Encrypty(string filePath, int offset = 0)
    {
        if (offset <= 0)
        {
            return;
        }

        filePath = filePath.Replace(@"\", "/");

        byte[] filedata = File.ReadAllBytes(filePath);

        if (filedata[0] == 0xFF && filedata[1] == 0xFF)
        {
            //已经加密了 
            return;
        }

        byte[] _abHeader = new byte[32];
        Array.Copy(filedata, 0, _abHeader, 0, 32);

        int filelen = offset + filedata.Length;
        byte[] buffer = new byte[filelen];
        UnityEngine.Random.seed = filePath.Length;
        byte[] _head = new byte[(int)offset];
        for (int j = 0; j < _head.Length; ++j)
        {
            if (j == 0 || j == 1)//一次 offset偏移加密标志位
            {
                _head[j] = 0xFF;
            }
            else if (j == 2 || j == 3)//二次 异或加密标志位
            {
                //_head[j] = (byte)UnityEngine.Random.Range(0, 0xEE);
                _head[j] = (byte)(j & filelen & 0xEE);
            }
            else if (j > 3 && j < 3 + _abHeader.Length)
            {
                _head[j] = _abHeader[j - 4];
            }
            else
            {
                //_head[j] = (byte)UnityEngine.Random.Range(0, 0xFF);
                _head[j] = (byte)(j & filelen & 0xFF);
            }
        }
        Array.Copy(_head, 0, buffer, 0, offset);
        Array.Copy(filedata, 0, buffer, offset, filedata.Length);

        File.WriteAllBytes(filePath, buffer);
        FileStream fs = File.OpenWrite(filePath);
        fs.Write(buffer, 0, filelen);
        fs.Flush();
        fs.Close();

    }

    //public static void DecryptyGameFiles(string filePath)
    //{

    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();

    //    FAssetBundleDataNest _assetBundleNest = Resources.Load("AssetBundleNest") as FAssetBundleDataNest;
    //    if (null == _assetBundleNest)
    //    {
    //        Debug.LogError("未找到  AssetBundleNest，加密失败！！！");
    //        return;
    //    }

    //    Dictionary<string, FAssetBundleData> _dic = new Dictionary<string, FAssetBundleData>(_assetBundleNest.m_list.Count);
    //    for (int i = 0; i < _assetBundleNest.m_list.Count; ++i)
    //    {
    //        _dic.Add(_assetBundleNest.m_list[i].Path, _assetBundleNest.m_list[i]);
    //    }

    //    string _path = string.Empty;
    //    FAssetBundleData _fAssetBundleData = null;
    //    int offset = 0;

    //    string[] files = new string[1];
    //    if (File.Exists(filePath))
    //    {
    //        files[0] = filePath;
    //    }
    //    else
    //    {

    //        files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories).Where(s =>
    //                s.ToLower().EndsWith(".assetbundle")).ToArray();
    //    }

    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        _path = files[i];
    //        _path = _path.Replace(@"\", "/");
    //        files[i] = _path;
    //        _path = _path.Replace(Application.dataPath + "/StreamingAssets/StreamingResources/", "");
    //        _dic.TryGetValue(_path, out _fAssetBundleData);

    //        if (null == _fAssetBundleData)
    //        {
    //            if (_path.Contains("allscripts.assetbundle"))
    //            {
    //                offset = 0xBA;
    //            }
    //            if (_path.Contains("manifest.assetbundle"))
    //            {
    //                offset = 0xFF;
    //            }

    //        }
    //        else
    //        {
    //            offset = (int)_fAssetBundleData.offset;
    //        }

    //        Decrypty(files[i], offset);


    //    }


    //}

    public static void Decrypty(string filePath, int offset = 0)
    {
        if (offset < 0)
        {
            return;
        }

        filePath = filePath.Replace(@"\", "/");

        byte[] filedata = File.ReadAllBytes(filePath);
        if (filedata[0] == 0xFF && filedata[1] == 0xFF)
        {
            int filelen = filedata.Length - offset;
            byte[] buffer = new byte[filelen];

            Array.Copy(filedata, offset, buffer, 0, filelen);

            File.WriteAllBytes(filePath, buffer);
            FileStream fs = File.OpenWrite(filePath);
            fs.Write(buffer, 0, filelen);
            fs.Flush();
            fs.Close();

        }
    }

    #endregion

    #region 异或 加密

    public static byte[] EncryptyFile(byte[] filedata, int code)
    {

        int filelen = filedata.Length;

        for (int i = 0; i < filelen; ++i)
        {
            if (i % 2 == 0)
            {
                filedata[i] = (byte)(filedata[i] ^ code);
            }
            else
            {
                filedata[i] = filedata[i];
            }

        }
        return filedata;
    }

    #endregion

    #endregion
    public static void DeletAllManiFest(string path)
    {
        List<string> _manifestList = GetTheResources(path, ".manifest");
        for (int i = 0; i < _manifestList.Count; ++i)
        {
            File.Delete(_manifestList[i]);
        }

    }
    public static void DeletUnusedAssetBundle(string filePath)
    {
        ManiFest _assetBundleNest = Resources.Load("AssetBundleData") as ManiFest;
        if (null == _assetBundleNest)
        {
            Debug.LogError("未找到  AssetBundleNest，操作失败！！！");
            return;
        }
        var _list=_assetBundleNest.Path.ToList();
        string _path = string.Empty;

        string[] files = Directory.GetFiles(filePath, "*.*", SearchOption.AllDirectories).Where(s =>
                  s.ToLower().EndsWith(".manifest") || s.EndsWith(".assetbundle")).ToArray();
        for (int i = 0; i < files.Length; i++)
        {
            string _filePath = files[i].Replace(@"\", "/");
            string _assetBundleFilePath = files[i].Replace(".manifest", "").Replace(@"\", "/");
            _path = _assetBundleFilePath.Replace(Application.dataPath + "/StreamingAssets/StreamingResources/", "");

            if (!_list.Exists(v=>v==_path))
            {
                if (_path.Contains("allscripts"))
                {
                    continue;
                }
                if (_path.Contains("luascriptsbyte"))
                {
                    continue;
                }
                if (_path.Contains("manifest"))
                {
                    continue;
                }
                if (_path.Contains("xluafixfiles"))
                {
                    continue;
                }
                if (_path.Contains("allinject"))
                {
                    continue;
                }
                if (_assetBundleFilePath.Contains("StreamingResources/StreamingResources"))
                {
                    continue;
                }

                File.Delete(_filePath);
                Debug.LogError("删除无效文件：" + _filePath);
                if (File.Exists(_assetBundleFilePath))
                {
                    File.Delete(_assetBundleFilePath);
                    Debug.LogError("删除无效文件：" + _assetBundleFilePath);
                }
                continue;

            }

        }
    }

    public static void AutoBuildHotfix(string sourceFolder, string destFolder, string hotfixFolder)
    {
        if (string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(destFolder) || string.IsNullOrEmpty(hotfixFolder))
        {
            return;
        }
        List<string> _list = new List<string>();
        string[] files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories).Where(s =>
                 s.ToLower().EndsWith(".assetbundle")).ToArray();
        for (int i = 0; i < files.Length; ++i)
        {
            string _chilePath = files[i].Replace(sourceFolder, "").Replace("\\", "/");
            if (!string.IsNullOrEmpty(CompareFile(files[i], destFolder + _chilePath)))
            {
                _list.Add(files[i]);
            }
        }

        DelectDir(hotfixFolder);
        for (int i = 0; i < _list.Count; ++i)
        {
            string _chilePath = _list[i].Replace(sourceFolder, "").Replace("\\", "/");
            string _dest = hotfixFolder + _chilePath;
            string _folderPath = GetResourcePath(_dest);
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }

            BuidFile(_dest, File.ReadAllBytes(_list[i]));
        }
        string _GameResPath = hotfixFolder.Replace("/StreamingResources", "");
        //生成MD5
        //BuildAssetBundle.CreateMoveList(_GameResPath);

        //生成压缩文件
        if (File.Exists(_GameResPath + ".7z"))
        {
            File.Delete(_GameResPath + ".7z");
        }
        //ZipHelper _zipHelper = new ZipHelper(Application.dataPath + "/../../hotfix/7z/7z.exe");
        //_zipHelper.CompressDirectory(_GameResPath, _GameResPath + ".7z");


    }

    public static string CompareFile(string sourceFilePath, string destFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath) || string.IsNullOrEmpty(destFilePath))
        {
            return string.Empty;
        }
        if (!File.Exists(sourceFilePath))
        {
            return string.Empty;
        }
        if (!File.Exists(destFilePath))
        {
            return sourceFilePath;
        }
        byte[] _source = File.ReadAllBytes(sourceFilePath);
        byte[] _dest = File.ReadAllBytes(destFilePath);
        if (_source.Length != _dest.Length)
        {
            return sourceFilePath;
        }
        for (int i = 0; i < _source.Length; ++i)
        {
            if (_source[i] != _dest[i])
            {
                return sourceFilePath;
            }
        }


        return string.Empty;
    }

    public static void ClearCache()
    {
        Resources.UnloadUnusedAssets();
        AssetDatabase.RemoveUnusedAssetBundleNames();
        EditorUtility.UnloadUnusedAssetsImmediate();
#if UNITY_5
         Caching.CleanCache();
#elif UNITY_2017|| UNITY_2018
        Caching.ClearCache();
#endif
        System.GC.Collect();
        AssetDatabase.Refresh();
    }

    public static string GetAssetPath(Object asset)
    {
        if (null == asset)
        {
            return string.Empty;
        }
        return AssetDatabase.GetAssetPath(asset);
    }

    public static Object LoadMainAssetAtPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }
        return AssetDatabase.LoadMainAssetAtPath(assetPath);
    }


    public static void DisplayProgressBar(string title, string info, int i, int count)
    {
        if (0 == count)
        {
            i = count = 0;
            Debug.LogError("count can not be zero.");
        }
        float _percent = i / (count * 1.0f);
        string _percentStr = (int)(_percent * 100) + "%";
        EditorUtility.DisplayProgressBar(title + i + "/" + count + " (" + _percentStr + ")", info, _percent);
    }

    #region ab包设置相关
    public static void SetABInfo(Dictionary<string, List<string>> abDic,string assetBundleName,string assetName)
    {
        List<string> _value;
        abDic.TryGetValue(assetBundleName, out _value);
        if (_value == null)
        {
            _value = new List<string>();
            if (_value.Exists(v => v == assetName) == false)
                _value.Add(assetName);
            abDic[assetBundleName] = _value;
        }
        else
        {
            if (_value.Exists(v => v == assetName) == false)
                _value.Add(assetName);
        }
    }
    public static List<AssetBundleBuild> ABInfoDicToArray(Dictionary<string, List<string>> abDic)
    {
        List<AssetBundleBuild> _abList = new List<AssetBundleBuild>();
        foreach (var item in abDic)
        {
            AssetBundleBuild _temp = new AssetBundleBuild();
            _temp.assetBundleName = item.Key;
            _temp.assetNames = item.Value.ToArray();
            _abList.Add(_temp);
        }
        return _abList;
    }
    #endregion
}

public class LabelInfo
{
    public string label;
    public string path;
}