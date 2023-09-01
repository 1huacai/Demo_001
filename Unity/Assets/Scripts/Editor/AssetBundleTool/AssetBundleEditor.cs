using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;
using IO;

/// <summary>
/// 
/// </summary>
public class AssetBundleEditor : EditorWindow
{

    private enum modelType
    {

        BuildAssetBundle = 0,

        Other,

        Setting,

        end,
    }
    private int m_modeLabelsIndex = 0;
    private string[] m_ModeLabels = new[] { "打包", "其他", "设置" };
    /// <summary>
    /// 默认增量打包，不使用typetredd，关闭名字和后缀适配
    /// </summary>
    private static BuildAssetBundleOptions s_BuildAssetBundleOptions = BuildAssetBundleOptions.DeterministicAssetBundle |
        BuildAssetBundleOptions.ChunkBasedCompression/*|
        BuildAssetBundleOptions.DisableWriteTypeTree |
        BuildAssetBundleOptions.DisableLoadAssetByFileName |
        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension*/;

    private BuildAssetBundleOptions m_FinalBuildAssetBundleOptions
    {
        get
        {
            return s_BuildAssetBundleOptions;
        }
    }
    private static BuildTarget s_targetPlantform
    {
        get
        {
            return EditorUserBuildSettings.activeBuildTarget;
        }
    }


    /// <summary>
    /// 使用偏移加密
    /// </summary>
    public static bool s_UseOffsetEncryption = true;

    private List<EditorAssetData> m_SceneList = new List<EditorAssetData>();


    private List<EditorAssetData> m_UselessEditorAssetDataList = new List<EditorAssetData>();


    private int m_AssetSize = 5;//单位 字节 KB
    private int m_AssetDependentTimes = 5;//

    /// <summary>
    ///  是否允许冗余
    /// </summary>
    public static bool s_Allowredundance = false;
    public static int s_DomainSize;
    public static int s_AssetDependentTimes;

    /// <summary>
    /// 公共资源合包上限
    /// </summary>
    public static int s_CommonAssetMax = 10;

    /// <summary>
    /// 打包日志 
    /// </summary> 
    private StringBuilder m_packLogSB = new StringBuilder();


    /// <summary>
    /// 开始打包时的时间
    /// </summary>
    private static DateTime s_startDateTime;



    private readonly int m_height = 20;


    [MenuItem("AssetBundleTool/资源打包")]
    public static void AddWindow()
    {
        AssetBundleEditor w = GetWindow<AssetBundleEditor>("打包 ");
        w.minSize = new Vector2(500, 200);

        int _options = PlayerPrefs.GetInt("BuildAssetBundleOptions");

        s_BuildAssetBundleOptions = (BuildAssetBundleOptions)_options;

        s_BuildAssetBundleOptions = s_BuildAssetBundleOptions == BuildAssetBundleOptions.None ? BuildAssetBundleOptions.DeterministicAssetBundle : s_BuildAssetBundleOptions;
    }

    public void OnGUI()
    {
        GUILayout.Space(m_height);
        EditorGUILayout.BeginHorizontal();
        m_modeLabelsIndex = GUILayout.SelectionGrid(m_modeLabelsIndex, m_ModeLabels, m_ModeLabels.Length, GUILayout.Height(m_height));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(m_height);
        GUILayout.Space(m_height);
        if (m_modeLabelsIndex == modelType.BuildAssetBundle.GetHashCode())
        {
            packAllEditorWindow();
        }
        else if (m_modeLabelsIndex == modelType.Other.GetHashCode())
        {
            otherEditorWindow();
        }
        if (m_modeLabelsIndex == modelType.Setting.GetHashCode())
        {
            SettingEditorWindow();
        }
        Repaint();
    }

    #region 打包
    /// <summary>
    /// 完整打包
    /// </summary>
    private void packAllEditorWindow()
    {
        GUILayout.BeginArea(new Rect(20, 50, position.width - 40, position.height - 50));
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("代码打包方式:", GUILayout.Width(100));
        AssetBundleTool.CodeType = (codeType)EditorGUILayout.EnumPopup(AssetBundleTool.CodeType,GUILayout.Width(70));
        #region 单打代码
        if (GUILayout.Button("单打代码"))
        {
            if (EditorUtility.DisplayDialog("提示", "你确定要单打代码吗？", "确定", "取消"))
            {
                EditorApplication.delayCall += PackCodeOnly;
            }
        }
        #endregion
        if (GUILayout.Button("单打配置"))
        {
            if (EditorUtility.DisplayDialog("提示", "你确定单打配置吗？", "确定", "取消"))
            {
                EditorApplication.delayCall += PackConfigOnly;
            }
        }

        if (GUILayout.Button("完整打包"))
        {
            if (EditorUtility.DisplayDialog("提示", "你确定要打包？", "确定", "取消"))
            {
                EditorApplication.delayCall += PackAll;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("生成热更MD5列表"))
        {
            //BuildAssetBundle.CreateMoveList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("一键生成热更包"))
        {
            EditorApplication.delayCall += EditorBuildHotfix;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("复制代码资源到Release工程"))
        {
            if (EditorUtility.DisplayDialog("提示", "你确定要复制代码资源到Release工程吗？", "确定", "取消"))
            {
                EditorApplication.delayCall += CopyResources;
            }
        }
        if (GUILayout.Button("备份热更原始ab包"))
        {
            if (EditorUtility.DisplayDialog("提示", "你确定要备份热更原始ab包吗？", "确定", "取消"))
            {
                EditorApplication.delayCall += CopyHotFixOrginalResources;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        HelpEditorWindow();
        GUILayout.EndArea();

    }
    /// <summary>
    /// 获取 Assets/Resources 文件夹下所有指定标签的资源
    /// </summary>
    /// <param name="label"></param>
    /// <returns></returns>
    private List<string> GetLabelAssets(string label)
    {

        string _folder = Application.dataPath.Replace(EditorPackDef.Assets_0, "");

        string[] _GUIDArray = AssetDatabase.FindAssets("l:" + label, new string[1] { "Assets/Resources" });
        List<string> _AssetPathList = new List<string>();

        for (int i = 0; i < _GUIDArray.Length; ++i)
        {
            string _path = AssetDatabase.GUIDToAssetPath(_GUIDArray[i]);
            string _temp = Path.Combine(_folder, _path);
            _temp = _temp.Replace(@"\", "/");
            if (AssetBundleTool.IsDirectory(_temp))
            {
                List<string> _tempList = AssetBundleTool.GetUsefulAssets(_temp);
                for (int j = 0; j < _tempList.Count; ++j)
                {
                    string _str1 = _tempList[j].Replace(_folder, "");
                    if (!_AssetPathList.Contains(_str1))
                    {
                        _AssetPathList.Add(_str1);
                    }
                }
                continue;
            }

            if (!_AssetPathList.Contains(_path))
            {
                _AssetPathList.Add(_path);
            }
        }

        return _AssetPathList;
    }
    private List<string> GetLabelAssets(string[] label, string excludeLabel)
    {

        string _folder = Application.dataPath.Replace(EditorPackDef.Assets_0, "");

        StringBuilder labels = new StringBuilder();
        for (int i = 0; i < label.Length; i++)
        {
            labels.Append("l:" + label[i]);
            if (i + 1 != label.Length)
                labels.Append(" ");
        }
        string[] _GUIDArray = AssetDatabase.FindAssets(labels.ToString(), new string[1] { "Assets/Resources" });
        List<string> _AssetPathList = new List<string>();

        for (int i = 0; i < _GUIDArray.Length; ++i)
        {
            string _path = AssetDatabase.GUIDToAssetPath(_GUIDArray[i]);
            string _temp = Path.Combine(_folder, _path);
            _temp = _temp.Replace(@"\", "/");
            if (AssetBundleTool.IsDirectory(_temp))
            {
                List<string> _tempList = AssetBundleTool.GetUsefulAssets(_temp);
                for (int j = 0; j < _tempList.Count; ++j)
                {
                    string _str1 = _tempList[j].Replace(_folder, "");
                    if (!_AssetPathList.Contains(_str1))
                    {
                        _AssetPathList.Add(_str1);
                    }
                }
                continue;
            }

            if (!_AssetPathList.Contains(_path))
            {
                _AssetPathList.Add(_path);
            }
        }

        return _AssetPathList;
    }
    private List<string> FilteRepeat(List<string> editorList, List<string> list)
    {
        if (null == editorList || null == list)
        {
            return new List<string>();
        }

        for (int i = list.Count - 1; i >= 0; --i)
        {
            if (editorList.Contains(list[i]))
            {
                list.RemoveAt(i);
            }
        }

        return list;
    }

    private Dictionary<string, EditorAssetData> GetDependenciesDic(List<string> list)
    {
        if (null == list)
        {
            return new Dictionary<string, EditorAssetData>();
        }
        Dictionary<string, EditorAssetData> _dic = new Dictionary<string, global::EditorAssetData>();

        List<string> _newList = new List<string>();

        int _count = list.Count;
        for (int i = 0; i < _count; ++i)
        {
            List<string> _recursiveList = new List<string>();
            List<string> _noRecursiveList = new List<string>();
            string _assetPath = list[i];

            EditorBundleType _type = AssetBundleTool.GetBundleTypeByPath(_assetPath);
            if (_type == EditorBundleType.Script)
            {
                continue;
            }

            if (_type != EditorBundleType.Script &&
                    _type != EditorBundleType.Texture &&
                      _type != EditorBundleType.Animation &&
            //  _type != EditorBundleType.Model && 
            _type != EditorBundleType.TextAsset &&
             _type != EditorBundleType.Audio)
            {
                _recursiveList = AssetDatabase.GetDependencies(_assetPath).ToList();
                _recursiveList.Remove(_assetPath);//去掉自己
                _noRecursiveList = AssetDatabase.GetDependencies(_assetPath, false).ToList();

                for (int j = 0; j < _recursiveList.Count; ++j)
                {
                    if (!_newList.Contains(_recursiveList[j]) && !list.Contains(_recursiveList[j]))
                    {
                        _newList.Add(_recursiveList[j]);
                    }
                }
            }

            EditorAssetData _data = new EditorAssetData(_assetPath, _recursiveList, _noRecursiveList, _type);
            _dic.Add(_assetPath, _data);
        }

        for (int i = 0; i < _newList.Count; ++i)
        {
            List<string> _recursiveList = new List<string>();
            List<string> _noRecursiveList = new List<string>();
            string _assetPath = _newList[i];

            if (_dic.ContainsKey(_assetPath))
            {
                continue;
            }

            EditorBundleType _type = AssetBundleTool.GetBundleTypeByPath(_assetPath);
            if (_type == EditorBundleType.Script)
            {
                continue;
            }

            if (_type != EditorBundleType.Script &&
                    _type != EditorBundleType.Texture &&
                      _type != EditorBundleType.Animation &&
            // _type != EditorBundleType.Model &&
            _type != EditorBundleType.TextAsset &&
             _type != EditorBundleType.Audio)
            {
                _recursiveList = AssetDatabase.GetDependencies(_assetPath).ToList();
                _recursiveList.Remove(_assetPath);//去掉自己
                _noRecursiveList = AssetDatabase.GetDependencies(_assetPath, false).ToList();

            }

            EditorAssetData _data = new EditorAssetData(_assetPath, _recursiveList, _noRecursiveList, _type);
            _dic.Add(_assetPath, _data);
        }

        return _dic;
    }


    private List<string> GetLabeledAssetPathList()
    {
        Dictionary<string, EditorAssetData> _dic = new Dictionary<string, EditorAssetData>();


        List<string> _list = new List<string>();
        List<string> _list1 = new List<string>();
        //找到所有 EditorOnly 资源
        List<string> _EditorOnlyAssetPathList = GetLabelAssets(EditorPackDef.Label_EditorOnly);
        _list1 = GetLabelAssets(new string[] { EditorPackDef.Label_BuildSingle, EditorPackDef.Label_IsCommonAsset, EditorPackDef.Label_BuildAsFolderName }, EditorPackDef.Label_EditorOnly);
        _list1 = FilteRepeat(_EditorOnlyAssetPathList, _list1);
        AssetBundleTool.AddListToTargetList(_list, _list1);

        return _list;
    }

    /// <summary>
    /// 完整打包
    /// </summary>
    private void PackAll()
    {

        PackAllWithoutTip();
        if (EditorUtility.DisplayDialog("提示", "打包完成！", "确定", "取消"))
        {
            ClearTmp();
            ClearCache();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }


    private bool CollectAssets()
    {

        s_startDateTime = DateTime.Now;
        m_packLogSB.Append("开始打包：" + DateTime.Now.GetDateTimeFormats('s')[0].ToString() + "\n\n");
        initPackAllEviroment();
        DateTime _time = DateTime.Now;
        List<string> _LabeledAssetPathList = GetLabeledAssetPathList();


        if (null == _LabeledAssetPathList)
        {
            return false;
        }

        Dictionary<string, EditorAssetData> _dic = GetDependenciesDic(_LabeledAssetPathList);

        double _time1 = (DateTime.Now - _time).TotalSeconds;

        m_packLogSB.Append("收集资源耗时：" + _time1 + "\r\n");

        List<string> _allList = new List<string>();
        List<string> _repeatList = new List<string>();
        foreach (string key in _dic.Keys)
        {
            string _path = AssetBundleTool.DeletSuffix(key);

            if (!_allList.Contains(_path))
            {
                _allList.Add(_path);
            }
            else
            {
                if (!_repeatList.Contains(_path))
                {
                    _repeatList.Add(_path);
                }
            }
        }

        if (_repeatList.Count > 0)
        {
            for (int i = 0; i < _repeatList.Count; ++i)
            {
                Debug.LogError(_repeatList[i]);
            }

            if (EditorUtility.DisplayDialog("提示", "发现了同名资源，是否修改资源名？！", "修改", "继续打包"))
            {
                EditorUtility.ClearProgressBar();
                return false;
            }
        }


        EditorAssetDataManager.Inst.TargetAssetDataDic = collateABAssetInfoRelationship(_dic);
        return true;
    }
    /// <summary>
    /// 标记配置表
    /// </summary>
    private void InitConfig()
    {
        Object _ConfigByte = AssetBundleTool.GetObjectByPath("Assets/Resources/ConfigByte");
        Object _Game = AssetBundleTool.GetObjectByPath("Assets/Resources/ConfigByte/Game");
        Object _Language = AssetBundleTool.GetObjectByPath("Assets/Resources/ConfigByte/Language");

        AssetBundleTool.ClearLabel(_ConfigByte);
        AssetBundleTool.ClearLabel(_Game);
        AssetBundleTool.ClearLabel(_Language);
        AssetBundleTool.AddLabel(_ConfigByte, EditorPackDef.Label_BuildAsFolderName);
        AssetBundleTool.AddLabel(_Game, EditorPackDef.Label_BuildAsFolderName);
        AssetBundleTool.AddLabel(_Language, EditorPackDef.Label_BuildAsFolderName);
    }
    /// <summary>
    /// 标记shader
    /// </summary>
    private void InitShader()
    {
        string _path = "Assets/Resources/" + EditorPackDef.AllShaders + ".prefab";
        var _AllShadersPrefab = AssetBundleTool.LoadMainAssetAtPath(_path);

        if (null == _AllShadersPrefab)
        {
            _AllShadersPrefab = AssetBundleTool.CreatePrefab(EditorPackDef.AllShaders, _path);
        }
        AssetBundleTool.AddLabel(_AllShadersPrefab, EditorPackDef.Label_BuildSingle);
    }
    private void PackAllWithoutTip()
    {
        InitConfig();
        InitShader();
        //收集资源
        if (!CollectAssets())
            return;
        PackCode();
        List<EditorAssetData> _newAssetDataList = EditorAssetDataManager.Inst.AllAssetDataDic.Values.ToList();

        string _maniFest = string.Empty;
        string _bundleData = string.Empty;
        /*编辑器工具 查找资源信息和ab包信息****************************/
        //生成ManiFestText文本
        _maniFest = AssetBundleTool.CreateManiFestText(_newAssetDataList);
        BuildManiFestText(Application.dataPath + "/" + EditorPackDef.ManiFestTextPath, _maniFest);
        //生成AssetBundleText文本
        _bundleData = AssetBundleTool.CreateAssetBundleDataText(_newAssetDataList);
        BuildAssetBundleDataText(Application.dataPath + "/" + EditorPackDef.AssetBundleTextPath, _bundleData);
        /*编辑器工具 查找资源信息和ab包信息****************************/
        //创建mainifest信息
        AssetBundleTool.CreateAssetManifest(_newAssetDataList);

        PackTargetAssets(EditorAssetDataManager.Inst.TargetAssetDataDic);

        AssetBundleTool.DeletAllManiFest(Application.dataPath + "/StreamingAssets/StreamingResources");
        AssetBundleTool.DeletUnusedAssetBundle(Application.dataPath + "/StreamingAssets/StreamingResources");


    }
    #region 单打配置
    /// <summary>
    /// 单打配置
    /// </summary>
    private void PackConfigOnly()
    {

        if (check())
        {
            return;
        }

        //配置表转二进制
        //ConfigToolNew.GinerateConfig2Bytes();
        AssetBundleTool.DelectDir(Application.dataPath + "/StreamingAssets/StreamingResources/configbyte");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        s_startDateTime = DateTime.Now;
        m_packLogSB.Append("开始打包：" + DateTime.Now.GetDateTimeFormats('s')[0].ToString());


        List<string> _configPathList = null;
        _configPathList = AssetBundleTool.GetTheResources(Application.dataPath + "/Resources/ConfigByte", ".bytes");
        if (null == _configPathList)
        {
            return;
        }
        Dictionary<string, List<string>> _abDic = new Dictionary<string, List<string>>();
        List<AssetBundleBuild> _list = new List<AssetBundleBuild>();
        int _count = _configPathList.Count;
        for (int i = 0; i < _count; i++)
        {
            string _path = _configPathList[i];

            string _assetBundleName = string.Empty;
            if (_path.Contains("BattleSystem/"))
            {
                _assetBundleName = "configbyte/battlesystem/battlesystem_bundle" + EditorPackDef.AssetBundleSuffix;
            }
            else if (_path.Contains("Game/"))
            {
                _assetBundleName = "configbyte/game/game_bundle" + EditorPackDef.AssetBundleSuffix;
                if (_path.Contains("Game/VersionConfig"))
                {
                    _assetBundleName = "configbyte/game/versionconfig" + EditorPackDef.AssetBundleSuffix;
                }
            }
            else if (_path.Contains("Language/"))
            {
                _assetBundleName = "configbyte/language/language_bundle" + EditorPackDef.AssetBundleSuffix;
            }
            else
            {
                _assetBundleName = "configbyte/configbyte_bundle" + EditorPackDef.AssetBundleSuffix;
            }
            AssetBundleTool.SetABInfo(_abDic, _assetBundleName, _path);
        }
        createTargetAssetBundle(AssetBundleTool.ABInfoDicToArray(_abDic).ToArray());

        m_packLogSB.Append("\r\n");
        m_packLogSB.Append("Config总数：" + _count);
        m_packLogSB.Append("\r\n");
        double _costTime = (DateTime.Now - s_startDateTime).TotalSeconds;
        m_packLogSB.Append("打包完毕：" + DateTime.Now.GetDateTimeFormats('s')[0].ToString() + "\n" + "总耗时：" + _costTime + "秒");


        BuildPackLogText(m_packLogSB.ToString());


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            //AssetBundleTool.EncryptyGameFiles(Application.dataPath + "/StreamingAssets/StreamingResources/configbyte");



        if (EditorUtility.DisplayDialog("提示", "打包配置表完成！", "确定", "取消"))
        {
            ClearTmp();
            ClearCache();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
    }
    #endregion

    #region 代码
    private void PackCode()
    {
        if (AssetBundleTool.CodeType == codeType.injectFix)
        {
            List<string> _pathList = HandleInject();
            AssetBundleBuild _ab = new AssetBundleBuild();
            _ab.assetBundleName = EditorPackDef.AllInject + EditorPackDef.AssetBundleSuffix;
            _ab.assetNames = _pathList.ToArray();
            createTargetAssetBundle(new AssetBundleBuild[1] { _ab });
        }
        //else if (AssetBundleTool.CodeType == codeType.xlua)
        //{
        //    string _assetBundleName = "luascriptsbyte" + EditorPackDef.AssetBundleSuffix;
        //    List<string> _pathList = HandleLua();

        //    AssetBundleBuild _ab = new AssetBundleBuild();
        //    _ab.assetBundleName = _assetBundleName;
        //    _ab.assetNames = _pathList.ToArray();

        //    createTargetAssetBundle(new AssetBundleBuild[1] { _ab });
        //}
        else
        {
            List<string> _pathList = HandleScripts();
            AssetBundleBuild _ab = new AssetBundleBuild();
            _ab.assetBundleName = EditorPackDef.AllScripts + EditorPackDef.AssetBundleSuffix;
            _ab.assetNames = _pathList.ToArray();
            createTargetAssetBundle(new AssetBundleBuild[1] { _ab });
        }

    }
    private void PackCodeOnly()
    {
        initBaseEviroment();
        if (AssetBundleTool.CodeType == codeType.injectFix)
        {
            List<string> _pathList = HandleInject();
            AssetBundleBuild _ab = new AssetBundleBuild();
            _ab.assetBundleName = EditorPackDef.AllInject + EditorPackDef.AssetBundleSuffix;
            _ab.assetNames = _pathList.ToArray();
            createTargetAssetBundle(new AssetBundleBuild[1] { _ab });
        }
        //else if (AssetBundleTool.CodeType == codeType.xlua)
        //{
        //    string _assetBundleName = "luascriptsbyte" + EditorPackDef.AssetBundleSuffix;
        //    List<string> _pathList = HandleLua();

        //    AssetBundleBuild _ab = new AssetBundleBuild();
        //    _ab.assetBundleName = _assetBundleName;
        //    _ab.assetNames = _pathList.ToArray();

        //    createTargetAssetBundle(new AssetBundleBuild[1] { _ab });
        //    //AssetBundleTool.EncryptyGameFiles(Application.dataPath + "/StreamingAssets/StreamingResources/" + _assetBundleName);
        //}
        else
        {
            List<string> _pathList = HandleScripts();
            AssetBundleBuild _ab = new AssetBundleBuild();
            _ab.assetBundleName = EditorPackDef.AllScripts + EditorPackDef.AssetBundleSuffix;
            _ab.assetNames = _pathList.ToArray();

            createTargetAssetBundle(new AssetBundleBuild[1] { _ab });
            //if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                //AssetBundleTool.EncryptyGameFiles(Application.dataPath + "/StreamingAssets/StreamingResources/allscripts.assetbundle");
        }
        //if (lua)
        //{
        //    List<string> _pathList = HandleLua();

        //    AssetBundleBuild _ab = new AssetBundleBuild();
        //    _ab.assetBundleName = "xluafixfiles/xluafixfiles_bundle" + EditorPackDef.AssetBundleSuffix;
        //    _ab.assetNames = _pathList.ToArray();

        //    createTargetAssetBundle(new AssetBundleBuild[1] { _ab });
        //    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        //        AssetBundleTool.EncryptyGameFiles(Application.dataPath + "/StreamingAssets/StreamingResources/xluafixfiles/xluafixfiles_bundle.assetbundle");
        //}


        if (EditorUtility.DisplayDialog("提示", "打包代码完成！", "确定", "取消"))
        {
            ClearTmp();
            ClearCache();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private List<string> HandleLua()
    {
        string path = Application.dataPath + "/Resources/Lua/LuaScripts";
        string _bytePath = Application.dataPath + "/Resources/Lua/LuaScriptsByte";
        if (Directory.Exists(_bytePath))
            Directory.CreateDirectory(_bytePath);
        string[] allfiles = Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);

        string _assetBundleName = "luascriptsbyte" + EditorPackDef.AssetBundleSuffix;

        List<string> newLuaPath = new List<string>();
        for (int i = 0; i < allfiles.Length; i++)
        {
            string oldPath = allfiles[i].Replace("\\", "/");

            string oldAssetPath = oldPath.Replace(Application.dataPath, EditorPackDef.Assets_0);

            if (oldPath.Contains("Lua/LuaScripts/Editor"))
                continue;

            string targetPath = oldPath.Replace("Resources/Lua/LuaScripts", "Resources/Lua/LuaScriptsByte").Replace(".lua", ".bytes");
            //string str = UIO.ReadAllText(oldPath);
            byte[] data = UIO.ReadAllBytes(oldPath); //Encoding.Default.GetBytes(str);
            UIO.WriteAllBytes(targetPath, data);

            string newAssetPath = targetPath.Replace(Application.dataPath, EditorPackDef.Assets_0);
            newLuaPath.Add(newAssetPath);
            Object _target = AssetBundleTool.GetObjectByPath(newAssetPath);
            //AssetBundleTool.AddLabel(_target, sing);
        }
        return newLuaPath;
    }

    private List<string> HandleInject()
    {

        List<string> _assetPathList = null;
        string rootPath = Application.dataPath + "/Resources/Inject";
        _assetPathList = AssetBundleTool.GetTheResources(rootPath, ".patch.bytes");
        if (null == _assetPathList)
        {
            return null;
        }

        List<string> _pathList = new List<string>();
        for (int i = 0; i < _assetPathList.Count; ++i)
        {
            string _path = _assetPathList[i];
            _path = _path.Replace(".dll", ".bytes");
            _path = _path.Replace("Resources/Inject", "tmp");
            AssetDatabase.CopyAsset(_assetPathList[i], _path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string _dllPath = Application.dataPath.Replace("Assets", "") + _path;
            byte[] _fanal = File.ReadAllBytes(_dllPath);
            File.WriteAllBytes(_dllPath, _fanal);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetBundleTool.SetAssetBundleName(_path, EditorPackDef.AllInject + EditorPackDef.AssetBundleSuffix);
            _pathList.Add(_path);
        }

        return _pathList;
    }
    private List<string> HandleScripts()
    {

        List<string> _assetPathList = null;
        _assetPathList = AssetBundleTool.GetTheResources(Application.dataPath + "/Resources/Plugins", ".dll");
        if (null == _assetPathList)
        {
            return null;
        }

        List<string> _pathList = new List<string>();
        for (int i = 0; i < _assetPathList.Count; ++i)
        {
            string _path = _assetPathList[i];
            if (_path.ToLower().Contains("editor") ||
                _path.Contains("FrameWorkScriptableObject") ||
                      _path.Contains("PostProcessing"))
            {
                continue;
            }

            _path = _path.Replace(".dll", ".bytes");
            _path = _path.Replace("Resources/Plugins", "tmp");
            AssetDatabase.CopyAsset(_assetPathList[i], _path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string _dllPath = Application.dataPath.Replace("Assets", "") + _path;
            byte[] _fanal = AssetBundleTool.EncryptyFile(File.ReadAllBytes(_dllPath), 0xBB);
            File.WriteAllBytes(_dllPath, _fanal);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _pathList.Add(_path);
            AssetBundleTool.SetAssetBundleName(_path, EditorPackDef.AllScripts + EditorPackDef.AssetBundleSuffix);
        }

        return _pathList;
    }
    #endregion

    private Dictionary<string, List<Object>> m_AssetBundleNameDic = new Dictionary<string, List<UnityEngine.Object>>();
    private List<string> m_allAssetList = new List<string>();

    private void GetAllRelatedObjects(Dictionary<string, List<Object>> dic, List<Object> list)
    {
        if (null == dic || null == list || 0 == list.Count)
        {
            return;
        }

        int _count = m_allAssetList.Count;

        List<string> _pathList = new List<string>();
        List<string> _assetBundleNameList = new List<string>();
        List<Object> _assetList = new List<UnityEngine.Object>();

        for (int i = 0; i < list.Count; ++i)
        {
            string _pathName = AssetBundleTool.GetAssetPath(list[i]);
            if (!_pathList.Contains(_pathName))
            {
                _pathList.Add(_pathName);
            }

        }

        string[] _allPathArray = AssetDatabase.GetDependencies(_pathList.ToArray());

        for (int i = 0; i < _allPathArray.Length; ++i)
        {
            string _assetBundleName = AssetBundleTool.GetAssetBundleName(_allPathArray[i]);
            if (string.IsNullOrEmpty(_assetBundleName))
            {
                continue;
            }

            if (!_assetBundleNameList.Contains(_assetBundleName))
            {
                _assetBundleNameList.Add(_assetBundleName);
            }
        }

        for (int i = 0; i < _assetBundleNameList.Count; ++i)
        {
            string[] _pathArray = AssetDatabase.FindAssets("b:" + _assetBundleNameList[i]);
            for (int j = 0; j < _pathArray.Length; ++j)
            {
                string _assetPath = AssetDatabase.GUIDToAssetPath(_pathArray[j]);
                if (!m_allAssetList.Contains(_assetPath))
                {
                    m_allAssetList.Add(_assetPath);
                }
                else
                {
                    continue;
                }

                Object _asset = AssetBundleTool.GetObjectByPath(_assetPath);
                if (!_assetList.Contains(_asset))
                    _assetList.Add(_asset);
                AddToBuildDic(m_AssetBundleNameDic, _asset);
            }
        }

        if (m_allAssetList.Count != _count)
        {
            GetAllRelatedObjects(m_AssetBundleNameDic, _assetList);
        }


    }




    private void AddToBuildDic(Dictionary<string, List<Object>> dic, Object obj)
    {
        if (null == dic || null == obj)
        {
            return;
        }

        string _key = AssetBundleTool.GetAssetBundleName(obj);
        if (string.IsNullOrEmpty(_key))
        {
            Debug.LogError("AssetBundleName 不能为空。");
            return;
        }
        List<Object> _list = null;
        if (!dic.TryGetValue(_key, out _list))
        {
            _list = new List<Object>();
            dic.Add(_key, _list);
        }

        if (!_list.Contains(obj))
        {
            _list.Add(obj);
        }

    }


    #endregion

    #region 其他
    private void otherEditorWindow()
    {
        GUILayout.BeginArea(new Rect(20, 50, position.width - 40, position.height - 50));
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("清理AssetLabels"))
        {
            if (EditorUtility.DisplayDialog("提示", "是否清理所选资源或文件夹下的所有资源标签?", "确定", "取消"))
            {
                EditorApplication.delayCall += clearAssetLabels;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("生成link"))
        {
            CreateLink.CreateDllList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();



        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("加密"))
        {
            EditorApplication.delayCall += EditorEncrypty;
        }

        if (GUILayout.Button("解密"))
        {
            EditorApplication.delayCall += EditorDecrypty;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("删除冗余资源"))
        {
            EditorApplication.delayCall += EditorDeletUnusedFile;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("刷新"))
        {
            EditorApplication.delayCall += EditorRefresh;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();



        GUILayout.EndArea();


    }
    #endregion

    #region 设置

    private void SettingEditorWindow()
    {

        #region 设置压缩模式
        GUILayout.BeginArea(new Rect(20, 50, position.width - 40, position.height - 50));
        s_BuildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(
            "BuildAssetBundleOptions:",
         s_BuildAssetBundleOptions);


        GUILayout.EndArea();
        #endregion


        GUILayout.BeginArea(new Rect(20, 100, position.width - 40, position.height - 50));
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        s_UseOffsetEncryption = GUILayout.Toggle(s_UseOffsetEncryption, "使用offset偏移加密");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        GUIStyle _style = new GUIStyle();
        _style.normal.textColor = Color.red;
        EditorGUILayout.LabelField(
            string.Format("若公共资源小于{0}KB且被引用次数大于1且小于{1}次，则公共资源不单独打包，而是与依赖资源打一起.", m_AssetSize, m_AssetDependentTimes),
            _style);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        s_Allowredundance = EditorGUILayout.Toggle("是否允许冗余打包", s_Allowredundance);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        m_AssetSize = EditorGUILayout.IntField("资源大小(KB)", m_AssetSize);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        m_AssetDependentTimes = EditorGUILayout.IntField("被引用次数", m_AssetDependentTimes);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        s_DomainSize = m_AssetSize;
        m_AssetDependentTimes = Mathf.Clamp(m_AssetDependentTimes, 3, int.MaxValue);
        s_AssetDependentTimes = m_AssetDependentTimes;

        s_CommonAssetMax = Mathf.Clamp(s_CommonAssetMax, 1, int.MaxValue);
        s_CommonAssetMax = EditorGUILayout.IntField("公共资源合包上限", s_CommonAssetMax);

        EditorGUILayout.EndHorizontal();


        GUILayout.EndArea();


    }
    #endregion

    #region 帮助
    private Vector2 scroll;
    private void HelpEditorWindow()
    {
        string _help = "打包思路:1、如果不想被打包，标记为EditorOnly2、如果单独打包，标记为BuildSingle3、若想强制设置为公共资源，标记为IsCommonAsset4、若想合并打包，标记为BuildAsFolderName";
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(_help);
        EditorGUILayout.EndHorizontal();
    }
    #endregion


    #region Unity5_x
    private void clearAssetLabels()
    {
        Object[] list = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        int _length = list.Length;
        for (int i = 0; i < _length; ++i)
        {
            AssetDatabase.ClearLabels(list[i]);
        }

        if (EditorUtility.DisplayDialog("提示", "清理AssetLabels完成!", "确定", "取消"))
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }


    /// <summary>
    /// 为目标资源设置AssetBunldeName
    /// </summary>
    /// <param name="assetData"></param>
    private void SetAssetBunldeNamesForTargetAssets(EditorAssetData assetData, string assetbundleName = "")
    {

        EditorAssetBundleDataManager.Inst.Add(assetData, assetbundleName);

    }


    /// <summary>
    /// 创建目标资源AssetBundle包
    /// </summary>
    /// <param name="array"></param>
    private void createTargetAssetBundle(AssetBundleBuild[] array)
    {
        if (null == array || 0 == array.Length)
        {
            return;
        }
        string _targetPath = EditorPackDef.Assets_0 + "/" + EditorPackDef.ABPathRoot;

        BuildPipeline.BuildAssetBundles(_targetPath, array, m_FinalBuildAssetBundleOptions, s_targetPlantform);


    }



    /// <summary>
    /// 创建目标场景AssetBundle包
    /// </summary>
    /// <param name="scenePathList">全路径 Assets/Resources/Scenes/Main.unity</param>
    private void createTagetSceneAssetBundle(List<string> scenePathList)
    {
        string _folderPath = Application.dataPath + "/StreamingAssets/StreamingResources/scenes";
        AssetBundleTool.AddDir(_folderPath);

        for (int i = 0; i < scenePathList.Count; ++i)
        {
            string _path = scenePathList[i];
            if (AssetBundleTool.HasLabel(_path, EditorPackDef.Label_EditorOnly))
            {
                continue;
            }
            string _name = AssetBundleTool.GetResourceName(_path) + ".unity3d";
            BuildPipeline.BuildPlayer(new string[1] { _path }, _folderPath + "/" + _name, s_targetPlantform, BuildOptions.BuildAdditionalStreamedScenes);

        }

    }

    /// <summary>
    /// 打关系表
    /// </summary>
    private List<string> HandleManiFest()
    {
        string _assetBundleName = (EditorPackDef.ManiFest + EditorPackDef.AssetBundleSuffix).ToLower();
        List<string> _list = new List<string>();

        _list.Add(EditorPackDef.Assets_0 + "/" + EditorPackDef.AssetBundlePath);
        return _list;
    }

    #endregion

    #region 去冗余操作


    /// <summary>
    /// 替换内置资源
    /// </summary>
    private void ReplaceBuildInAsset()
    {

    }

    /// <summary>
    /// 还原内置资源
    /// </summary>
    private void RevertBuildInAsset()
    {

    }


    /// <summary>
    /// 剥离FBX上冗余资源
    /// </summary>
    /// <param name="list"></param>
    private void StripFBX(List<EditorAssetData> list)
    {
        if (null != list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                EditorAssetData _data = list[i];
                if (_data.BundleType == EditorBundleType.Model)
                {
                    GameObject _obj = AssetBundleTool.GetObjectByPath(_data.AssetPath) as GameObject;
                    if (null != _obj)
                    {
                        RemoveFBXAnimator(_obj);
                        RemoveFBXAnimation(_obj);
                        //RemoveFBXDefaultNull(_obj, null);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void RemoveFBXAnimator(GameObject rootObj)
    {
        if (null == rootObj)
        {
            return;
        }
        Animator[] _array = rootObj.GetComponentsInChildren<Animator>(true);
        if (null != _array && _array.Length > 0)
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                Animator _animator = _array[i];
                GameObject.DestroyImmediate(_animator, true);
                m_packLogSB.Append(string.Format("删除 {0} 的Animator成功！", rootObj.name) + "\r\n");

            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void RemoveFBXAnimation(GameObject rootObj)
    {
        if (null == rootObj)
        {
            return;
        }
        Animation[] _array = rootObj.GetComponentsInChildren<Animation>(true);
        if (null != _array && _array.Length > 0)
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                Animation _animation = _array[i];
                GameObject.DestroyImmediate(_animation, true);
                m_packLogSB.Append(string.Format("删除 {0} 的Animation成功！", rootObj.name) + "\r\n");
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void RemoveFBXDefaultNull(GameObject rootObj, Material mat)
    {
        if (null == rootObj)
        {
            return;
        }
        //去除默认材质球
        Renderer[] _array = rootObj.GetComponentsInChildren<Renderer>(true);
        if (null != _array && _array.Length > 0)
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                Renderer _renderer = _array[i];

                if (null != _renderer.sharedMaterial)
                {
                    if (_renderer.sharedMaterial.name == "Default-Material")
                    {
                        _renderer.sharedMaterial = null;

                        m_packLogSB.Append(string.Format("单独替换 {0} 的Material成功！", rootObj.name) + "\r\n");
                    }
                }

                if (null != _renderer.sharedMaterials && _renderer.sharedMaterials.Length > 0)
                {

                    for (int j = 0; j < _renderer.sharedMaterials.Length; ++j)
                    {
                        Material _mat = _renderer.sharedMaterials[j];
                        if (null != _mat && _mat.name == "Default-Material")
                        {
                            _renderer.material = mat;
                            _renderer.sharedMaterials[j] = mat;

                            m_packLogSB.Append(string.Format("替换 {0} 的Material成功！", rootObj.name) + "\r\n");

                        }
                    }
                }

            }
        }
    }

    /// <summary>
    /// 剥离粒子系统冗余的资源
    /// </summary>
    /// <param name="list"></param>
    private void StripParticleSystem(List<EditorAssetData> list)
    {
        if (null != list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                EditorAssetData _data = list[i];
                if (_data.BundleType == EditorBundleType.Prefab)
                {
                    GameObject _obj = AssetBundleTool.GetObjectByPath(_data.AssetPath) as GameObject;
                    if (null != _obj)
                    {
                        RemoveParticleSystemDefaultNull(_obj, null);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 剔除粒子系统的默认材质球
    /// </summary>
    /// <param name="rootObj"></param>
    /// <param name="mat"></param>
    private void RemoveParticleSystemDefaultNull(GameObject rootObj, Material mat)
    {
        if (null == rootObj)
        {
            return;
        }
        ParticleSystem[] _array = rootObj.GetComponentsInChildren<ParticleSystem>(true);
        if (null != _array && _array.Length > 0)
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                ParticleSystem _particleSystem = _array[i];
                Renderer _renderer = _particleSystem.GetComponent<Renderer>();
                if (null != _renderer)
                {
                    if (null != _renderer.sharedMaterial)
                    {
                        if (_renderer.sharedMaterial.name == "Default-Material")
                        {
                            _renderer.sharedMaterial = mat;

                            m_packLogSB.Append(string.Format("替换 {0} 的Material成功！", rootObj.name) + "\r\n");
                        }


                    }

                }

            }
        }


    }


    #endregion


    public void AlwaysIncludedShaders(List<string> shaderNameList)
    {

        if (null == shaderNameList)
        {
            return;
        }


        SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        SerializedProperty it = graphicsSettings.GetIterator();
        SerializedProperty dataPoint;
        while (it.NextVisible(true))
        {
            if (it.name == "m_AlwaysIncludedShaders")
            {
                it.ClearArray();

                for (int i = 0; i < shaderNameList.Count; i++)
                {
                    it.InsertArrayElementAtIndex(i);
                    dataPoint = it.GetArrayElementAtIndex(i);
                    dataPoint.objectReferenceValue = Shader.Find(shaderNameList[i]);
                }

                graphicsSettings.ApplyModifiedProperties();
            }

            //
            #region LightmapStripping
            if (it.name == "m_LightmapStripping")
            {
                it.intValue = 1;
            }
            if (it.name == "m_InstancingStripping")
            {
                it.intValue = 0;
            }
            if (it.name == "m_LightmapKeepPlain")
            {
                it.boolValue = true;
            }
            if (it.name == "m_LightmapKeepDirCombined")
            {
                it.boolValue = false;
            }
            if (it.name == "m_LightmapKeepDynamicPlain")
            {
                it.boolValue = false;
            }
            if (it.name == "m_LightmapKeepDynamicDirCombined")
            {
                it.boolValue = false;
            }
            if (it.name == "m_LightmapKeepShadowMask")
            {
                it.boolValue = false;
            }
            if (it.name == "m_LightmapKeepSubtractive")
            {
                it.boolValue = false;
            }

            #endregion

            #region FogStripping
            if (it.name == "m_FogStripping")
            {
                it.intValue = 1;
            }
            if (it.name == "m_FogKeepLinear")
            {
                it.boolValue = true;
            }
            if (it.name == "m_FogKeepExp")
            {
                it.boolValue = false;
            }
            if (it.name == "m_FogKeepExp2")
            {
                it.boolValue = true;
            }
            #endregion

        }

        graphicsSettings.ApplyModifiedProperties();
    }

    /// <summary>
    /// 初始化基本环境
    /// </summary>
    private void initBaseEviroment()
    {
        AssetBundleTool.Init();

        ClearTmp();
        ClearCache();

        EditorAssetDataManager.Inst.Clear();
        EditorAssetBundleDataManager.Inst.Clear();

        m_SceneList.Clear();

        m_UselessEditorAssetDataList.Clear();
        m_packLogSB = new StringBuilder();

        AssetBundleTool.AddDir(EditorPackDef.AssetTmpPathRoot);

        AssetBundleTool.AddDir(Application.dataPath + "/StreamingAssets/GameRes");
        AssetBundleTool.AddDir(Application.dataPath + "/StreamingAssets/StreamingResources");

        string[] myShaders = new string[]{
         "Legacy Shaders/Diffuse",
         "UI/Default",
         "Sprites/Default",
         "Mobile/Particles/Alpha Blended",
         "Hidden/VideoDecode",
         "Standard",
         "UI/Default Font",
         "Custom/NoLight/Unlit - Object##",
         "Custom/Light/Diffuse - Object##",
         "Custom/Light/Diffuse - Human##",
         "Custom/Light/Diffuse - RimLight##",
         "Hidden/Compositing"
     };

        AlwaysIncludedShaders(myShaders.ToList());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }


    /// <summary>
    /// 初始化完整打包时的环境
    /// </summary>
    private void initPackAllEviroment()
    {
        DateTime _time = DateTime.Now;
        initBaseEviroment();

        AssetDatabase.Refresh();

        m_packLogSB.Append("初始化环境耗时：" + (DateTime.Now - _time).TotalSeconds + "\r\n");
    }
    private void SetLoopAssetNest(List<EditorAssetData> list)
    {

        for (int i = 0; i < list.Count; i++)
        {
            var _data = list[i];
            if (!_data.AssetPath.Contains("Effect/Role_effect"))
                continue;
            for (int j = 0; j < _data.UpDependenceList.Count; j++)
            {
                var upData = _data.UpDependenceList[j];
                if (_data.FinalAssetBundleName != upData.FinalAssetBundleName && _data.DownDependenceDataList.Exists(v => v.FinalAssetBundleName == upData.FinalAssetBundleName) && upData.DownDependenceDataList.Exists(v => v.FinalAssetBundleName == _data.FinalAssetBundleName))//跳出相互引用循环
                {
                    //var Label = EditorPackDef.Label_BuildSingle;//强制设置为 BuildSingle
                    Object _target = AssetBundleTool.GetObjectByPath(_data.AssetPath);
                    AssetBundleTool.ClearLabel(_target);
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_LoopSingle);
                    _data.FinalAssetBundleName = "NULL";
                    Debug.LogError("存在循环引用：" + _data.FinalAssetBundleName);
                    break;
                }
            }
        }
    }
    /// <summary>
    /// 公共资源合包处理
    /// </summary>
    /// <param name="list"></param>
    private void combineCommonAB(List<EditorAssetData> list)
    {
        if (null == list || 0 == list.Count)
        {
            return;
        }
        List<EditorAssetData> _allCommonDataList = new List<EditorAssetData>();
        for (int i = 0; i < list.Count; ++i)
        {

            if (list[i].IsCommonAsset)
            {
                _allCommonDataList.Add(list[i]);
            }
        }


        for (int i = 0; i < _allCommonDataList.Count; ++i)
        {
            EditorAssetData _data = _allCommonDataList[i];

            if (0 == _data.TopUpDependenceList.Count || _data.BundleType == EditorBundleType.Shader || _data.IsBuildSingle)
            {
                continue;
            }

            if (_data.BuildAsFolderName)
            {
                continue;
            }

            bool _flag = true;
            string _finalAssetBundleName = "";
            for (int j = 0; j < _data.TopUpDependenceList.Count; ++j)
            {

                if (_data.TopUpDependenceList[j].Useless || _data.TopUpDependenceList[j].IsEditorOnly)
                {
                    continue;
                }



                if (string.IsNullOrEmpty(_finalAssetBundleName))
                {
                    _finalAssetBundleName = _data.TopUpDependenceList[j].FinalAssetBundleName;
                }

                if (_finalAssetBundleName != _data.TopUpDependenceList[j].FinalAssetBundleName)
                {
                    _flag = false;
                    break;
                }

            }
            if (_flag)
            {
                if (!string.IsNullOrEmpty(_finalAssetBundleName))
                {
                    List<EditorAssetData> _list = _data.GetDownDependenceListUnIndepentAsset();
                    for (int j = 0; j < _list.Count; ++j)
                    {
                        _list[j].FinalAssetBundleName = _finalAssetBundleName;
                    }
                    _data.FinalAssetBundleName = _finalAssetBundleName;
                    _data.IsCommonAsset = false;
                }
            }

        }





    }

    /// <summary>
    /// 打包目标资源 
    /// </summary>
    /// <param name="targetAssetDataDic"></param>
    private void PackTargetAssets(Dictionary<string, EditorAssetData> targetAssetDataDic)
    {
        if (null == targetAssetDataDic || 0 == targetAssetDataDic.Count)
        {
            return;
        }

        Dictionary<string, EditorAssetBundleData> _dic = new Dictionary<string, global::EditorAssetBundleData>();
        List<EditorAssetData> _list = targetAssetDataDic.Values.ToList();
        Dictionary<string, List<string>> _abDic = new Dictionary<string, List<string>>();
        for (int i = 0; i < _list.Count; ++i)
        {
            EditorAssetData _data = _list[i];
            List<string> _value;
            _abDic.TryGetValue(_data.FinalAssetBundleName.ToLower(), out _value);
            if (_value == null)
            {
                _value = new List<string>();
                if (_value.Exists(v => v == _data.AssetPath) == false)
                    _value.Add(_data.AssetPath);
                _abDic[_data.FinalAssetBundleName.ToLower()] = _value;
            }
            else
            {
                if (_value.Exists(v => v == _data.AssetPath) == false)
                    _value.Add(_data.AssetPath);
            }
        }
        EditorUtility.ClearProgressBar();
        List<AssetBundleBuild> _abList = new List<AssetBundleBuild>();
        foreach (var item in _abDic)
        {
            AssetBundleBuild _temp = new AssetBundleBuild();
            _temp.assetBundleName = item.Key;
            _temp.assetNames = item.Value.ToArray();
            _abList.Add(_temp);
        }
        //单打manifest************************************
        List<string> _pathList = HandleManiFest();
        AssetBundleBuild _ab = new AssetBundleBuild();
        _ab.assetBundleName = EditorPackDef.ManiFest + EditorPackDef.AssetBundleSuffix;
        _ab.assetNames = _pathList.ToArray();
        _abList.Add(_ab);
        //单打manifest************************************
        //增加shader 变体************************************shader是在收集关系标里，但是没有变体信息，所以在此添加
        var _tempShader = _abList.Find(v => v.assetBundleName == (EditorPackDef.AllShaders + EditorPackDef.AssetBundleSuffix).ToLower());
        _abList.Remove(_tempShader);
        var _aar = _tempShader.assetNames.ToList();
        _aar.Add("Assets/Resources/ShaderCollector/ShaderVariant.shadervariants");
        _tempShader.assetNames = _aar.ToArray();
        _abList.Add(_tempShader);
        //增加shader 变体************************************shader是在收集关系标里，但是没有变体信息，所以在此添加
        createTargetAssetBundle(_abList.ToArray());

        DateTime _sceneDateTime = DateTime.Now;
        if (m_SceneList.Count > 0)
        {
            List<string> _scenePathList = new List<string>();
            for (int i = 0; i < m_SceneList.Count; ++i)
            {
                _scenePathList.Add(m_SceneList[i].AssetPath);
            }
            createTagetSceneAssetBundle(_scenePathList);
        }


        double _sceneCostTime = (DateTime.Now - _sceneDateTime).TotalSeconds;
        m_packLogSB.Append("打包场景完毕：" + DateTime.Now.GetDateTimeFormats('s')[0].ToString() + "\n" + "耗时：" + _sceneCostTime + "秒"
        );
        m_packLogSB.Append("\r\n");
        m_packLogSB.Append("\r\n");


        for (int i = 0; i < m_UselessEditorAssetDataList.Count; ++i)
        {
            m_packLogSB.Append("未使用的资源：" + m_UselessEditorAssetDataList[i].AssetPath);
            m_packLogSB.Append("\r\n");
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        m_packLogSB.Append("\r\n");
        m_packLogSB.Append("资源总数：" + targetAssetDataDic.Count);
        m_packLogSB.Append("\r\n");

        double _costTime = (DateTime.Now - s_startDateTime).TotalSeconds;
        m_packLogSB.Append("打包完毕：" + DateTime.Now.GetDateTimeFormats('s')[0].ToString() + "\n" + "耗时：" + _costTime + "秒");
        m_packLogSB.Append("\r\n");


        m_packLogSB.Append("流程完毕：" + DateTime.Now.GetDateTimeFormats('s')[0].ToString() + "\n" + "总耗时：" + _costTime + "秒");
        //生成PackLogText文本
        BuildPackLogText(m_packLogSB.ToString());


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();


    }

    /// <summary>
    /// 整理资源之间的交叉依赖关系
    /// </summary>
    /// <param name="allAssetPathList"></param>
    /// <returns></returns>
    private Dictionary<string, EditorAssetData> collateABAssetInfoRelationship(Dictionary<string, EditorAssetData> _dic)
    {
        DateTime _time = DateTime.Now;

        //整理依赖关系
        _dic = AssetBundleTool.CollateABAssetInfoRelationship(_dic);
        List<EditorAssetData> _list = _dic.Values.ToList();
        string _name = string.Empty;
        for (int i = 0; i < _list.Count; ++i)
        {
            EditorAssetData _data = _list[i];
            _name = _data.FinalAssetBundleName;
        }
        combineCommonAB(_list);

        //决定资源属于哪个AssetBundle
        EditorAssetDataManager.Inst.BundledAssetDataList = NameSelectedAssets(_dic);


        for (int i = 0; i < _list.Count; ++i)
        {
            EditorAssetData _data = _list[i];
            if (_data.Useless)//剔除无用资源
            {
                m_UselessEditorAssetDataList.Add(_data);
                _dic.Remove(_data.AssetPath);
                EditorAssetDataManager.Inst.BundledAssetDataList.Remove(_data);

            }
            if (_data.FinalAssetBundleName.Equals(string.Empty))//剔除无包名资源，例如：只有一个父级资源，且父级资源未被任何其他资源引用
            {
                _dic.Remove(_data.AssetPath);
                EditorAssetDataManager.Inst.BundledAssetDataList.Remove(_data);
            }
        }




        m_packLogSB.Append("整理依赖关系耗时：" + (DateTime.Now - _time).TotalSeconds + "\r\n");
        EditorAssetDataManager.Inst.AllAssetDataDic = _dic;
        return _dic;
    }

    /// <summary>
    /// 为被选中的相关资源命名AssetBundleName
    /// </summary>
    /// <param name="assetPathList"></param>
    /// <returns></returns>
    private List<EditorAssetData> NameSelectedAssets(Dictionary<string, EditorAssetData> targetAssetDataDic)
    {
        DateTime _time = DateTime.Now;
        List<EditorAssetData> _assetDataList = targetAssetDataDic.Values.ToList();


        int _count = _assetDataList.Count;
        for (int i = 0; i < _count; ++i)
        {
            EditorAssetData _info = _assetDataList[i];


            if (_info.BundleType == EditorBundleType.Scene)
            {
                m_SceneList.Add(_info);
                targetAssetDataDic.Remove(_info.AssetPath);
                continue;
            }


            if (_info.IsOnlyUsedByScene)
            {
                targetAssetDataDic.Remove(_info.AssetPath);
                continue;
            }

            SetAssetBunldeNamesForTargetAssets(_info);

            if (_info.Useless)
            {
                AssetBundleTool.ClearAssetBundleName(_info.AssetPath);
            }

        }


        m_packLogSB.Append("为资源命名耗时：" + (DateTime.Now - _time).TotalSeconds + "\r\n");
        return targetAssetDataDic.Values.ToList();
    }


    /// <summary>
    /// 生成PackLogText文本
    /// </summary>
    /// <param name="content"></param>
    private void BuildPackLogText(string content)
    {
        AssetBundleTool.BuildFileTxt(Application.dataPath + "/" + EditorPackDef.PackLogPath + "_" + AssetBundleTool.ToTimeFormat(DateTime.Now)
    + ".txt", content);
    }
    /// <summary>
    /// 生成ManiFestText文本
    /// </summary>
    /// <param name="content"></param>
    private void BuildManiFestText(string path, string content)
    {
        AssetBundleTool.BuildFileTxt(path, content);
    }
    /// <summary>
    /// 生成AssetBundleText文本
    /// </summary>
    /// <param name="content"></param>
    private void BuildAssetBundleDataText(string path, string content)
    {
        AssetBundleTool.BuildFileTxt(path, content);
    }

    private void ClearTmp()
    {

        AssetBundleTool.DelectDir(EditorPackDef.AssetTmpPathRoot);
        AssetBundleTool.DeletFolder(EditorPackDef.AssetTmpPathRoot);

        AssetDatabase.Refresh();
    }

    private void ClearCache()
    {
        AssetBundleTool.ClearCache();
    }


    /// <summary>
    /// 检查关系表是否被其他进程打开
    /// </summary>
    /// <returns></returns>
    private bool check()
    {
        //判定ManiFestText文件是否被打开
        if (File.Exists(Application.dataPath + "/" + EditorPackDef.ManiFestTextPath))
        {
            if (AssetBundleTool.IsFileInUse(Application.dataPath + "/" + EditorPackDef.ManiFestTextPath))
            {
                EditorUtility.DisplayDialog("提示", "ManiFestText文件已被其他进程打开！", "确定", "取消");

                return true;

            }
        }
        if (File.Exists(Application.dataPath + "/" + EditorPackDef.AssetBundleTextPath))
        {
            //判定AssetBundleText文件是否被打开
            if (AssetBundleTool.IsFileInUse(Application.dataPath + "/" + EditorPackDef.AssetBundleTextPath))
            {
                EditorUtility.DisplayDialog("提示", "AssetBundleText文件已被其他进程打开！", "确定", "取消");

                return true;

            }
        }

        return false;
    }

    private void OnDestroy()
    {

        PlayerPrefs.SetInt("BuildAssetBundleOptions", (int)s_BuildAssetBundleOptions);

    }

    private void EditorEncrypty()
    {
        if (Selection.objects.Length == 0)
        {
            return;
        }
        string _path = Application.dataPath.Replace("Assets", "") + AssetBundleTool.GetAssetPath(Selection.objects[0]);
        if (!_path.Contains("StreamingAssets/StreamingResources"))
        {
            return;
        }
        //if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            //AssetBundleTool.EncryptyGameFiles(_path);

        AssetDatabase.Refresh();
    }

    private void EditorDecrypty()
    {
        if (Selection.objects.Length == 0)
        {
            return;
        }
        string _path = Application.dataPath.Replace("Assets", "") + AssetBundleTool.GetAssetPath(Selection.objects[0]);
        if (!_path.Contains("StreamingAssets/StreamingResources"))
        {
            return;
        }
        //AssetBundleTool.DecryptyGameFiles(_path);

        AssetDatabase.Refresh();

    }

    private void EditorDeletUnusedFile()
    {
        //AssetBundleTool.DeletUnusedAssetBundle(Application.dataPath + "/StreamingAssets/StreamingResources");
        if (EditorUtility.DisplayDialog("提示", "操作完成", "确定", "取消"))
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void EditorRefresh()
    {
        EditorUtility.ClearProgressBar();
        ClearCache();
        AssetDatabase.Refresh();
    }

    private void EditorBuildHotfix()
    {
        //PackAllWithoutTip();
        string _name = "GameRes";
        if (s_targetPlantform == BuildTarget.iOS)
        {
            _name = "iOS_GameRes";
        }
        else if (s_targetPlantform == BuildTarget.Android)
        {
            _name = "Android_GameRes";
        }
        else if (s_targetPlantform == BuildTarget.StandaloneWindows ||
            s_targetPlantform == BuildTarget.StandaloneWindows64)
        {
            _name = "PC_GameRes";
        }
        //string AssetsPath = EditorUtility.SaveFolderPanel("请选择原始包的StreamingResources文件夹", "StreamingResources", "GameRes");
        AssetBundleTool.AutoBuildHotfix(Application.dataPath + "/StreamingAssets/StreamingResources",
            Application.dataPath + "/../../hotfix/OrigionalAB/StreamingAssets/StreamingResources",
            Application.dataPath + "/../../hotfix/" + _name + "/StreamingResources");

        System.Diagnostics.Process.Start(Application.dataPath + "/../../hotfix");


    }
    private static void CopyResources()
    {
        string sdkSrc = Application.dataPath + "/Resources/Plugins";
        string sdkDst = Application.dataPath.Replace("Unity", "Unity_Release") + "/Resources/Plugins";

        UIO.MoveToDirectory(sdkSrc, sdkDst, s => s.ToLower().EndsWith(".dll")|| s.ToLower().EndsWith(".mdb") || s.ToLower().EndsWith(".pdb"));
        sdkSrc = Application.dataPath+ "/StreamingAssets/StreamingResources";
        sdkDst = Application.dataPath.Replace("Unity", "Unity_Release") + "/StreamingAssets/StreamingResources";
        UIO.MoveToDirectory(sdkSrc, sdkDst, s => s.ToLower().EndsWith(".assetbundle"));
        if (EditorUtility.DisplayDialog("提示", "复制完成！", "确定", "取消"))
        {
           
        }
    }
    private static void CopyHotFixOrginalResources()
    {
        string _path = Application.dataPath;
        string sdkSrc = _path + "/StreamingAssets/StreamingResources";
        string sdkDst = _path.Replace("Unity/Assets", "") + "/hotfix/OrigionalAB/StreamingAssets/StreamingResources";
        UIO.MoveToDirectory(sdkSrc, sdkDst, s => s.ToLower().EndsWith(".assetbundle"));
        if (EditorUtility.DisplayDialog("提示", "复制完成！", "确定", "取消"))
        {

        }
    }
}
