using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

/// <summary>
/// 如果上级累计引用计数为1且，则与上级资源打在一起，否则打成单独的AssetBundle包，让上级资源记录与自己的引用关系
/// 如果下级资源只依赖于自己，则与自己打包在一起，否则只记录与下级资源的依赖关系
/// </summary>
public class EditorAssetData
{
    /// <summary>
    /// 顶点上级资源
    /// </summary>
    public List<EditorAssetData> TopUpDependenceList;

    /// <summary>
    /// 上级引用关系，被哪些资源直接引用了
    /// </summary>
    public List<EditorAssetData> UpDependenceList;
    /// <summary>
    /// 资源长路径，例如：Assets/Resources/Textures/test.png
    /// </summary>
    public string AssetPath;
    /// <summary>
    /// 资源加载路径，例如：Textures/test.png
    /// </summary>
    public string LoadPath;
    private Type m_assetType;
    public Type AssetType
    {
        get
        {
            if (null == m_assetType)
            {
                m_assetType = AssetDatabase.GetMainAssetTypeAtPath(AssetPath);
            }
            if (null == m_assetType)
            {
                m_assetType = typeof(UnityEngine.Object);
            }
            return m_assetType;
        }
    }
    public Object asset
    {
        get
        {
            if (null == m_asset)
            {
                m_asset = AssetBundleTool.LoadMainAssetAtPath(AssetPath);
            }
            return m_asset;
        }
        set
        {
            m_asset = value;
        }
    }
    private Object m_asset;

    public EditorAssetBundleData AssetBundleData;

    private EditorBundleType m_bundleType = EditorBundleType.None;
    public EditorBundleType BundleType
    {
        get
        {
            if (m_bundleType == EditorBundleType.None)
            {
                m_bundleType = AssetBundleTool.GetBundleTypeByPath(AssetPath);
            }
            return m_bundleType;
        }
        set
        {
            m_bundleType = value;
        }
    }


    /// <summary>
    /// 下级引用关系，引用了哪些资源，采用了递归收集
    /// </summary>
    public List<EditorAssetData> DownDependenceDataList;

    /// <summary>
    /// 下级依赖资源的路径，采用了递归收集
    /// </summary>
    public List<string> DownDependentPathList;
    /// <summary>
    /// 下级依赖资源的路径，未采用了递归收集
    /// </summary>
    public List<string> NoRecursiveDownDependentPathList;

    /// <summary>
    /// 
    /// </summary>
    private int m_validTopUpDependenceCount;

    private bool m_labelInited = false;
    private string m_Label;
    public string Label
    {
        get
        {
            if (!m_labelInited && string.IsNullOrEmpty(m_Label))
            {
                m_Label = AssetBundleTool.GetTheLabel(AssetPath);
                m_labelInited = true;
            }
            return m_Label;
        }
        set
        {
            m_Label = value;
        }
    }

    public bool BuildAsFolderName
    {
        get
        {
            return Label == EditorPackDef.Label_BuildAsFolderName;
        }
    }

    public bool IsEditorOnly
    {
        get
        {
            return Label == EditorPackDef.Label_EditorOnly;
        }
    }

    /// <summary>
    ///  单独打包
    /// </summary>
    public bool IsBuildSingle
    {
        get
        {
            return Label == EditorPackDef.Label_BuildSingle || Label == EditorPackDef.Label_LoopSingle;
        }
    }


    /// <summary>
    /// 一直向上递归，直到上级资源的上级资源数为0或上级资源为BuildSingle
    /// </summary>
    public List<EditorAssetData> caculateTop()
    {
        EditorAssetData _data = null;
        for (int i = 0; i < UpDependenceList.Count; ++i)
        {
            _data = UpDependenceList[i];
            if (_data.DownDependenceDataList.Contains(this) && this.DownDependenceDataList.Contains(_data))//跳出相互引用循环
            {
                Label = EditorPackDef.Label_BuildSingle;//强制设置为 BuildSingle
                Object _target = AssetBundleTool.GetObjectByPath(AssetPath);
                AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                Debug.LogError("存在循环引用，强制设置为BuildSingle:" + AssetPath);
                continue;
            }
            if (_data.IsBuildSingle)
            {
                if (!TopUpDependenceList.Contains(_data))
                {
                    TopUpDependenceList.Add(_data);
                }
            }
            else if (0 == _data.UpDependenceList.Count)
            {
                if (!TopUpDependenceList.Contains(_data))
                {
                    TopUpDependenceList.Add(_data);
                }
            }
            else
            {
                List<EditorAssetData> _list = _data.caculateTop();
                if (null != _list)
                {
                    for (int j = 0; j < _list.Count; ++j)
                    {
                        EditorAssetData _tmp = _list[j];
                        if (!TopUpDependenceList.Contains(_tmp))
                        {
                            TopUpDependenceList.Add(_tmp);
                        }
                    }
                }
            }
        }
        return TopUpDependenceList;
    }



    private int m_commonAssetCount = 0;
    private bool m_isCommonAsset;
    private bool m_isCommonAssetInited = false;
    /// <summary>
    /// 是否为公共资源 (上级累计引用计数大于1或被标记为CommonAsset， 则认为是公共资源，单独打包)
    /// </summary>
    public bool IsCommonAsset
    {
        get
        {
            if (!m_isCommonAssetInited)
            {
                if (0 == UpDependenceList.Count)
                {
                    m_commonAssetCount = 0;
                }
                else
                {
                    if (0 == m_commonAssetCount)
                    {
                        caculateTop();
                        TopUpDependenceList = TopUpDependenceList.OrderBy(v => v.AssetPath).ToList();
                        m_validTopUpDependenceCount = 0;
                        for (int i = 0; i < TopUpDependenceList.Count; ++i)
                        {
                            if (!TopUpDependenceList[i].Useless)
                            {
                                m_validTopUpDependenceCount++;
                            }
                        }
                        m_commonAssetCount = m_validTopUpDependenceCount;


                    }

                }
                m_isCommonAsset = m_commonAssetCount > 1 || AssetBundleTool.IstheLabel(AssetPath, EditorPackDef.Label_IsCommonAsset);
                m_isCommonAssetInited = true;
            }
            return m_isCommonAsset;
        }

        set
        {
            m_isCommonAsset = value;
        }
    }
    /// <summary>
    /// 是否只被场景引用
    /// </summary>
    public bool IsOnlyUsedByScene
    {
        get
        {
            if (UpDependenceList.Count > 0)
            {
                for (int i = 0; i < UpDependenceList.Count; ++i)
                {
                    EditorAssetData _data = UpDependenceList[i];
                    if (_data.BundleType != EditorBundleType.Scene)
                    {
                        return false;

                    }
                }
                return true;
            }
            return false;
        }
    }

    private string m_assignAssetBundleName = string.Empty;
    /// <summary>
    /// 指定的包名 包名与上级文件夹名称一致
    /// </summary>
    public string AssignAssetBundleName
    {

        get
        {
            if (string.IsNullOrEmpty(m_assignAssetBundleName))
            {
                m_assignAssetBundleName = EditorPackDef.Inited;

                List<string> _list = AssetBundleTool.GetTheLabelList(EditorPackDef.Label_BuildAsFolderName);
                for (int i = 0; i < _list.Count; ++i)
                {
                    string _path = _list[i];
                    if (AssetPath.Contains(_path))
                    {
                        _path = _path.Replace(EditorPackDef.AssetPathRoot, "");
                        if (AssetBundleTool.HasSuffix(_path))
                        {
                            string _suffix = AssetBundleTool.GetFileSuffix(_path);
                            string _name = AssetBundleTool.GetResourceName(_path);
                            _path = _path.Replace("/" + _name + _suffix, "");

                            string _folderName = AssetBundleTool.GetResourceName(_path);
                            _path = _path + "/" + _folderName;
                        }
                        else
                        {
                            _path = _path + "/" + AssetBundleTool.GetResourceName(_path);
                        }
                        m_assignAssetBundleName = _path + EditorPackDef.AssetBundleSuffix;
                        break;
                    }
                }

            }

            return m_assignAssetBundleName;
        }
    }

    private string m_finalAssetBundleName = "NULL";
    public string FinalAssetBundleName
    {
        get
        {

            if (m_finalAssetBundleName == string.Empty)
            {
                return m_finalAssetBundleName;
            }

            if ("NULL" != m_finalAssetBundleName)
            {
                m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                return m_finalAssetBundleName;
            }



            string _assetbundlename = AssetPath.Replace(EditorPackDef.AssetPathRoot, "");
            _assetbundlename = AssetBundleTool.DeletSuffix(_assetbundlename);
            _assetbundlename += EditorPackDef.AssetBundleSuffix;

            bool _isCom = IsCommonAsset;//必须放这里，不能改顺序

            if (BundleType == EditorBundleType.Prefab && AssetPath.Contains(EditorPackDef.AllShaders))
            {

                _assetbundlename = EditorPackDef.AllShaders + EditorPackDef.AssetBundleSuffix;
                m_finalAssetBundleName = _assetbundlename;
                m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                return m_finalAssetBundleName;
            }

            if (BundleType == EditorBundleType.Shader)
            {
                if (0 == m_validTopUpDependenceCount)
                {
                    m_finalAssetBundleName = string.Empty; //无用资源
                    return m_finalAssetBundleName;
                }
                _assetbundlename = EditorPackDef.AllShaders + EditorPackDef.AssetBundleSuffix;
                m_finalAssetBundleName = _assetbundlename;
                m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                return m_finalAssetBundleName;
            }
            else if (BundleType == EditorBundleType.Script)
            {
                _assetbundlename = EditorPackDef.AllScripts + EditorPackDef.AssetBundleSuffix;
                m_finalAssetBundleName = _assetbundlename;
                m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                return m_finalAssetBundleName;
            }

            if (BundleType == EditorBundleType.Scene)
            {
                m_finalAssetBundleName = String.Empty;
                return m_finalAssetBundleName;
            }

            if (IsBuildSingle)
            {
                m_finalAssetBundleName = _assetbundlename;
                m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                return m_finalAssetBundleName;
            }

            if (IsCommonAsset)
            {
                if (AssetBundleEditor.s_Allowredundance &&
                    m_validTopUpDependenceCount > 1 &&
                    m_validTopUpDependenceCount <= AssetBundleEditor.s_AssetDependentTimes)
                {

                    if (Size <= AssetBundleEditor.s_DomainSize)
                    {
                        m_finalAssetBundleName = String.Empty;
                        return m_finalAssetBundleName;
                    }
                }

                //BuildAsFolderName 只对公共资源作归档处理
                if (!string.IsNullOrEmpty(AssignAssetBundleName) && !AssignAssetBundleName.Equals(EditorPackDef.Inited))
                {
                    m_finalAssetBundleName = AssignAssetBundleName.Insert(AssignAssetBundleName.IndexOf('.'), "_Com");
                    m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                    return m_finalAssetBundleName;
                }

                if (1 == UpDependenceList.Count)//如果只有一个上级资源
                {
                    EditorAssetData _data = UpDependenceList[0];
                    if (_data.BundleType == EditorBundleType.Scene)
                    {
                        m_finalAssetBundleName = _data.FinalAssetBundleName;
                        m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                        return m_finalAssetBundleName;
                    }
                    else
                    {
                        m_finalAssetBundleName = _data.FinalAssetBundleName;
                        m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                        return m_finalAssetBundleName;
                    }
                }

                if (1 == m_validTopUpDependenceCount)//如果只有一个有效顶级资源
                {
                    EditorAssetData _data = UpDependenceList[0];
                    if (_data.BundleType == EditorBundleType.Scene)
                    {
                        m_finalAssetBundleName = _data.FinalAssetBundleName;
                        m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                        return m_finalAssetBundleName;
                    }
                    else
                    {
                        m_finalAssetBundleName = _data.FinalAssetBundleName;
                        m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                        return m_finalAssetBundleName;
                    }
                }

                m_finalAssetBundleName = _assetbundlename.Insert(_assetbundlename.IndexOf('.'), "_Com");
                m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                return m_finalAssetBundleName;
            }

            if (!string.IsNullOrEmpty(AssignAssetBundleName) && !AssignAssetBundleName.Equals(EditorPackDef.Inited))
            {
                m_finalAssetBundleName = AssignAssetBundleName.Insert(AssignAssetBundleName.IndexOf('.'), "_Bundle");
                m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                return m_finalAssetBundleName;
            }

            if (1 == UpDependenceList.Count)//如果只有一个上级资源
            {
                EditorAssetData _data = UpDependenceList[0];
                if (_data.BundleType == EditorBundleType.Scene)
                {
                    m_finalAssetBundleName = _data.FinalAssetBundleName;
                    m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                    return m_finalAssetBundleName;
                }
                else
                {
                    m_finalAssetBundleName = _data.FinalAssetBundleName;
                    m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                    return m_finalAssetBundleName;
                }
            }


            if (1 == m_validTopUpDependenceCount)//如果只有一个有效顶级资源
            {
                EditorAssetData _data = TopUpDependenceList[0];
                if (_data.BundleType == EditorBundleType.Scene)
                {
                    m_finalAssetBundleName = _data.FinalAssetBundleName;
                    m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                    return m_finalAssetBundleName;
                }
                else
                {
                    m_finalAssetBundleName = _data.FinalAssetBundleName;
                    m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
                    return m_finalAssetBundleName;
                }
            }




            if (0 == m_validTopUpDependenceCount)
            {
                if (!IsBuildSingle && !IsCommonAsset && BundleType != EditorBundleType.Prefab && BundleType != EditorBundleType.Script)
                {
                    Useless = true;
                }
            }

            if (Useless)
            {
                _assetbundlename = string.Empty; //无用资源
            }
            else
            {

            }


            m_finalAssetBundleName = _assetbundlename;
            m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
            return m_finalAssetBundleName;
        }
        set
        {

            m_finalAssetBundleName = value;
            m_finalAssetBundleName = removeSpaceChar(m_finalAssetBundleName);
        }
    }

    private string removeSpaceChar(string value)
    {

        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        value= value.Replace(" ", "_");
        value = value.Replace("#", "_");
        value = value.Replace("%", "_");
        if (value.Length > 100)
            value = "LongPach/" + value.GetHashCode();
        return value;
    }

    /// <summary>
    /// 如果被标记为单打且只有一个顶级上级资源，不丢包
    /// </summary>
    public bool IsSolid
    {
        get
        {
            bool _common = IsCommonAsset;//不能删
            return AssetType == typeof(AudioClip) ||
             1 == m_validTopUpDependenceCount && FinalAssetBundleName != TopUpDependenceList[0].FinalAssetBundleName;
        }
    }
    /// <summary>
    /// 无用的资源
    /// </summary>
    public bool Useless = false;

    private float m_size = float.MaxValue;
    private float Size
    {
        get
        {

            if (m_size == float.MaxValue)
            {
                int _size = 0;
                AssetBundleTool.GetFileMD5(AssetPath, ref _size);

                m_size = _size / 1024f;
            }

            return m_size;
        }
    }

    public EditorAssetData()
    {

        TopUpDependenceList = new List<EditorAssetData>();
        UpDependenceList = new List<EditorAssetData>();
        DownDependenceDataList = new List<EditorAssetData>();
    }

    public EditorAssetData(string path, List<string> recursiveList, List<string> noRecurseveList, EditorBundleType type = EditorBundleType.None)
    {
        AssetPath = path;
        LoadPath = AssetBundleTool.DeletSuffix(AssetPath.Replace(EditorPackDef.AssetPathRoot, string.Empty));
        TopUpDependenceList = new List<EditorAssetData>();
        UpDependenceList = new List<EditorAssetData>();
        DownDependenceDataList = new List<EditorAssetData>();
        DownDependentPathList = recursiveList;
        NoRecursiveDownDependentPathList = noRecurseveList;
        AssetBundleData = new EditorAssetBundleData();

        m_bundleType = type;

    }





    /// <summary>
    /// 标记此资源为公共资源
    /// </summary>
    /// <param name="isCommon"></param>
    public void SetAsCommonAsset(bool isCommon)
    {
        m_commonAssetCount = isCommon ? 2 : 0;
    }

    /// <summary>
    /// 增加上级引用资源
    /// </summary>
    /// <param name="info"></param>
    public void AddUpDependence(EditorAssetData info)
    {
        if (!UpDependenceList.Exists(v => v.AssetPath == info.AssetPath))
        {
            UpDependenceList.Add(info);
        }
    }

    /// <summary>
    /// 增加下级引用资源
    /// </summary>
    /// <param name="info"></param>
    public void AddDownDependence(EditorAssetData info)
    {
        if (!DownDependenceDataList.Exists(v => v.AssetPath == info.AssetPath))
        {
            DownDependenceDataList.Add(info);
        }
    }


    /// <summary>
    /// 获取下级资源中为单独打包的资源，不和自己打一个包
    /// </summary>
    /// <returns></returns> 
    public List<EditorAssetData> GetDownDependenceListIndepentAsset()
    {

        List<EditorAssetData> _list = new List<EditorAssetData>();
        int _count = DownDependenceDataList.Count;
        for (int i = 0; i < _count; ++i)
        {
            if (DownDependenceDataList[i].FinalAssetBundleName != FinalAssetBundleName)
            {
                _list.Add(DownDependenceDataList[i]);
            }
        }
        return _list;
    }


    public List<EditorAssetData> GetDownDependenceListUnIndepentAsset()
    {

        List<EditorAssetData> _list = new List<EditorAssetData>();
        int _count = DownDependenceDataList.Count;
        for (int i = 0; i < _count; ++i)
        {
            if (DownDependenceDataList[i].FinalAssetBundleName == FinalAssetBundleName)
            {
                _list.Add(DownDependenceDataList[i]);
            }
        }
        return _list;
    }

    /// <summary>
    /// 是否只被场景引用
    /// </summary>
    /// <returns></returns>
    private bool OnlyBeUsedByScene()
    {
        bool _flag = true;
        if (null != UpDependenceList && UpDependenceList.Count > 0)
            for (int i = 0; i < UpDependenceList.Count; ++i)
            {
                EditorAssetData _data = UpDependenceList[i];
                if (_data.BundleType != EditorBundleType.Scene)
                {
                    _flag = false;
                    break;
                }
            }
        return _flag;
    }
}



