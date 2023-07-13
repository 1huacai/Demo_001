using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEditor;
using Object = UnityEngine.Object;

public delegate void CallBackHandle();
public delegate void CallBack();
public delegate void CallBack<T>(T arg0);
public delegate void CallBack<T, T1>(T arg0, T1 arg1);
public delegate void CallBack<T, T1, T2>(T arg0, T1 arg1, T2 args2);
public delegate void CallBack<T, T1, T2, T3>(T arg0, T1 arg1, T2 args2, T3 args3);



public delegate bool CallbackBool();
public delegate bool CallbackBool<T>(T arg0);
public delegate bool CallbackBool<T, T1>(T arg0, T1 arg1);

public delegate object CallbackObj();
public delegate object CallbackObj<T>(T arg0);
public delegate object CallbackObj<T, T1>(T arg0, T1 arg1);

public delegate void CallBackFunc();
public delegate void CallBackFunc<T>(T arg0);


///file byte简单测试
/// <summary>
/// 读包     19       6.5   压缩快3倍多
///	load    11.3     36    不压缩快3倍多
///	总时间   30       42    不压缩快1倍多
/// </summary>

/// 最佳方案：不压缩，打Zip，第一次安装拷贝后直读


//资源assetbundle打包功能
//public class BuildAssetBundle
//{

//    public static string assetbundleFilePath = Application.dataPath + "/StreamingAssets";
//    public static string assetbundleFileSuffix = ".bytes";
//    public static string assetbundleFileBackupFName = "BuildAssetsBackup";
//    public static string assetbundleFileBackup = Application.dataPath.Replace("Assets", "") + assetbundleFileBackupFName;
//    //永远忽略打包的部分,支持文件夹
//    public static string EDITOR_ONLY = "l:EditorOnly";
//    //固定打包的部分,支持文件夹
//    public static string BUILD_SINGLE = "l:BuildSingle";
//    //只支持预制体,不进行深度拆分,如果预制体下的资源被其他预制体引用,那么会有多份该资源
//    public static string BUILD_EACH_PREFAB = "l:BuildEachPrefab";


//    public static List<string> constEditorOnlyPathList;
//    public static List<string> constBuildePathList;
//    private static List<string> _BuildEachPrefabPathList;

//    public static List<string> BuildEachPrefabPathList
//    {
//        get
//        {
//            if (_BuildEachPrefabPathList == null)
//                GetAllBuildEachPrefab();

//            return _BuildEachPrefabPathList;
//        }
//    }


//    //是否打包Script
//    public static bool isBuildScripts = true;
//    //是否打包DLL
//    public static bool isBuildDlls = true;
//    //是否生成MainFest
//    public static bool isBuildMainFest = true;
//    //是否创建list.settings
//    public static bool isCreateBuildAssetInfo = false;
//    //是否创建list.settings
//    public static bool isBuildAllMaterial = false;
//    //打包前删除老数据
//    public static bool DeleteOldData = false;

//    //assetbundle资源
//    static string assetFilePath = "/StreamingAssets";

//    //打包环境设置
//    private static BuildAssetBundleOptions m_options = BuildAssetBundleOptions.DeterministicAssetBundle;

//    private static BuildAssetBundleOptions options
//    {
//        get
//        {
//            if (BuildPlatform == BuildTarget.iOS)
//            {
//                return m_options | BuildAssetBundleOptions.ChunkBasedCompression;
//            }
//            else
//            {
//                return m_options;
//            }
//        }
//    }



//    public static BuildTarget BuildPlatform = BuildTarget.iOS;



//    //保存所有Resource信息
//    private static List<string> preResources = new List<string>();
//    private static List<string> allResources = new List<string>();
//    private static List<string> allScripts = new List<string>();
//    private static List<string> allDlls = new List<string>();
//    private static List<string> bulidAssetInfoList = new List<string>();
//    private static List<Object> allShaderList = new List<Object>();
//    private static List<Object> allMaterialList = new List<Object>();
//    private static List<Object> allFBXList = new List<Object>();

//    //所有资源信息
//    public static Dictionary<string, AssetInfo> allAssetInfos = new Dictionary<string, AssetInfo>();
//    private static List<AssetInfo> SingleAssetInfos = new List<AssetInfo>();
//    /// <summary>
//    /// 忽略资源路径，这种资源没有对AssetBundleName命名，被重复打包了 
//    /// </summary>
//    private static List<string> ignorAssetList = new List<string>();
//    //按深度等级保存的所有资源信息，最后打包的时候用
//    private static Dictionary<int, Dictionary<string, AssetInfo>> allLevelAssets = new Dictionary<int, Dictionary<string, AssetInfo>>();


//    //unity中的资源对应记录
//    private static GameObject _mainfest;




//    [MenuItem("Youkia/AssetBundle打包管理/工具/单独生成MD5s比对文件")]
//    public static void CreateMD5sFileNow()
//    {

//        AssetDatabase.Refresh();
//        AllFileMD5 md5 = BuildAssetBundle.CreateMD5Dic(true);
//        AssetDatabase.Refresh();
//        BuildAssetBundle.MD5Save(md5);
//        EditorUtility.DisplayDialog("提示", "MD5s比对文件生成成功", "OK");
//    }

//    //新打包工具
//    [MenuItem("Youkia/AssetBundle打包管理/AssetBundle打包工具")]
//    public static void openAssetBundlePackageTool()
//    {

//        List<BuildTarget> buildTargetList = new List<BuildTarget>();
//        buildTargetList.Add(BuildTarget.Android);
//        buildTargetList.Add(BuildTarget.iOS);

//        AssetBundlePackageToolWindow.init(buildTargetList);
//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/GUID转换")]
//    public static void GUIDCover()
//    {
//        GUIDCoverWindow.init();
//    }


//    [MenuItem("Youkia/AssetBundle打包管理/工具/清理缓存")]
//    public static void CleanCache()
//    {
//        Caching.CleanCache();
//        Clear();
//    }

//    public static void checkDep()
//    {
//        UnityEngine.Object[] List = Selection.objects;
//        string[] path = new string[]{
//            AssetDatabase.GetAssetPath (List[0])
//        };

//        string[] tmp = AssetDatabase.GetDependencies(path);

//    }



//    public static void RemoveAssetInfo(string path)
//    {
//        if (null != allAssetInfos && allAssetInfos.Count > 0)
//        {
//            if (!ignorAssetList.Contains(path))
//            {
//                ignorAssetList.Add(path);
//            }
//            allAssetInfos.Remove(path);

//        }
//    }

//    //  清理所有打包信息
//    public static void Clear()
//    {
//        allScripts.Clear();
//        preResources.Clear();
//        allResources.Clear();
//        allAssetInfos.Clear();
//        allLevelAssets.Clear();
//        allLoadObjs.Clear();

//        allShaderList.Clear();
//        allMaterialList.Clear();
//        allFBXList.Clear();

//        ignorAssetList.Clear();
//        if (_BuildEachPrefabPathList != null)
//            _BuildEachPrefabPathList.Clear();
//        if (_buildResources != null)
//            _buildResources.Clear();


//        bulidAssetInfoList.Clear();
//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/生成resLink")]
//    public static void CreateSelectResLink()
//    {
//        Object obj = Selection.activeObject;

//        CreateResLink(obj);
//    }
//    [MenuItem("Youkia/AssetBundle打包管理/工具/还原resLink")]
//    public static void RestoreSelectResLink()
//    {
//        Object obj = Selection.activeObject;

//        (obj as ResLinker).Setup(null);
//    }

//    public static ResLinker CreateResLink(Object obj)
//    {
//        return CreateResLink(obj, obj.name + "_ResLinker");
//    }

//    public static ResLinker CreateResLink(Object obj, string name)
//    {
//        //注意顺序,避免all里有ResLinker
//        ResLinker linker = ScriptableObject.CreateInstance<ResLinker>();

//        if (obj is GameObject)
//        {

//            Component[] all = (obj as GameObject).GetComponentsInChildren<Component>(true);

//            List<Component> passList = new List<Component>();
//            for (int i = 0; i < all.Length; i++)
//            {
//                if (all[i] is Transform || all[i] is CanvasRenderer || all[i] == null)
//                {
//                    continue;
//                }
//                else
//                {
//                    passList.Add(all[i]);
//                }
//            }
//            all = passList.ToArray();
//            List<ComponentResPathRecord> infos = new List<ComponentResPathRecord>();

//            for (int i = 0; i < all.Length; i++)
//            {
//                ComponentResPathRecord resPathRecord = ResLinkerTool.CreateComponentResPathRecord(all[i]);

//                if (resPathRecord != null && resPathRecord.Name.Length > 0)
//                    infos.Add(resPathRecord);

//            }
//            linker.infos = infos.ToArray();
//        }
//        else if (obj is Material)
//        {

//            ComponentResPathRecord resPathRecord = ResLinkerTool.CreateMaterialResPathRecord(obj as Material);
//            linker.infos = new ComponentResPathRecord[] { resPathRecord };
//        }
//        else
//        {
//            return null;
//        }
//        AssetDatabase.CreateAsset(linker, "Assets/" + name + ".asset");
//        return AssetDatabase.LoadAssetAtPath("Assets/" + name + ".asset", typeof(ResLinker)) as ResLinker;


//    }

//    public static void BuildCheck()
//    {


//        //刷新数据
//        //  Caching.CleanCache();
//        AssetDatabase.Refresh();

//        //先获取所有脚本
//        GetAllScripts(true);

//        //先获取dlls IOS无法热更Dll
//        if (BuildPlatform != BuildTarget.iOS)
//        {
//            if (!GetAllDlls(false))
//            {
//                dllErrorAlert();
//                return;
//            }
//        }

//        //确定需要打包的母资源
//        allResources = GetBuildResources(true);
//        System.GC.Collect();

//        //int index = Undo.GetCurrentGroup();
//        backupResources();
//        System.GC.Collect();
//        //资源转换
//        ScriptsCover();
//        System.GC.Collect();
//        //获取所有关联资源
//        GetAllAssets(true);
//        System.GC.Collect();
//        //还原
//        //Undo.RevertAllDownToGroup(index);
//        restoreResources();
//        System.GC.Collect();
//        Clear();
//    }

//    public static string AssetPathToGUID(string path)
//    {
//        return AssetDatabase.AssetPathToGUID(path);
//    }

//    public static void DeleteAssetFiles(HashSet<string> delPathSet)
//    {

//        string[] dList = delPathSet.ToArray();

//        if (dList != null && dList.Length > 0)
//        {

//            string delPath, fullDelPath;

//            int i, len = delPathSet.Count;
//            for (i = 0; i < len; i++)
//            {
//                fullDelPath = Application.dataPath + "/" + dList[i];
//                delPath = "Assets/" + dList[i];

//                if (File.Exists(fullDelPath))
//                {
//                    /*
//                    if (File.Exists(delPath))
//                    {
//                        if (File.Exists(delPath + ".meta"))
//                        {
//                            File.Delete(delPath + ".meta");
//                        }
//                        File.Delete(delPath);
//                    }
//                    else
//                    {
//                        Debug.LogWarning("BuildAssetBunlde.DeleteAssetFiles Warning :: 找不到要删除的对象 : " + delPath);
//                    }*/
//                    if (AssetDatabase.DeleteAsset(delPath))
//                    {
//                        Debug.Log("BuildAssetBundle.DeleteAssetFiles :: 成功删除AB包 : path = " + fullDelPath);
//                    }
//                    else
//                    {
//                        Debug.LogError("BuildAssetBundle.DeleteAssetFiles Error :: 删除的对象失败 ： path = " + fullDelPath);
//                    }

//                }
//                else
//                {
//                    Debug.LogWarning("BuildAssetBundle.DeleteAssetFiles Warning :: 找不到要删除的对象 ： path = " + fullDelPath);
//                }
//            }

//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();

//        }
//    }
//    private static bool isExcludeINDelList(string guid)
//    {

//        switch (guid)
//        {
//            case "AllScripts":
//                return true;
//            default:
//                return false;
//        }
//    }

//    public static Dictionary<string, string> GetResourcesByMD5Comparisons(ref AllFileMD5 newMd5s)
//    {

//        AllFileMD5 oldMd5s = Selection.activeObject as AllFileMD5;
//        if (oldMd5s == null)
//        {
//            EditorUtility.DisplayDialog("提示", "请在Project库里选择一个MD5s_XXX.asset文件作为比对数据源", "确定");
//            return null;
//        }
//        else
//        {
//            oldMd5s.LoadData();
//        }


//        newMd5s = CreateMD5Dic(true);
//        Dictionary<string, string> buildSet = new Dictionary<string, string>();

//        //创建ABmd5 进行对比, 防止以存在的MD5包文件被手动修改。
//        int i, len;
//        Dictionary<string, string> admd5_n = CreateABMD5Dic();
//        Dictionary<string, AllFileMD5.EMd5State> admd5_cp = CompareABMD5(oldMd5s.ABMD5Dic, admd5_n);
//        if (admd5_cp != null && admd5_cp.Count > 0)
//        {

//            string[] keys = admd5_cp.Keys.ToArray();
//            len = keys.Length;
//            //            foreach (KeyValuePair<string, AllFileMD5.EMd5State> each in admd5_cp)
//            for (i = 0; i < len; i++)
//            {

//                string abPath = keys[i].Replace("\\", "/");

//                //这里排除Dlls，打Dll是独立控制的
//                if (abPath.Contains("/Dlls/")) continue;

//                string guid = keys[i].Substring((abPath.LastIndexOf('/') + 1)).Replace(".bytes", "");
//                string p = AssetDatabase.GUIDToAssetPath(guid);

//                if (string.IsNullOrEmpty(p))
//                {
//                    //排除不能删除的AB包
//                    if (isExcludeINDelList(guid))
//                    {
//                        continue;
//                    }

//                    Debug.Log("ABMD5对比：： GUID：[" + guid + "] path:[" + abPath + "] 无法找到对应资源, 该AB将被移除。");
//                    if (!buildSet.ContainsKey(abPath))
//                    {
//                        buildSet.Add(abPath, AllFileMD5.EMd5State.delete.ToString());
//                    }
//                    continue;
//                }
//                if (!buildSet.ContainsKey(p))
//                    buildSet.Add(p, admd5_cp[keys[i]].ToString());
//            }
//        }

//        //校验新老md5都有的文件是否存在ab包,不存在则必打
//        string[] dicKeys = newMd5s.MD5Dic.Keys.ToArray();
//        len = dicKeys.Length;

//        for (i = 0; i < len; i++)
//        {
//            if (oldMd5s.MD5Dic.ContainsKey(dicKeys[i]))
//            {
//                string guid = AssetDatabase.AssetPathToGUID(dicKeys[i]);

//                string dir = BuildCommon.getFolder(dicKeys[i]);
//                dir = dir.Replace("Assets/Resources/", "Assets/StreamingAssets/StreamingResources/");
//                //材质球统一打一个包了,所以不需要再检测材质球ab包是否存在
//                if (AssetDatabase.LoadAssetAtPath(dir + "/" + guid + ".bytes", typeof(TextAsset)) == null && BuildCommon.getFileSuffix(dicKeys[i]).ToLower() != "mat")
//                {
//                    if (buildSet.ContainsKey(dicKeys[i]))
//                    {
//                        buildSet[dicKeys[i]] = AllFileMD5.EMd5State.lostUpdate.ToString();
//                    }
//                    else
//                    {
//                        buildSet.Add(dicKeys[i], AllFileMD5.EMd5State.lostUpdate.ToString());
//                    }
//                }
//            }

//        }

//        Dictionary<string, AllFileMD5.EMd5State> dic = CompareMD5(oldMd5s, newMd5s);
//        if (dic != null && dic.Count > 0)
//        {
//            string[] dicCpKeys = dic.Keys.ToArray();

//            len = dicCpKeys.Length;
//            //            foreach (var each in dic)
//            for (i = 0; i < len; i++)
//            {
//                if (!dicCpKeys[i].Contains("Resources/MainFest.prefab"))
//                {
//                    if (!buildSet.ContainsKey(dicCpKeys[i]))
//                        buildSet.Add(dicCpKeys[i], dic[dicCpKeys[i]].ToString());

//                }

//            }

//        }

//        //获得依赖子物体 4.x必须先打下级依赖
//        HashSet<string> allChilds = GetSelectedResources(buildSet.Keys.ToArray());

//        if (allChilds != null && allChilds.Count > 0)
//        {
//            string[] allChildsList = allChilds.ToArray();
//            len = allChildsList.Length;
//            //            foreach (var child in allChilds)
//            for (i = 0; i < len; i++)
//            {
//                string child = allChildsList[i];
//                if (!buildSet.Keys.Contains(child))
//                    buildSet.Add(child, AllFileMD5.EMd5State.linkChild.ToString());
//            }
//        }

//        return buildSet;

//    }

//    public static void BuildResourceFromSimpleRule(HashSet<string> selection)
//    {
//        //配置转UTF-8
//        //ConfigEncoding.Conver();
//        AssetDatabase.SaveAssets();
//        BuildCommon.ClearAssetBundleBuildList();


//        List<string> buildingList = new List<string>();

//        string[] selectionPath = selection.ToArray();
//        int i, len = selectionPath.Length;
//        //        foreach (string s in selection)
//        for (i = 0; i < len; i++)
//        {
//            buildingList.Add(selectionPath[i]);
//        }


//        len = buildingList.Count;
//        for (i = 0; i < len; i++)
//        {

//            AssetInfo asInfo = BuildCommon.CreateInfo(buildingList[i]);
//            _tempObj = AssetDatabase.LoadAssetAtPath(asInfo.longPath, typeof(Object));
//            //生成打包保存路径
//            string savePath = asInfo.SavePath;
//            savePath = savePath + assetbundleFileSuffix;

//            BuildCommon.CheckFolder(BuildCommon.getFolder(savePath));
//            BuildCommon.CollecateAssetBundle(_tempObj, null, asInfo.Directories, asInfo.Guid + assetbundleFileSuffix);

//        }
//        BuildCommon.FinalBuildAssetBundles("Assets/StreamingAssets/StreamingResources/", options, BuildPlatform);
//    }

//    public static int state = -1;
//    public static string workTips = "";
//    public static AssetBundlePackageToolWindow ToolWindow;

//    static void ShowTips(string tips)
//    {

//        if (ToolWindow)
//            ToolWindow.ShowBuildTips(tips);
//    }

//    public static void BuildResourceFromUnityRule(HashSet<string> selection, AssetBundlePackageToolWindow window = null, bool buildManiFestOnly = false)
//    {
//        bool _showTime = true;

//        DateTime _start = DateTime.Now;
//        DateTime _time = DateTime.Now;

//        //ClearAssetBundleName.ClearWithoutTip();

//        ToolWindow = window;
//        state = 1;

//        AssetDatabase.SaveAssets();
//        GetAllBuildEachPrefab();

//        if (_showTime)
//            Debug.LogError("获取所有BuildEachPrefab耗时：" + (DateTime.Now - _time).TotalSeconds + "秒");
//        _time = DateTime.Now;

//        //先获取所有脚本
//        if (isBuildScripts)
//        {
//            GetAllScripts(false);
//        }

//        if (_showTime)
//            Debug.LogError("获取所有脚本耗时：" + (DateTime.Now - _time).TotalSeconds + "秒");
//        _time = DateTime.Now;
//        //先获取dlls IOS无法热更Dll
//        if (BuildPlatform != BuildTarget.iOS && isBuildDlls)
//        {
//            if (!GetAllDlls(false))
//            {
//                dllErrorAlert();
//                state = 0;
//                return;
//            }
//        }

//        if (_showTime)
//            Debug.LogError("获取所有dll耗时：" + (DateTime.Now - _time).TotalSeconds + "秒");
//        _time = DateTime.Now;

//        ShowTips("确定需要打包的母资源");

//        //确定需要打包的母资源
//        if (selection == null)
//        {
//            preResources = GetBuildResources();
//            allResources = preResources;
//        }
//        else
//        {
//            //单打
//            preResources = GetBuildResources(selection.ToArray(), true);
//            allResources = GetBuildResources();

//        }

//        if (_showTime)
//            Debug.LogError("获取所有确定需要打包的母资源耗时：" + (DateTime.Now - _time).TotalSeconds + "秒");
//        _time = DateTime.Now;

//        ShowTips("AnimationCurveProxy转换");


//        ShowTips("资源Scripts转换");


//        allLoadObjs.Clear();

//        //打代码
//        if (BuildPlatform != BuildTarget.iOS && isBuildDlls)
//        {
//            BuildDlls();
//        }

//        if (_showTime)
//            Debug.LogError("打代码耗时：" + (DateTime.Now - _time).TotalSeconds + "秒");
//        _time = DateTime.Now;

//        ShowTips("获取所有关联资源");
//        //获取所有关联资源
//        if (!GetAllAssets(false))
//        {
//            //restoreResources();
//            state = 0;
//            return;
//        }

//        if (_showTime)
//            Debug.LogError("获取所有关联资源耗时：" + (DateTime.Now - _time).TotalSeconds + "秒");
//        _time = DateTime.Now;


//        //清理AssetInfoList
//        bulidAssetInfoList.Clear();


//        ShowTips("打包资源");

//        //打包资源
//        BuildResource(selection, buildManiFestOnly);


//        if (_showTime)
//            Debug.LogError("打包耗时：" + (DateTime.Now - _time).TotalSeconds + "秒");

//        Caching.CleanCache();
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();


//        if (bulidAssetInfoList.Count > 0)
//        {

//            saveBuildAssetInfo();

//        }

//        Clear();
//        EditorUtility.UnloadUnusedAssets();
//        Resources.UnloadUnusedAssets();
//        state = 0;


//        if (_showTime)
//            Debug.LogError("总耗时：" + (DateTime.Now - _start).TotalSeconds + "秒");


//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//    }
//    private static void deletAllManiFest()
//    {
//        List<string> _manifestList = GetTheResources(assetbundleFilePath, ".manifest");
//        for (int i = 0; i < _manifestList.Count; ++i)
//        {
//            File.Delete(_manifestList[i]);
//        }

//    }
//    public static List<string> GetTheResources(string resourcePath, string suffix)
//    {


//        if (string.IsNullOrEmpty(resourcePath) || !resourcePath.Contains(Application.dataPath))
//        {
//            Debug.LogError("非法路径");
//            return null;
//        }

//        string[] files = Directory.GetFiles(resourcePath, "*.*", SearchOption.AllDirectories).Where(s =>


//              s.ToLower().EndsWith(suffix)).ToArray();

//        string rootPath = Application.dataPath.Replace("Assets", "");
//        for (int i = 0; i < files.Length; i++)
//        {
//            files[i] = files[i].Replace(rootPath, "");
//            files[i] = files[i].Replace(@"\", "/");
//        }

//        List<string> _list = files.ToList();

//        return _list;
//    }



//    private static bool saveBuildAssetInfo()
//    {
//        _stringBuilder.Length = 0;

//        int i, len = bulidAssetInfoList.Count;
//        for (i = 0; i < len; i++)
//        {
//            if (i > 0)
//            {
//                _stringBuilder.Append("\n");
//            }
//            _stringBuilder.Append(bulidAssetInfoList[i]);
//        }
//        /*
//        string savePath = EditorUtility.SaveFilePanel("保存Info", "", "BuildAssetsInfo", "txt");
//        if (!string.IsNullOrEmpty(savePath)) {

//            return AorIO.SaveStringToFile(savePath, _stringBuilder.ToString());

//        }*/
//        string savePath = assetbundleFilePath + "/GameRes/list.settings";
//        return AorIO.SaveStringToFile(savePath, _stringBuilder.ToString()); ;
//    }

//    public static void AnimationCurveProxy()
//    {
//        int i, len = allResources.Count;
//        //        foreach (string str in preResources)
//        for (i = 0; i < len; i++)
//        {
//            //根据路径获取asset资源
//            string str = allResources[i];

//            if (BuildCommon.getFileSuffix(str) == "prefab")
//            {
//                _tempObj = null;
//                allLoadObjs.TryGetValue(str, out _tempObj);

//                if (!_tempObj)
//                {
//                    _tempObj = AssetDatabase.LoadAssetAtPath(str, typeof(Object));
//                    allLoadObjs.Add(str, _tempObj);
//                }

//                if (_tempObj is GameObject)
//                {

//                    Animation an = ((GameObject)_tempObj).FindComponentIncParent<Animation>();
//                    Animator ar = ((GameObject)_tempObj).FindComponentIncParent<Animator>();
//                    if (an != null || ar != null)
//                    {
//                        AnimationCurveProxyEditor.TrasformToACProxy((GameObject)_tempObj);
//                        EditorUtility.SetDirty(_tempObj);
//                    }
//                }
//            }

//            else
//            {
//                continue;
//            }

//        }
//        _tempObj = null;

//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/单独生成BuildAssetInfo报表")]
//    public static void CreateBuildAssetInfoFromExists()
//    {


//        return;
//        bulidAssetInfoList.Clear();
//        string[] files = Directory.GetFiles(assetbundleFilePath, "*.*", SearchOption.AllDirectories);

//        int i, len = files.Length;
//        //        foreach (string filePath in files)
//        for (i = 0; i < len; i++)
//        {

//            string filePath = files[i];

//            string fPath = filePath.Replace("\\", "/");

//            string suffix = "";
//            if (fPath.LastIndexOf('.') != -1)
//            {
//                suffix = fPath.Substring(fPath.LastIndexOf('.')).ToLower();
//            }

//            if (string.IsNullOrEmpty(suffix))
//            {
//                Debug.LogError("*** BuildAssetBundle.CreateBuildAssetInfoFromExists Error :: some file have not suffix = " + fPath);
//                continue;
//            }
//            if (fPath.Contains("GameRes/list.settings"))
//            {
//                continue;
//            }
//            if (suffix == ".meta")
//            {
//                continue;
//            }

//            _stringBuilder.Length = 0;
//            string infoPath = fPath.Replace(assetbundleFilePath + "/", "");
//            int fileSize = 0;
//            string fileMD5 = BuildCommon.GetFileMD5(fPath, ref fileSize);
//            string pathMD5 = GetStringMD5(infoPath);
//            _stringBuilder.Append(pathMD5 + "|");
//            _stringBuilder.Append(fileMD5 + "|");
//            _stringBuilder.Append(infoPath + "|");
//            _stringBuilder.Append(fileSize.ToString());
//            bulidAssetInfoList.Add(_stringBuilder.ToString());
//        }

//        if (bulidAssetInfoList.Count > 0)
//        {
//            saveBuildAssetInfo();
//        }

//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/列出需要打包的脚本")]
//    public static void GetAllScriptsMenu()
//    {
//        GetAllScripts(true);
//    }

//    private static List<string> _backupPrefabList = new List<string>();
//    private static Dictionary<string, string> _backupPrefabDic = new Dictionary<string, string>();

//    public static void backupResources()
//    {

//        _backupPrefabList.Clear();
//        _backupPrefabDic.Clear();

//        if (Directory.Exists(assetbundleFileBackup))
//        {
//            Directory.Delete(assetbundleFileBackup, true);
//        }

//        Directory.CreateDirectory(assetbundleFileBackup);

//        string FileName, suffix, FilePath, disPath, subDir;

//        int i, len = allResources.Count;
//        //        foreach (string str in preResources)
//        for (i = 0; i < len; i++)
//        {
//            string str = allResources[i];
//            FileName = str.Substring(str.LastIndexOf('/') + 1);
//            suffix = "";
//            if (FileName.LastIndexOf('.') != -1)
//            {
//                suffix = FileName.Substring(FileName.LastIndexOf('.')).ToLower();
//            }
//            if (!string.IsNullOrEmpty(suffix))
//            {

//                if (suffix == ".prefab" || suffix == ".unity" || suffix == ".anim" || suffix == ".controller")
//                {
//                    FilePath = Application.dataPath.Replace("Assets", "") + str;

//                    disPath = assetbundleFileBackup + "/" + FilePath.Replace(Application.dataPath + "/", "");
//                    if (_backupPrefabDic.ContainsKey(disPath))
//                    {
//                        Debug.LogError("*** BuildAssetBundle.backupResources Error :: same key already exists in Dictionary , key = " + disPath);
//                    }
//                    else
//                    {
//                        _backupPrefabDic.Add(disPath, FilePath);
//                        _backupPrefabList.Add(disPath);

//                        subDir = disPath.Replace("/" + FileName, "");
//                        if (!Directory.Exists(subDir))
//                        {
//                            Directory.CreateDirectory(subDir);
//                        }

//                        File.Copy(FilePath, disPath, true);
//                    }
//                }
//            }
//        }

//    }

//    public static void restoreResources()
//    {

//        if (_backupPrefabList.Count > 0)
//        {

//            int i, len = _backupPrefabList.Count;

//            for (i = 0; i < len; i++)
//            {
//                string filePath = _backupPrefabList[i];
//                File.Copy(filePath, _backupPrefabDic[filePath], true);
//            }

//            if (Directory.Exists(assetbundleFileBackup))
//            {
//                /*
//                string metaPath = assetbundleFilePath + "/" + assetbundleFileBackupFName + ".meta";
//                if (File.Exists(metaPath))
//                {
//                    File.Delete(metaPath);
//                }*/

//                Directory.Delete(assetbundleFileBackup, true);
//            }
//        }

//        _backupPrefabList.Clear();
//        _backupPrefabDic.Clear();

//    }

//    /// <summary>
//    /// 脚本转换
//    /// </summary>
//    public static void ScriptsCover(List<string> resources)
//    {
//        int i, len = resources.Count;
//        allLoadObjs.Clear();
//        //先转选中的资源
//        for (i = 0; i < len; i++)
//        {
//            //根据路径获取asset资源
//            string str = resources[i];

//            if (BuildCommon.getFileSuffix(str).Equals("prefab"))
//            {


//                _tempObj = null;
//                allLoadObjs.TryGetValue(str, out _tempObj);

//                if (!_tempObj)
//                {
//                    _tempObj = AssetDatabase.LoadAssetAtPath(str, typeof(Object));
//                    allLoadObjs.Add(str, _tempObj);
//                }


//                if (_tempObj is GameObject)
//                {
//                    ScriptCover.ScriptChange((_tempObj as GameObject));
//                    EditorUtility.SetDirty(_tempObj);
//                }
//                _tempObj = null;
//            }
//            else
//            {
//                continue;
//            }

//        }


//        _tempObj = null;
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//    }

//    /// <summary>
//    /// 脚本转换
//    /// </summary>
//    public static void ScriptsCover()
//    {
//        int i, len = allResources.Count;
//        //先转选中的资源
//        for (i = 0; i < len; i++)
//        {
//            //根据路径获取asset资源
//            string str = allResources[i];

//            if (BuildCommon.getFileSuffix(str).Equals("prefab"))
//            {


//                _tempObj = null;
//                allLoadObjs.TryGetValue(str, out _tempObj);

//                if (!_tempObj)
//                {
//                    _tempObj = AssetDatabase.LoadAssetAtPath(str, typeof(Object));
//                    allLoadObjs.Add(str, _tempObj);
//                }


//                if (_tempObj is GameObject)
//                {
//                    ScriptCover.ScriptChange((_tempObj as GameObject));
//                    EditorUtility.SetDirty(_tempObj);
//                }
//                _tempObj = null;
//            }
//            else
//            {
//                continue;
//            }

//        }
//        //转单独打包资源
//        //        if (BuildEachPrefabPathList.Count > 0)
//        //        {
//        //            for (int j = 0; j < BuildEachPrefabPathList.Count; j++)
//        //            {
//        //                if (string.IsNullOrEmpty(BuildCommon.getFileSuffix(BuildEachPrefabPathList[j])))
//        //                {
//        //                    //转文件夹内的预制体
//        //                    string[] prefabObjects = AssetDatabase.FindAssets("t:Prefab", new string[] { BuildEachPrefabPathList[j] });
//        //
//        //                    if (prefabObjects != null && prefabObjects.Length > 0)
//        //                    {
//        //
//        //                        for (int u = 0; u < prefabObjects.Length; u++)
//        //                        {
//        //                            //editor不管 
//        //                            if (IsPathInEditorOnlyPathList(AssetDatabase.GUIDToAssetPath(prefabObjects[u])))
//        //                                continue;
//        //
//        //                            GameObject go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(prefabObjects[u]), typeof(Object)) as GameObject;
//        //                            //已经转过的走掉
//        //
//        //                            ScriptDataStorage[] ms = go.GetComponents<ScriptDataStorage>();
//        //                            if (ms.Length != 0)
//        //                            {
//        //                                continue;
//        //                            }
//        //
//        //
//        //                            ScriptCover.ScriptChange(go);
//        //                            EditorUtility.SetDirty(go);
//        //                        }
//        //                    }
//        //
//        //                }
//        //            }
//        //        }

//        _tempObj = null;
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//    }

//    public static bool GetAllDlls(bool justLog)
//    {
//        allDlls.Clear();
//        string[] files = Directory.GetFiles(Application.dataPath, "*.dll", SearchOption.AllDirectories);

//        return GetDlls(files, justLog);
//    }

//    public static bool GetDlls(string[] files, bool justLog)
//    {

//        if (justLog)
//            BuildCommon.Log("***************Dll检查开始*********************");

//        int i, len = files.Length;
//        //        foreach (string file in files)
//        for (i = 0; i < len; i++)
//        {
//            string file = files[i];
//            string suffix = "";
//            if (file.LastIndexOf('.') != -1)
//            {
//                suffix = file.Substring(file.LastIndexOf('.')).ToLower();
//            }

//            if (suffix == ".dll")
//            {

//                string realFile = file.Replace("\\", "/");
//                realFile = realFile.Replace(Application.dataPath, "Assets");

//                //强制检查
//                if (realFile.Contains("/UnityEngine.dll") || realFile.Contains("/UnityEngine.UI.dll") || realFile.Contains("/UnityEditor.dll") || realFile.Contains("/UnityEditor.UI.dll"))
//                {
//                    return false;
//                }

//                if (justLog)
//                {
//                    BuildCommon.Log(file);
//                }
//                else
//                {
//                    if (realFile.Contains("Editor") || realFile.Contains("editor"))
//                        continue;

//                    if (realFile.Contains("UnityEngine.dll") || realFile.Contains("YoukiaBridge.dll") || realFile.Contains("UnityEngine.UI.dll"))
//                        continue;



//                    if (allDlls.Contains(realFile))
//                        continue;

//                    allDlls.Add(realFile);
//                }

//            }
//        }

//        if (justLog)
//        {
//            BuildCommon.Log("***************Dll检查结束*********************");
//            return true;
//        }

//        List<string> newPaths = new List<string>();

//        len = allDlls.Count;
//        //        foreach (string path in allDlls)
//        for (i = 0; i < len; i++)
//        {
//            string path = allDlls[i];
//            string name = BuildCommon.GetName(path, false);

//            BuildCommon.CheckFolder("Assets/tmp/");
//            AssetDatabase.CopyAsset(path, "Assets/tmp/" + name + assetbundleFileSuffix);

//            string realFile = ("Assets/tmp/" + name + assetbundleFileSuffix).Replace("\\", "/");

//            realFile = realFile.Replace(Application.dataPath, "Assets");

//            if (newPaths.Contains(realFile))
//                continue;
//            newPaths.Add(realFile);

//        }

//        allDlls = newPaths;

//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//        return true;
//    }

//    private static HashSet<string> _renameDic;
//    /// <summary>
//    /// 检查重名文件,并自动重命名;
//    /// </summary>
//    [MenuItem("Youkia/资源管理/一键修改重名文件")]
//    public static void DoRanameAssets()
//    {

//        if (_renameDic == null)
//        {
//            _renameDic = new HashSet<string>();
//        }

//        List<EditorAssetInfo> infos = EditorAssetInfo.FindEditorAssetInfoInPath(Application.dataPath + "/Resources/", false);

//        int i, len = infos.Count;
//        EditorAssetInfo info;
//        for (i = 0; i < len; i++)
//        {

//            EditorUtility.DisplayProgressBar("重名文件检查", "正在工作" + i + " / " + len, (float)i / len);

//            info = infos[i];
//            string p = info.path;
//            if (p.ToLower().Contains(".dll") || !p.Contains("Assets/Resources/"))
//                continue;

//            if (p.Contains("Assets/Resources/AorUIRes/AorUISample"))
//                continue;

//            string k = info.resPath;
//            if (_renameDic.Contains(k))
//            {
//                string newName = info.name + "_R_" + info.suffix.Replace(".", "");
//                //                string newPath = unit.longPath.Replace(unit.Name, "") + newName + "." + unit.Suffix;
//                string newPath = info.path.Replace(info.name + info.suffix, "") + newName + info.suffix;

//                BuildCommon.LogWarning(info.dirPath + " 检测到已经存在同名但后缀名不同的文件: " + info.name + ", 正在尝试重命名为 :" + newName);

//                string newp;
//                AssetDatabase.DeleteAsset(newName);
//                AssetDatabase.Refresh();
//                newp = AssetDatabase.RenameAsset(info.path, newName);
//                if (string.IsNullOrEmpty(newp))
//                {
//                    Debug.Log("BuildAssetBundle.DoRanameAssets > rename :: " + info.path + " > " + newPath);
//                    AssetDatabase.SaveAssets();
//                    AssetDatabase.Refresh();
//                }
//                else
//                {



//                    //   BuildCommon.LogError("*** 重名发生错误: " + info.name + " 已经存在同名但后缀名不同的文件,且自动重命名失败. :: + " + newp);
//                }
//            }
//            else
//            {
//                _renameDic.Add(k);
//            }
//        }

//        EditorUtility.ClearProgressBar();

//        _renameDic.Clear();
//        _renameDic = null;
//    }



//    public static void GetAllScripts(bool justLog)
//    {

//        string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

//        GetScripts(files, justLog);
//    }

//    public static void GetScripts(string[] files, bool justLog)
//    {

//        if (justLog)
//            BuildCommon.Log("***************脚本检查开始*********************");

//        int i, len = files.Length;
//        //        foreach (string file in files)
//        for (i = 0; i < len; i++)
//        {
//            string file = files[i];
//            string suffix = "";
//            if (file.LastIndexOf('.') != -1)
//            {
//                suffix = file.Substring(file.LastIndexOf('.')).ToLower();
//            }

//            if (suffix == ".cs")
//            {

//                string realFile = file.Replace("\\", "/");
//                realFile = realFile.Replace(Application.dataPath, "Assets");

//                //不打editor脚本
//                if (BuildCommon.getFolder(realFile).Contains("Editor"))
//                    continue;

//                if (justLog)
//                {
//                    BuildCommon.Log(file);
//                }
//                else
//                {
//                    allScripts.Add(realFile);
//                }

//            }
//        }

//        if (justLog)
//            BuildCommon.Log("***************脚本检查结束*********************");
//    }

//    public static Dictionary<string, AllFileMD5.EMd5State> CompareMD5(AllFileMD5 oldMd5s, AllFileMD5 newMd5s)
//    {

//        if (oldMd5s == null || newMd5s == null)
//            return null;

//        // AllFileMD5 newMd5s = CreateMD5Dic();


//        return CompareMD5(oldMd5s.MD5Dic, newMd5s.MD5Dic);

//    }

//    public static Dictionary<string, AllFileMD5.EMd5State> CompareMD5(Dictionary<string, string> oldDic, Dictionary<string, string> newDic, Func<string, bool> conditionFun = null)
//    {
//        Dictionary<string, AllFileMD5.EMd5State> editorDic = new Dictionary<string, AllFileMD5.EMd5State>();

//        int i, len;
//        if (conditionFun != null)
//        {

//            string[] keys = newDic.Keys.ToArray();
//            len = keys.Length;
//            //            foreach (KeyValuePair<string, string> each in newDic)
//            for (i = 0; i < len; i++)
//            {
//                if (conditionFun(keys[i]))
//                {
//                    editorDic.Add(keys[i], AllFileMD5.EMd5State.fixUpdate);
//                    continue;
//                }

//            }

//            return editorDic;
//        }

//        string[] newDicKeys = newDic.Keys.ToArray();
//        len = newDicKeys.Length;
//        //        foreach (KeyValuePair<string, string> each in newDic)
//        for (i = 0; i < len; i++)
//        {

//            if (!oldDic.ContainsKey(newDicKeys[i]))
//            {
//                //新添
//                editorDic.Add(newDicKeys[i], AllFileMD5.EMd5State.newObj);

//            }
//            else
//            {
//                if (oldDic[newDicKeys[i]] != newDic[newDicKeys[i]])
//                {
//                    //修改
//                    editorDic.Add(newDicKeys[i], AllFileMD5.EMd5State.modify);

//                }

//            }
//        }

//        //删除的
//        string[] oldDicKeys = oldDic.Keys.ToArray();
//        len = oldDicKeys.Length;
//        //        foreach (KeyValuePair<string, string> each in oldDic)
//        for (i = 0; i < len; i++)
//        {
//            if (!newDic.ContainsKey(oldDicKeys[i]))
//            {

//                string delPath = "";
//                string delGUID = BuildAssetBundle.AssetPathToGUID(oldDicKeys[i]);
//                if (!string.IsNullOrEmpty(delGUID))
//                {
//                    delPath = oldDicKeys[i].Substring(0, oldDicKeys[i].LastIndexOf('/') + 1);

//                    delPath = delPath.Replace("Assets/Resources/", "StreamingAssets/StreamingResources/");
//                    delPath = delPath + delGUID + ".bytes";
//                }
//                else
//                {
//                    Debug.LogError("BuildAssetBundle.CompareMD5 Error :: 无法将Path转换成GUID. path = " + oldDicKeys[i]);
//                    continue;
//                }

//                editorDic.Add(delPath, AllFileMD5.EMd5State.delete);

//            }

//        }

//        return editorDic;

//    }

//    public static Dictionary<string, AllFileMD5.EMd5State> CompareABMD5(Dictionary<string, string> oldDic,
//        Dictionary<string, string> newDic)
//    {

//        if (oldDic == null) return null;

//        int i, len;
//        Dictionary<string, AllFileMD5.EMd5State> dic = new Dictionary<string, AllFileMD5.EMd5State>();

//        string[] newDicKeys = newDic.Keys.ToArray();
//        len = newDicKeys.Length;
//        //        foreach (KeyValuePair<string, string> keyValuePair in newDic)
//        for (i = 0; i < len; i++)
//        {

//            if (!oldDic.ContainsKey(newDicKeys[i]))
//            {
//                //手动新加了AB文件
//                dic.Add(newDicKeys[i], AllFileMD5.EMd5State.lostUpdate);
//            }
//            else
//            {
//                if (!oldDic.ContainsValue(newDic[newDicKeys[i]]))
//                {
//                    //ab文件被修改
//                    dic.Add(newDicKeys[i], AllFileMD5.EMd5State.lostUpdate);
//                }
//            }

//        }
//        string[] oldDicKeys = oldDic.Keys.ToArray();
//        len = oldDicKeys.Length;
//        //        foreach (KeyValuePair<string, string> keyValuePair in oldDic)
//        for (i = 0; i < len; i++)
//        {
//            if (!newDic.ContainsKey(oldDicKeys[i]))
//            {
//                //多余要被删除的
//                dic.Add(oldDicKeys[i], AllFileMD5.EMd5State.delete);
//            }
//        }

//        return dic;

//    }

//    public static void CreateMD5AndSave()
//    {
//        AllFileMD5 md5s = CreateMD5Dic(true);
//        MD5Save(md5s);

//    }


//    public static AllFileMD5 newAllFileMD5; //全局缓存
//    public static void MD5Save(AllFileMD5 md5s)
//    {

//        //生成AB包的MD5数据用于下一次对比。
//        CreateABMD5DicInAllFileMD5(md5s);

//        string timestring = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
//        AssetDatabase.CreateAsset(md5s, "Assets/MD5s_" + timestring + ".asset");
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//    }

//    private static void CreateABMD5DicInAllFileMD5(AllFileMD5 md5s)
//    {
//        md5s.ABMD5Dic = CreateABMD5Dic();
//        md5s.SaveABData();
//    }

//    public static Dictionary<string, string> CreateABMD5Dic()
//    {
//        Dictionary<string, string> dic = new Dictionary<string, string>();

//        string[] abPaths = Directory.GetFiles(Application.dataPath + "/StreamingAssets", "*.bytes", SearchOption.AllDirectories);

//        int i, len = abPaths.Length;

//        for (i = 0; i < len; i++)
//        {

//            string abPath = abPaths[i];

//            string fullPath = abPath.Replace("\\", "/");

//            string p = fullPath.Replace(Application.dataPath + "/", "");

//            string abMd5 = BuildCommon.GetFileMD5(fullPath);

//            if (!dic.ContainsKey(p))
//            {
//                dic.Add(p, abMd5);
//            }
//            else
//            {
//                //throw new Exception("错误！ 在创建ABMD5数据的时候，发现了重复的GUID ... ... 这将引发不可预计的错误。 请手动检查 ： GUID：：" + guid + " . path = " + p);
//            }
//        }

//        return dic;
//    }

//    public static AllFileMD5 CreateMD5Dic(bool rebuildingAllResourcesList = false)
//    {
//        AllFileMD5 md5s = ScriptableObject.CreateInstance<AllFileMD5>();
//        md5s.MD5Dic = new Dictionary<string, string>();

//        List<string> pathList = new List<string>();

//        List<string> tmp = GetBuildResources(rebuildingAllResourcesList);

//        for (int j = 0; j < tmp.Count; j++)
//        {
//            //在独立打包内的文件排除,只能靠后面的GetDependencies添加
//            if (!IsPathInBuildEachPrefabPathList(tmp[j]))
//                pathList.Add(tmp[j]);
//        }

//        string[] allExportAssets;
//        //根据基础资源 获取所有依赖资源,已经过滤重复

//        allExportAssets = AssetDatabase.GetDependencies(pathList.ToArray());

//        int i, len = allExportAssets.Length;
//        for (i = 0; i < len; i++)
//        {
//            string p = allExportAssets[i];
//            addPathToMd5(md5s, p);
//        }

//        //添加独立打包文件,这些文件不搜索引用
//        //*文件夹拿不到Md5,所以每次对比都显示要重新打*
//        for (int j = 0; j < BuildEachPrefabPathList.Count; j++)
//        {
//            addPathToMd5(md5s, BuildEachPrefabPathList[j]);
//        }


//        md5s.SaveData();

//        return md5s;
//    }

//    static bool addPathToMd5(AllFileMD5 md5s, string p)
//    {


//        if (p.ToLower().Contains(".dll") || !p.Contains("Assets/Resources/"))
//            return false;
//        string realFile = p;
//        string tmp = Application.dataPath.Replace("Assets", "");
//        realFile = tmp + realFile;
//        try
//        {
//            md5s.MD5Dic.Add(p, BuildCommon.GetFileMD5(realFile));
//        }
//        catch
//        {
//            Debug.LogError("md5重复:" + realFile);
//        }

//        return true;
//    }

//    private static List<string> _buildResources;
//    /// <summary>
//    /// 确定需要打包的母资源
//    /// </summary>
//    public static List<string> GetBuildResources(bool rebuilding = false)
//    {

//        if (rebuilding || _buildResources == null)
//        {
//            if (_buildResources != null)
//            {
//                _buildResources.Clear();
//            }
//            //Resource资源路径
//            string resourcePath = Application.dataPath + "/Resources/";
//            string[] files = Directory.GetFiles(resourcePath, "*.*", SearchOption.AllDirectories).Where(v => !v.EndsWith(".meta")).ToArray();
//            _buildResources = GetBuildResources(files);
//        }
//        return _buildResources;
//    }

//    public static List<string> GetBuildResources(string[] files, bool fileCheck = true)
//    {
//        ignorAssetList.Clear();
//        allShaderList.Clear();
//        allMaterialList.Clear();
//        allFBXList.Clear();
//        allAssetInfos.Clear();
//        allLevelAssets.Clear();


//        List<string> list = new List<string>();

//        int i, len = files.Length;

//        for (i = 0; i < len; i++)
//        {

//            string file = files[i];
//            string realFile = file.Replace("\\", "/");
//            realFile = realFile.Replace(Application.dataPath, "Assets");

//            if (fileCheck)
//            {
//                if (!checkFile(realFile))
//                    continue;
//            }


//            list.Add(realFile);
//        }

//        return list;

//    }


//    /// <summary>
//    /// 获得文件夹内的所有资源
//    /// </summary>
//    public static HashSet<string> GetFolderResources(string resourcePath)
//    {

//        HashSet<string> allfile = new HashSet<string>();
//        //Resource资源路径
//        string[] files = Directory.GetFiles(resourcePath, "*.*", SearchOption.AllDirectories);

//        int i, len = files.Length;

//        for (i = 0; i < len; i++)
//        {

//            string file = files[i];
//            string realFile = file.Replace("\\", "/");
//            realFile = realFile.Replace(Application.dataPath, "Assets");


//            if (string.IsNullOrEmpty(realFile) || !checkFile(realFile, false))
//                continue;

//            if (!allfile.Contains(realFile))
//                allfile.Add(realFile);

//        }


//        return allfile;
//    }

//    static bool SuffixCheck(string suffix)
//    {

//        if (suffix.ToLower() != "prefab" && suffix.ToLower() != "txt" && suffix.ToLower() != "csv" && suffix.ToLower() != "json" && suffix.ToLower() != "anim")
//        {
//            return false;
//        }
//        return true;
//    }

//    static bool checkFile(string path, bool checkSuffix = true)
//    {


//        string suffix = BuildCommon.getFileSuffix(path);

//        if (suffix.ToLower() == "" || suffix.ToLower() == "unity" || suffix.ToLower() == "meta")
//            return false;

//        if (!path.Contains("Assets/Resources/"))
//            return false;

//        if (IsPathInEditorOnlyPathList(path))
//            return false;

//        if (path.Contains("/.."))
//            return false;

//        if (BuildPlatform == BuildTarget.Android && BuildCommon.GetName(path, true) == "DllLinker_IOS.prefab")
//        {
//            return false;
//        }

//        if (checkSuffix && !SuffixCheck(suffix))
//        {
//            if (!IsPathInConstBuildePathList(path))
//            {
//                return false;
//            }

//        }


//        //基础info不要重复打
//        if (path == "Assets/Resources/MainFest.prefab" || path == "Assets/Resources/UIBase.prefab")
//        {
//            return false;

//        }

//        return true;
//    }
//    //路径是否在不打包路径字典内
//    public static bool IsPathInEditorOnlyPathList(string path)
//    {

//        if (constEditorOnlyPathList == null)
//        {
//            constEditorOnlyPathList = new List<string>();
//            string[] paths = AssetDatabase.FindAssets(EDITOR_ONLY);
//            for (int i = 0; i < paths.Length; i++)
//            {
//                constEditorOnlyPathList.Add(AssetDatabase.GUIDToAssetPath(paths[i]));
//            }
//        }

//        for (int i = 0; i < constEditorOnlyPathList.Count; i++)
//        {

//            if (path == constEditorOnlyPathList[i])
//                return true;

//            if (IsInPath(path, constEditorOnlyPathList[i]))
//                return true;
//        }

//        return false;
//    }

//    static bool IsInPath(string path, string comparePath)
//    {

//        string[] paths = path.Split('/');
//        string[] paths2 = comparePath.Split('/');

//        if (paths.Length > paths2.Length)
//        {
//            for (int j = 0; j < paths2.Length; j++)
//            {
//                if (paths[j] != paths2[j])
//                    return false;

//            }
//            //遍历完对比路径相等,说明在路径下
//            return true;
//        }
//        else
//        {
//            //长度短或等,那么肯定不相同
//            return false;
//        }
//    }

//    //路径是否在固定打包路径字典内
//    public static bool IsPathInConstBuildePathList(string path)
//    {

//        if (constBuildePathList == null)
//        {
//            constBuildePathList = new List<string>();
//            string[] paths = AssetDatabase.FindAssets(BUILD_SINGLE);
//            for (int i = 0; i < paths.Length; i++)
//            {
//                constBuildePathList.Add(AssetDatabase.GUIDToAssetPath(paths[i]));
//            }
//        }



//        for (int i = 0; i < constBuildePathList.Count; i++)
//        {
//            if (path == constBuildePathList[i])
//                return true;

//            if (IsInPath(path, constBuildePathList[i]))
//                return true;
//        }

//        return false;
//    }

//    public static void GetAllBuildEachPrefab()
//    {
//        if (null == _BuildEachPrefabPathList)
//        {
//            _BuildEachPrefabPathList = new List<string>();
//            string[] paths = AssetDatabase.FindAssets(BUILD_EACH_PREFAB);
//            for (int i = 0; i < paths.Length; i++)
//            {
//                _BuildEachPrefabPathList.Add(AssetDatabase.GUIDToAssetPath(paths[i]));
//            }
//        }
//    }

//    //路径是否在只打预制体的路径字典内
//    public static bool IsPathInBuildEachPrefabPathList(string path)
//    {
//        if (_BuildEachPrefabPathList == null)
//        {
//            GetAllBuildEachPrefab();
//        }

//        for (int i = 0; i < _BuildEachPrefabPathList.Count; i++)
//        {

//            if (path == _BuildEachPrefabPathList[i])
//                return true;

//            if (IsInPath(path, _BuildEachPrefabPathList[i]))
//                return true;
//        }

//        return false;
//    }

//    public static HashSet<string> GetParentResources(string[] selection)
//    {

//        List<string> all = GetBuildResources();
//        HashSet<string> sets = new HashSet<string>();

//        for (int i = 0; i < selection.Length; i++)
//        {

//            string suffix = BuildCommon.getFileSuffix(selection[i]);

//            if (suffix.ToLower() == "unity" || suffix.ToLower() == "meta" || suffix.ToLower() == "dll")
//                continue;

//            for (int j = 0; j < all.Count; j++)
//            {
//                string[] deps = AssetDatabase.GetDependencies(new string[] { all[j] });

//                for (int k = 0; k < deps.Length; k++)
//                {
//                    if (deps[k] == selection[i])
//                    {
//                        sets.Add(all[j]);
//                        break;
//                    }
//                }


//            }
//        }

//        return sets;
//    }

//    /// <summary>
//    /// 获得选择物件以及他的所有依赖物件路径
//    /// </summary>
//    /// <param name="selection"></param>
//    /// <returns></returns>
//    public static HashSet<string> GetSelectedResources(string[] selection)
//    {

//        if (selection.Length == 0)
//            return null;


//        List<string> selectionOK = new List<string>();

//        int i, len = selection.Length;

//        for (i = 0; i < len; i++)
//        {

//            string assetPath = selection[i];
//            if (string.IsNullOrEmpty(assetPath))
//                continue;

//            if (!selectionOK.Contains(assetPath))
//                selectionOK.Add(assetPath);

//        }


//        if (selectionOK.Count == 0)
//            return null;

//        List<string> allpath = new List<string>();

//        //        foreach (string path in selectionOK)
//        len = selectionOK.Count;
//        for (i = 0; i < len; i++)
//        {
//            string path = selectionOK[i];
//            if (!string.IsNullOrEmpty(path) && !allpath.Contains(path))
//            {
//                allpath.Add(path);
//            }
//        }

//        string[] depList = AssetDatabase.GetDependencies(allpath.ToArray());
//        HashSet<string> SelectObjPaths = new HashSet<string>();
//        len = depList.Length;
//        for (i = 0; i < len; i++)
//        {
//            if (!SelectObjPaths.Contains(depList[i]))
//                SelectObjPaths.Add(depList[i]);
//        }

//        return SelectObjPaths;

//    }

//    static void AddToLevelAssets(AssetInfo unit)
//    {
//        //Asset等级
//        int level = unit.Level;

//        //存在
//        if (allLevelAssets.ContainsKey(level))
//        {
//            allLevelAssets[level].Add(unit.Path, unit);
//        }
//        else
//        {
//            //添加等级
//            Dictionary<string, AssetInfo> levelAsset = new Dictionary<string, AssetInfo>();
//            allLevelAssets.Add(level, levelAsset);
//            //添加asset信息
//            allLevelAssets[level].Add(unit.Path, unit);
//        }
//    }

//    // private List<AssetInfo> matlist=new List<AssetInfo>(); 
//    public static bool GetAllAssets(bool justlog)
//    {
//        //标识为单打预制体的,最后统一单独打
//        for (int i = 0; i < BuildEachPrefabPathList.Count; i++)
//        {
//            string path = BuildEachPrefabPathList[i];
//            AssetInfo unit = BuildCommon.CreateInfo(path, false);
//            unit.IsBuildEachPrefab = true;
//            if (unit.Path.ToLower().Contains(".shader"))
//                continue;
//            if (IsPathInEditorOnlyPathList(unit.longPath))
//                continue;

//            allAssetInfos.Add(unit.PathWithOutSuffix, unit);
//            SingleAssetInfos.Add(unit);

//        }

//        //从总资源里移除项目
//        for (int i = 0; i < allResources.Count; i++)
//        {

//            string path = allResources[i];


//            if (IsPathInBuildEachPrefabPathList(path))
//            {
//                preResources.Remove(path);
//                allResources.Remove(path);
//                i--;
//            }

//        }


//        string[] allExportAssets;

//        //  allResources = GetBuildResources();
//        //根据基础资源 获取所有依赖资源,已经过滤重复
//        allExportAssets = AssetDatabase.GetDependencies(allResources.ToArray());

//        BuildCommon.Log("Build asset number:" + allExportAssets.Length);

//        int len = allExportAssets.Length;
//        string p = "";
//        for (int i = 0; i < len; i++)
//        {

//            p = allExportAssets[i];
//            if (IsPathInEditorOnlyPathList(p))
//                continue;
//            if (p.ToLower().Contains(".shader"))
//                continue;

//            if (p.ToLower().Contains(".dll") || !p.Contains("Assets/Resources/"))
//                continue;

//            AssetInfo unit = BuildCommon.CreateInfo(p);


//            if (allAssetInfos.ContainsKey(unit.PathWithOutSuffix))
//            {
//                BuildCommon.LogError("可能引发的严重错误: " + unit.PathWithOutSuffix + " 已经存在同名但后缀名不同的文件");
//                if (justlog)
//                {
//                    continue;
//                }
//                else
//                {
//                    return false;
//                }
//            }



//            allAssetInfos.Add(unit.PathWithOutSuffix, unit);

//        }



//        AssetInfo[] infos = allAssetInfos.Values.ToArray();

//        len = infos.Length;
//        for (int i = 0; i < len; i++)
//        {
//            AssetInfo unit = infos[i];
//            AddToLevelAssets(unit);
//        }

//        //        KeyValuePair<string, AssetInfo>[] level1 = allLevelAssets[1].ToArray();
//        //        AssetInfo info = null;
//        //        AssetInfo compUnit = null;
//        //        for (int n = 0; n < level1.Length; n++)
//        //        {
//        //            info = level1[n].Value;
//        //            for (int level = 2; level <= allLevelAssets.Count; level++)
//        //            {
//        //
//        //                KeyValuePair<string, AssetInfo>[] LevelAssets = allLevelAssets[level].ToArray();
//        //
//        //                for (int j = 0; j < LevelAssets.Length; j++)
//        //                {
//        //                    compUnit = LevelAssets[j].Value;
//        //
//        //                    if (info == compUnit || !compUnit.isPrefab())
//        //                        continue;
//        //                    else
//        //                    {
//        //
//        //                        for (int i = 0; i < compUnit.AllDependencies.Length; i++)
//        //                        {
//        //                            if (compUnit.AllDependencies[i] == info.PathWithOutSuffix)
//        //                            {
//        //
//        //                                info.CrossRefCount += 1;
//        //                                break;
//        //                            }
//        //                        }
//        //
//        //
//        //
//        //                    }
//        //
//        //                }
//        //
//        //            }
//        //
//        //        }

//        //检查输出
//        if (justlog)
//        {
//            BuildCommon.Log("*****************资源检查开始*****************");

//            int maxLevel = allLevelAssets.Count;
//            if (maxLevel == 0)
//                return true;

//            for (int level = 1; level <= maxLevel; level++)
//            {
//                Dictionary<string, AssetInfo> assets;
//                if (!allLevelAssets.TryGetValue(level, out assets))
//                {
//                    BuildCommon.LogError("Level:" + level + "Not in allLevelAssets");
//                    continue;
//                }
//                string[] keys = assets.Keys.ToArray();
//                int kl = keys.Length;
//                for (int item = 0; item < kl; item++)
//                {
//                    BuildCommon.Log("Level:" + level + "             " + keys[item]);

//                }

//            }
//            BuildCommon.Log("*****************资源检查结束*****************");

//            //清理
//            preResources.Clear();
//            allResources.Clear();
//            allAssetInfos.Clear();
//            allLevelAssets.Clear();
//            allShaderList.Clear();
//            allMaterialList.Clear();
//            allFBXList.Clear();
//            ignorAssetList.Clear();

//        }

//        return true;
//    }

//    private static void allAssetInfoToString(GameObject obj)
//    {
//        AssetDataStorage ex = obj.AddComponent<AssetDataStorage>();
//        ex.InspectorDicKeys = new string[6]
//        {
//            "Path",
//            "RefCount",
//            "Guid",
//            "Dep",
//            "Type",
//            "Size"
//        };



//        ex.InspectorDicValues = new CommonDataStorage.DicValue[6];


//        AssetDataStorage.DicValue data = new AssetDataStorage.DicValue();
//        data.ValueType = 12;
//        data.StrArray = new List<string>(allAssetInfos.Keys.Count);
//        ex.InspectorDicValues[0] = data;


//        data = new AssetDataStorage.DicValue();
//        data.ValueType = 12;
//        data.StrArray = new List<string>(allAssetInfos.Keys.Count);
//        ex.InspectorDicValues[1] = data;

//        data = new AssetDataStorage.DicValue();
//        data.ValueType = 12;
//        data.StrArray = new List<string>(allAssetInfos.Keys.Count);
//        ex.InspectorDicValues[2] = data;

//        data = new AssetDataStorage.DicValue();
//        data.ValueType = 12;
//        data.StrArray = new List<string>(allAssetInfos.Keys.Count);
//        ex.InspectorDicValues[3] = data;

//        data = new AssetDataStorage.DicValue();
//        data.ValueType = 12;
//        data.StrArray = new List<string>(allAssetInfos.Keys.Count);
//        ex.InspectorDicValues[4] = data;

//        data = new AssetDataStorage.DicValue();
//        data.ValueType = 12;
//        data.StrArray = new List<string>(allAssetInfos.Keys.Count);
//        ex.InspectorDicValues[5] = data;


//        // ex.OtherParma = "@MainFest";

//        AssetInfo[] AssetInfos = allAssetInfos.Values.ToArray();
//        int i, len = AssetInfos.Length;
//        //        foreach (KeyValuePair<string, AssetInfo> each in allAssetInfos)
//        for (i = 0; i < len; i++)
//        {
//            AssetInfo each = AssetInfos[i];
//            ex.InspectorDicValues[0].StrArray.Add(each.Path);
//            ex.InspectorDicValues[1].StrArray.Add(each.CrossRefCount.ToString());
//            ex.InspectorDicValues[2].StrArray.Add(each.Guid);


//            if (each.AllDependencies == null || each.AllDependencies.Length == 0)
//            {
//                //没有也要占位，一一对应
//                if (each.IsBuildEachPrefab)
//                    ex.InspectorDicValues[3].StrArray.Add("-1");
//                else
//                    ex.InspectorDicValues[3].StrArray.Add(null);

//            }
//            else
//            {
//                string deps = "";
//                int u, ulen = each.AllDependencies.Length;
//                for (u = 0; u < ulen; u++)
//                {
//                    string s = each.AllDependencies[u];
//                    if (ignorAssetList.Contains(s))
//                    {
//                        continue;
//                    }

//                    deps = deps + s + ",";
//                }

//                if (!string.IsNullOrEmpty(deps))
//                {
//                    deps = deps.Substring(0, deps.Length - 1);
//                }

//                ex.InspectorDicValues[3].StrArray.Add(deps);

//            }

//            ex.InspectorDicValues[4].StrArray.Add(each.ResType);
//            ex.InspectorDicValues[5].StrArray.Add(each.Size.ToString());

//        }

//    }

//    //找到顶部引用
//    private static Dictionary<string, int> findTop(string kvInfo)
//    {


//        KeyValuePair<string, AssetInfo> lastTop = new KeyValuePair<string, AssetInfo>();
//        Dictionary<string, int> RefCount = new Dictionary<string, int>();

//        string[] keys = allAssetInfos.Keys.ToArray();
//        int i, len = keys.Length;

//        //        foreach (KeyValuePair<string, AssetInfo> each in allAssetInfos)
//        for (i = 0; i < len; i++)
//        {

//            if (keys[i] == kvInfo)
//                continue;


//            //如果子节点包含它
//            if (allAssetInfos[keys[i]].AllDependencies.Contains(kvInfo))
//            {
//                lastTop = new KeyValuePair<string, AssetInfo>(keys[i], allAssetInfos[keys[i]]);

//                if (lastTop.Value.Suffix.ToLower() != "prefab")
//                {
//                    //继续递归找prefab顶
//                    KeyValuePair<string, AssetInfo> tmp = _findTop(lastTop);

//                    if (tmp.Value != null)
//                    {
//                        lastTop = tmp;

//                    }
//                }


//            }

//            //一次遍历后，记录一个top
//            if (lastTop.Value != null)
//            {
//                if (RefCount.ContainsKey(lastTop.Key))
//                {
//                    RefCount[lastTop.Key] += 1;
//                }
//                else
//                {
//                    RefCount.Add(lastTop.Key, 1);
//                }

//            }

//        }

//        return RefCount;


//    }

//    private static KeyValuePair<string, AssetInfo> _findTop(KeyValuePair<string, AssetInfo> kvInfo)
//    {


//        KeyValuePair<string, AssetInfo> lastTop = new KeyValuePair<string, AssetInfo>();

//        string[] keys = allAssetInfos.Keys.ToArray();

//        int i, len = keys.Length;
//        //        foreach (KeyValuePair<string, AssetInfo> each in allAssetInfos)
//        for (i = 0; i < len; i++)
//        {

//            if (keys[i] == kvInfo.Key)
//                continue;



//            if (allAssetInfos[keys[i]].AllDependencies.Contains(kvInfo.Key))
//            {
//                lastTop = new KeyValuePair<string, AssetInfo>(keys[i], allAssetInfos[keys[i]]);
//                KeyValuePair<string, AssetInfo> tmp = _findTop(lastTop);

//                if (tmp.Value != null)
//                {
//                    lastTop = tmp;

//                }

//            }

//        }

//        return lastTop;

//    }



//    [MenuItem("Youkia/AssetBundle打包管理/工具/查看选中资源依赖关系")]
//    public static void CheckDepentResources()
//    {
//        AssetDatabase.Refresh();

//        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
//        string[] allExportAssets = AssetDatabase.GetDependencies(new string[] { path });
//        BuildCommon.Log("********************************");
//        for (int i = 0; i < allExportAssets.Length; i++)
//        {

//            BuildCommon.Log(allExportAssets[i]);

//        }
//        BuildCommon.Log("********************************");
//    }




//    public static GameObject CreateMainfest()
//    {
//        string infoPath = "Assets/Resources/MainFest.prefab";
//        GameObject writeInfo = (GameObject)AssetDatabase.LoadAssetAtPath(infoPath, typeof(GameObject));

//        if (writeInfo != null)
//        {
//            AssetDatabase.DeleteAsset(infoPath);
//        }
//        AssetDatabase.Refresh();
//        GameObject go = new GameObject();
//        go.name = "Mainfest";
//        PrefabUtility.CreatePrefab(infoPath, go);

//        writeInfo = (GameObject)AssetDatabase.LoadAssetAtPath(infoPath, typeof(GameObject));

//        allAssetInfoToString(writeInfo);
//        GameObject.DestroyImmediate(go);
//        return writeInfo;

//    }


//    //创建关系网
//    public static void BuildMainFest()
//    {

//        GameObject writeInfo = CreateMainfest();
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();
//        BuildCommon.CollecateAssetBundle(writeInfo, null, "", "MainFest.bytes");

//    }


//    public static void BuildScripts()
//    {
//        //全关联脚本打包
//        List<Object> Scripts = new List<Object>();

//        int i, len = allScripts.Count;
//        //        foreach (string each in allScripts)
//        for (i = 0; i < len; i++)
//        {
//            string each = allScripts[i];
//            Object asset = AssetDatabase.LoadAssetAtPath(each, typeof(object));
//            Scripts.Add(asset);
//        }

//        GameObject DllLinker = AssetDatabase.LoadAssetAtPath("Assets/Resources/DllLinker.prefab", typeof(Object)) as GameObject;
//        if (DllLinker != null)
//        {
//            if (!Scripts.Contains(DllLinker))
//            {
//                Scripts.Add(DllLinker);
//            }
//        }
//        else
//        {
//            Debug.LogError("**** BuildAssetBundle.BuildScripts Error :: Can not find DllLinker[Assets/Resources/DllLinker.prefab] ***");
//        }

//        BuildCommon.CheckFolder("Assets/StreamingAssets/StreamingResources");
//        BuildCommon.BuildAssetBundles(DllLinker, Scripts.ToArray(), "Assets/StreamingAssets/StreamingResources/", "AllScripts.bytes", options, BuildPlatform);


//    }

//    public static void BuildDlls()
//    {

//        BuildCommon.CheckFolder("Assets/StreamingAssets/Dlls");
//        GameObject dllMainFest = new GameObject("DllMainFest");


//        CommonDataStorage storage = dllMainFest.AddComponent<CommonDataStorage>();
//        storage.InspectorDicKeys = new string[allDlls.Count];

//        List<AssetBundleBuild> _list = new List<AssetBundleBuild>();
//        for (int i = 0; i < allDlls.Count; i++)
//        {
//            string each = allDlls[i];

//            AssetBundleBuild _abb = new AssetBundleBuild();
//            _abb.assetBundleName = BuildCommon.GetName(each, true);
//            _abb.assetNames = new string[1] { each };
//            _list.Add(_abb);
//            storage.InspectorDicKeys[i] = "Dlls/" + BuildCommon.GetName(each, false);
//        }


//        //打记录器
//        GameObject prefab = PrefabUtility.CreatePrefab("Assets/DllMainFest.prefab", dllMainFest);

//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//        AssetBundleBuild _abb1 = new AssetBundleBuild();
//        _abb1.assetBundleName = "DllMainFest.bytes";
//        _abb1.assetNames = new string[1] { "Assets/DllMainFest.prefab" };
//        _list.Add(_abb1);
//        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/Dlls/", _list.ToArray(), options, BuildPlatform);

//        AssetDatabase.DeleteAsset("Assets/DllMainFest.prefab");
//        AssetDatabase.DeleteAsset("Assets/tmp");
//        GameObject.DestroyImmediate(dllMainFest);
//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/单打Textures")]
//    public static void BuildTextures()
//    {
//        List<AssetBundleBuild> _list = new List<AssetBundleBuild>(); //
//        string resourcePath = Application.dataPath + "/Resources/Textures/";
//        string[] files = Directory.GetFiles(resourcePath, "*.*", SearchOption.AllDirectories);
//        List<string> _texturesPathList = new List<string>();
//        int _length = files.Length;

//        for (int i = 0; i < _length; ++i)
//        {
//            string _path = files[i];

//            if (!_path.Contains(".meta") && !_path.Contains(".mat"))
//            {
//                _path = _path.Replace(Application.dataPath, "Assets");
//                _path = _path.Replace("\\", "/");
//                _texturesPathList.Add(_path);
//            }

//        }


//        for (int i = 0; i < _texturesPathList.Count; ++i)
//        {
//            string _path = _texturesPathList[i];
//            AssetImporter _importer = AssetImporter.GetAtPath(_path);
//            if (null != _importer && string.Empty != _importer.assetBundleName)
//            {
//                AssetBundleBuild _abb = new AssetBundleBuild();
//                _abb.assetBundleName = _importer.assetBundleName;
//                _abb.assetNames = new string[1] { _path };

//                _list.Add(_abb);
//            }
//        }


//        BuildCommon.CheckFolder("Assets/StreamingAssets/StreamingResources");
//        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/StreamingResources/", _list.ToArray(), options, BuildPlatform);

//        if (EditorUtility.DisplayDialog("提示", "单打Textures完成！", "确定", "取消"))
//        {

//        }
//    }


//    [MenuItem("Youkia/AssetBundle打包管理/工具/单打AudioClip")]
//    public static void BuildAudioClip()
//    {
//        List<AssetBundleBuild> _list = new List<AssetBundleBuild>();
//        string[] _allAudio = AssetDatabase.FindAssets("t:AudioClip");
//        for (int i = 0; i < _allAudio.Length; ++i)
//        {
//            string _path = AssetDatabase.GUIDToAssetPath(_allAudio[i]);
//            AssetImporter _importer = AssetImporter.GetAtPath(_path);
//            AssetBundleBuild _abb = new AssetBundleBuild();
//            _abb.assetBundleName = _importer.assetBundleName;
//            _abb.assetNames = new string[1] { _path };

//            _list.Add(_abb);

//        }


//        BuildCommon.CheckFolder("Assets/StreamingAssets/StreamingResources");
//        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/StreamingResources/", _list.ToArray(), options, BuildPlatform);

//        if (EditorUtility.DisplayDialog("提示", "单打AudioClip完成！", "确定", "取消"))
//        {

//        }
//    }



//    [MenuItem("Youkia/AssetBundle打包管理/工具/单打Font")]
//    public static void BuildFonts()
//    {
//        List<AssetBundleBuild> _list = new List<AssetBundleBuild>();
//        string[] _allFont = AssetDatabase.FindAssets("t:Font");
//        for (int i = 0; i < _allFont.Length; ++i)
//        {
//            string _path = AssetDatabase.GUIDToAssetPath(_allFont[i]);
//            AssetImporter _importer = AssetImporter.GetAtPath(_path);
//            AssetBundleBuild _abb = new AssetBundleBuild();
//            _abb.assetBundleName = _importer.assetBundleName;
//            _abb.assetNames = new string[1] { _path };

//            _list.Add(_abb);

//        }


//        BuildCommon.CheckFolder("Assets/StreamingAssets/StreamingResources");
//        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/StreamingResources/", _list.ToArray(), options, BuildPlatform);

//        if (EditorUtility.DisplayDialog("提示", "单打Font完成！", "确定", "取消"))
//        {

//        }
//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/单打Shader")]
//    public static void BuildShaders()
//    {

//        List<string> _allShaderList = new List<string>();
//        string[] _allShader = AssetDatabase.FindAssets("t:Shader");
//        for (int i = 0; i < _allShader.Length; ++i)
//        {
//            string _path = AssetDatabase.GUIDToAssetPath(_allShader[i]);
//            _allShaderList.Add(_path);
//        }
//        AssetBundleBuild _AssetBundleBuild = new AssetBundleBuild();
//        _AssetBundleBuild.assetBundleName = "AllShader.bytes";
//        _AssetBundleBuild.assetNames = _allShaderList.ToArray();

//        BuildCommon.CheckFolder("Assets/StreamingAssets/StreamingResources");
//        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/StreamingResources/", new AssetBundleBuild[1] { _AssetBundleBuild }, options, BuildPlatform);

//        if (EditorUtility.DisplayDialog("提示", "单打Shader完成！", "确定", "取消"))
//        {

//        }
//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/单打配置文件Config")]
//    public static void BuildConfigs()
//    {
//        string resourcePath = Application.dataPath + "/Resources/ConfigByte/";



//        string[] files = Directory.GetFiles(resourcePath, "*.*", SearchOption.AllDirectories);
//        List<string> _configPathList = new List<string>();
//        int _length = files.Length;

//        for (int i = 0; i < _length; ++i)
//        {
//            string _path = files[i];

//            if (!_path.Contains(".meta"))
//            {
//                _path = _path.Replace(Application.dataPath, "Assets");
//                _path = _path.Replace("\\", "/");
//                _configPathList.Add(_path);
//            }

//        }


//        List<AssetBundleBuild> _list = new List<AssetBundleBuild>();
//        for (int i = 0; i < _configPathList.Count; i++)
//        {

//            string each = _configPathList[i];

//            AssetInfo unit = BuildCommon.CreateInfo(each, false);
//            BuildCommon.CheckFolder(unit.SaveDirectories);

//            AssetBundleBuild _abb = new AssetBundleBuild();
//            _abb.assetBundleName = unit.Directories + unit.Guid + ".bytes";

//            _abb.assetNames = new string[1] { unit.longPath };
//            _list.Add(_abb);

//        }

//        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/StreamingResources/", _list.ToArray(), options, BuildPlatform);

//        if (EditorUtility.DisplayDialog("提示", "单打配置完成！", "确定", "取消"))
//        {

//        }
//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/单打Dlls")]
//    public static void OnlyBuildDlls()
//    {
//        if (!GetAllDlls(false))
//        {
//            dllErrorAlert();
//            return;
//        }
//        BuildDlls();
//        GetAllScripts(false);
//        AssetDatabase.Refresh();
//        //打代码
//        BuildScripts();
//        AssetDatabase.Refresh();
//        Caching.CleanCache();
//        Clear();

//        if (EditorUtility.DisplayDialog("提示", "单打Dlls完成！", "确定", "取消"))
//        {

//        }
//    }

//    private static void dllErrorAlert()
//    {
//        //"UnityEngine.dll") || realFile.Contains("UnityEngine.UI.dll") || realFile.Contains("UnityEditor.dll") || realFile.Contains("UnityEditor.UI.dll"))
//        EditorUtility.DisplayDialog("重要信息",
//            "项目Plugins > DLL中包含UnityEngine.dll/UnityEngine.UI.dll/UnityEditor.dll/UnityEditor.UI.dll其中一个或多个，这会导致打包数据出错。请删除上述文件后，重启Unity后再做打包操作。",
//            "OK");
//    }

//    static Dictionary<string, float> SizeDic;

//    [MenuItem("Youkia/AssetBundle打包管理/工具/计算大小")]
//    public static void GetSize()
//    {
//        Object c = Selection.activeObject;
//        Debug.Log(getSize(c));

//    }

//    private static int getSize(Object obj)
//    {
//        if (SizeDic == null)
//        {
//            SizeDic = new Dictionary<string, float>();
//            SizeDic.Add(TextureFormat.RGBA32.ToString(), 0.0078f);
//            SizeDic.Add(TextureFormat.ARGB32.ToString(), 0.0078f);

//            SizeDic.Add(TextureFormat.RGBA4444.ToString(), 0.004f);
//            SizeDic.Add(TextureFormat.ARGB4444.ToString(), 0.004f);

//            SizeDic.Add(TextureFormat.RGB24.ToString(), 0.0029f);
//            SizeDic.Add(TextureFormat.ETC_RGB4.ToString(), 0.001f);

//            SizeDic.Add(TextureFormat.PVRTC_RGBA2.ToString(), 0.001f);
//            SizeDic.Add(TextureFormat.PVRTC_RGB2.ToString(), 0.0008f);
//            SizeDic.Add(TextureFormat.PVRTC_RGBA4.ToString(), 0.002f);
//            SizeDic.Add(TextureFormat.PVRTC_RGB4.ToString(), 0.0016f);

//            SizeDic.Add("Mesh", 0.12f);
//            SizeDic.Add("Anim", 4f);
//        }


//        float size = 10f;
//        if (obj is TextAsset)
//        {
//            size = (obj as TextAsset).bytes.Length / 1024f;
//        }
//        else if (obj is Texture2D)
//        {
//            Texture2D tex = obj as Texture2D;
//            int pixCount = tex.width * tex.height;

//            if (SizeDic.ContainsKey(tex.format.ToString()))
//            {
//                size = SizeDic[tex.format.ToString()] * pixCount;
//            }
//        }
//        if (obj is Font)
//        {
//            size = 5120;
//        }
//        else if (obj is GameObject || obj is Mesh)
//        {
//            float meshSize = 0;
//            float animSize = 0;

//            if (obj is Mesh)
//            {
//                meshSize += SizeDic["Mesh"] * (obj as Mesh).vertexCount;
//            }
//            else
//            {
//                GameObject gb = obj as GameObject;
//                MeshFilter[] mf = gb.GetComponentsInChildren<MeshFilter>(true);
//                for (int i = 0; i < mf.Length; i++)
//                {
//                    if (mf[i] != null && mf[i].sharedMesh != null)
//                    {
//                        meshSize += SizeDic["Mesh"] * mf[i].sharedMesh.vertexCount;
//                    }
//                }

//                SkinnedMeshRenderer[] smrs = gb.GetComponentsInChildren<SkinnedMeshRenderer>(true);
//                for (int i = 0; i < smrs.Length; i++)
//                {
//                    if (smrs[i] != null && smrs[i].sharedMesh != null)
//                    {
//                        meshSize += SizeDic["Mesh"] * smrs[i].sharedMesh.vertexCount;
//                    }
//                }

//                Animation[] anims = gb.GetComponentsInChildren<Animation>(true);
//                for (int i = 0; i < anims.Length; i++)
//                {
//                    if (anims[i] != null && anims[i].clip != null)
//                    {
//                        animSize += SizeDic["Anim"] * anims[i].clip.length * anims[i].clip.frameRate;
//                    }
//                }

//            }



//            size = meshSize + animSize;
//        }


//        return (int)size;
//    }
//    private static UnityEngine.Object _tempObj;
//    private static Dictionary<string, Object> allLoadObjs = new Dictionary<string, Object>();


//    /// <summary>
//    /// 计算交叉依赖关系
//    /// </summary>
//    /// <param name="allAssetInfos"></param>
//    private static void caculateRelationship(Dictionary<string, AssetInfo> allAssetInfos)
//    {
//        if (null == allAssetInfos)
//        {
//            return;
//        }

//        AssetInfo _info = null;

//        List<AssetInfo> _list = allAssetInfos.Values.ToList();
//        for (int i = 0; i < _list.Count; ++i)
//        {
//            for (int j = 0; j < _list[i].AllDownDependencies_without_reserve.Count; ++j)
//            {
//                string _path = _list[i].AllDownDependencies_without_reserve[j];
//                string _suffix = BuildCommon.getFileSuffix(_path);
//                string _key = _path.Replace("Assets/Resources/", "").Replace("." + _suffix, "");

//                if (allAssetInfos.TryGetValue(_key, out _info))
//                {
//                    if (!_info.AllUpDependencies.Contains(_key)) { _info.AllUpDependencies.Add(_key); }

//                }
//            }


//        }


//    }

//    public static void BuildResource(HashSet<string> selection, bool buildManiFestOnly = false)
//    {
//        BuildCommon.ClearAssetBundleBuildList();
//        AssetInfo asInfo;
//        //caculateRelationship(allAssetInfos);

//        //打代码
//        BuildScripts();
//        AssetDatabase.Refresh();



//        if (isBuildAllMaterial)
//        {
//            List<AssetInfo> list = allAssetInfos.Values.ToList();

//            for (int i = 0; i < list.Count; i++)
//            {
//                _tempObj = null;
//                if (list[i].Suffix == "shader" || list[i].Suffix == "mat" || list[i].Suffix.ToLower() == "fbx")
//                {
//                    _tempObj = AssetDatabase.LoadAssetAtPath(list[i].longPath, typeof(Object));

//                    if (list[i].Suffix == "shader")
//                    {
//                        allShaderList.Add(_tempObj as Shader);
//                    }
//                    else if (list[i].Suffix == "mat")
//                    {
//                        allMaterialList.Add(_tempObj as Material);
//                    }
//                    else if (list[i].Suffix.ToLower() == "fbx")
//                    {
//                        allFBXList.Add(_tempObj as GameObject);
//                    }
//                }

//            }

//            list.Clear();

//        }


//        //资源最大等级
//        int maxLevel = allLevelAssets.Count;
//        if (maxLevel == 0)
//            return;

//        //从最低等级开始打包
//        for (int level = 1; level <= maxLevel; level++)
//        {
//            //获取不同等级的aaset
//            if (!allLevelAssets.ContainsKey(level))
//                continue;

//            Dictionary<string, AssetInfo> levelAssets = allLevelAssets[level];

//            AssetInfo[] infos = levelAssets.Values.ToArray();

//            //遍历该等级的所有asset打包
//            for (int i = 0; i < infos.Length; i++)
//            {
//                asInfo = infos[i];
//                if (asInfo.Suffix.ToLower() == "cs" || asInfo.IsBuildEachPrefab)
//                    continue;

//                //根据路径获取asset资源
//                _tempObj = AssetDatabase.LoadAssetAtPath(asInfo.longPath, typeof(Object));
//                if (_tempObj == null)
//                {
//                    Debug.Log(asInfo.longPath + "损坏了!");
//                    return;
//                }

//                if (asInfo.Suffix.ToLower() == "fbx")
//                {
//                    asInfo.ResType = typeof(Mesh).Name;
//                }
//                else
//                {
//                    asInfo.ResType = _tempObj.GetType().Name;
//                }

//                if (asInfo.Suffix != "prefab" && asInfo.Suffix != "mat")
//                    asInfo.Size = getSize(_tempObj);

//                if (selection != null)
//                {
//                    if (selection.Contains(asInfo.longPath))
//                    {
//                        buildObj(_tempObj, asInfo);
//                    }
//                }
//                else
//                {
//                    buildObj(_tempObj, asInfo);
//                }

//                _tempObj = null;
//            }
//        }

//        //有单独部分的先打
//        if (SingleAssetInfos != null && SingleAssetInfos.Count > 0)
//        {
//            for (int i = 0; i < SingleAssetInfos.Count; i++)
//            {

//                _tempObj = null;

//                _tempObj = AssetDatabase.LoadAssetAtPath(SingleAssetInfos[i].longPath, typeof(Object));
//                buildObj(_tempObj, SingleAssetInfos[i]);
//            }
//        }


//        //allShaderList.Clear();
//        //string[] _allShader = AssetDatabase.FindAssets("t:Shader");
//        //for (int i = 0; i < _allShader.Length; ++i)
//        //{
//        //    string _path = AssetDatabase.GUIDToAssetPath(_allShader[i]);
//        //    Object _obj = AssetDatabase.LoadMainAssetAtPath(_path);
//        //    if (null != _obj)
//        //    {
//        //        allShaderList.Add(_obj as Shader);
//        //    }
//        //}


//        BuildCommon.CollecateAssetBundle(null, allShaderList.ToArray(), "", "AllShader.bytes");
//        BuildCommon.CollecateAssetBundle(null, allMaterialList.ToArray(), "", "AllMaterial.bytes");

//        BuildMainFest();
//        if (!buildManiFestOnly)
//        {
//            for (int i = 0; i < allFBXList.Count; ++i)
//            {
//                SetObjRecursively_1(allFBXList[i] as GameObject, null);
//                SetObjRecursively_2(allFBXList[i] as GameObject);
//                SetObjRecursively_3(allFBXList[i] as GameObject);
//            }

//            BuildCommon.FinalBuildAssetBundles("Assets/StreamingAssets/StreamingResources/", options, BuildPlatform);
//        }


//        AssetDatabase.Refresh();
//    }

//    static void SetObjRecursively_1(GameObject rootObj, Material mat)
//    {
//        if (null == rootObj)
//        {
//            return;
//        }
//        //去除默认材质球
//        Renderer[] _array = rootObj.GetComponentsInChildren<Renderer>(true);
//        if (null != _array && _array.Length > 0)
//        {
//            for (int i = 0; i < _array.Length; ++i)
//            {
//                Renderer _renderer = _array[i];

//                if (null != _renderer.sharedMaterial)
//                {
//                    if (_renderer.sharedMaterial.name == "Default-Material")
//                    {
//                        _renderer.sharedMaterial = null;
//                        Debug.Log(string.Format("单独替换 {0} 的Material成功！", rootObj.name));
//                    }
//                }

//                if (null != _renderer.sharedMaterials && _renderer.sharedMaterials.Length > 0)
//                {

//                    for (int j = 0; j < _renderer.sharedMaterials.Length; ++j)
//                    {
//                        Material _mat = _renderer.sharedMaterials[j];
//                        if (null != _mat && _mat.name == "Default-Material")
//                        {
//                            _renderer.material = mat;
//                            _renderer.sharedMaterials[j] = mat;

//                            Debug.Log(string.Format("替换 {0} 的Material成功！", rootObj.name));
//                        }
//                    }
//                }

//            }
//        }

//        //去除Animator


//        //去除Animation


//    }
//    static void SetObjRecursively_2(GameObject rootObj)
//    {
//        if (null == rootObj)
//        {
//            return;
//        }
//        Animation[] _array = rootObj.GetComponentsInChildren<Animation>(true);
//        if (null != _array && _array.Length > 0)
//        {
//            for (int i = 0; i < _array.Length; ++i)
//            {
//                Animation _animation = _array[i];
//                GameObject.DestroyImmediate(_animation, true);
//                Debug.Log(string.Format("删除 {0} 的Animation成功！", rootObj.name));
//            }
//        }

//    }
//    static void SetObjRecursively_3(GameObject rootObj)
//    {
//        if (null == rootObj)
//        {
//            return;
//        }
//        Animator[] _array = rootObj.GetComponentsInChildren<Animator>(true);
//        if (null != _array && _array.Length > 0)
//        {
//            for (int i = 0; i < _array.Length; ++i)
//            {
//                Animator _animator = _array[i];
//                GameObject.DestroyImmediate(_animator, true);
//                Debug.Log(string.Format("删除 {0} 的Animator成功！", rootObj.name));
//            }
//        }

//    }


//    private static StringBuilder _stringBuilder = new StringBuilder();



//    static bool buildObj(Object asset, AssetInfo asInfo)
//    {
//        // Debug.Log(asInfo.longPath + "   size:" + asInfo.Size);

//        if (null == asset)
//        {
//            BuildCommon.LogError("load " + asInfo.longPath + " failed!!!");
//            return false;
//        }

//        if (asset is Material)
//        {
//            return false;
//        }

//        //生成打包保存路径
//        string savePath = asInfo.SavePath;
//        savePath = savePath + assetbundleFileSuffix;

//        if (IsPathInEditorOnlyPathList(asInfo.longPath))
//            return false;

//        BuildCommon.CheckFolder(BuildCommon.getFolder(savePath));

//        if (asInfo.IsBuildEachPrefab)
//        {
//            //单打
//            if (asInfo.IsFolder)
//            {
//                //打文件夹内的预制体和anim
//                string[] prefabObjects = AssetDatabase.FindAssets("t:Prefab", new string[] { asInfo.longPath });
//                string[] animObjects = AssetDatabase.FindAssets("t:AnimationClip", new string[] { asInfo.longPath });

//                if (prefabObjects != null && prefabObjects.Length > 0)
//                {
//                    List<Object> objs = new List<Object>();

//                    for (int i = 0; i < prefabObjects.Length; i++)
//                    {
//                        string _path = AssetDatabase.GUIDToAssetPath(prefabObjects[i]);
//                        if (IsPathInEditorOnlyPathList(_path))
//                            continue;

//                        List<string> _dependentList = AssetDatabase.GetDependencies(_path).ToList();
//                        List<string> _list = null == asInfo.AllDependencies ? new List<string>() : asInfo.AllDependencies.ToList();
//                        for (int j = 0; j < _dependentList.Count; j++)
//                        {
//                            Object _obj = AssetDatabase.LoadMainAssetAtPath(_dependentList[j]);
//                            if (_obj is Material)
//                            {
//                                //allMaterialList.Remove(_obj);

//                                continue;
//                            }
//                            if (!objs.Contains(_obj))
//                            {
//                                objs.Add(_obj);
//                            }

//                            //if (!_list.Contains(_dependentList[j]) && _path != _dependentList[j]
//                            //            && !_dependentList[j].EndsWith(".shader") && !_dependentList[j].EndsWith(".dll"))
//                            //{
//                            //    _list.Add(_dependentList[j]);
//                            //}

//                        }
//                        //asInfo.AllDependencies = _list.ToArray();

//                    }

//                    for (int i = 0; i < animObjects.Length; i++)
//                    {
//                        string _path = AssetDatabase.GUIDToAssetPath(animObjects[i]);
//                        if (IsPathInEditorOnlyPathList(_path))
//                            continue;

//                        objs.Add(AssetDatabase.LoadAssetAtPath(_path, typeof(AnimationClip)));

//                    }



//                    BuildCommon.CollecateAssetBundle(objs[0], objs.ToArray(), asInfo.Directories, asInfo.Guid + assetbundleFileSuffix);
//                }

//            }
//            else
//            {
//                if (asInfo.Suffix.ToLower() == "prefab")
//                {
//                    string _path = asInfo.longPath;
//                    List<Object> objs = new List<Object>();

//                    if (!IsPathInEditorOnlyPathList(_path))
//                    {

//                        List<string> _dependentList = AssetDatabase.GetDependencies(_path).ToList();
//                        List<string> _list = null == asInfo.AllDependencies ? new List<string>() : asInfo.AllDependencies.ToList();
//                        for (int j = 0; j < _dependentList.Count; j++)
//                        {
//                            Object _obj = AssetDatabase.LoadMainAssetAtPath(_dependentList[j]);
//                            if (_obj is Material)
//                            {
//                                //allMaterialList.Remove(_obj);

//                                continue;
//                            }
//                            if (!objs.Contains(_obj))
//                            {
//                                objs.Add(_obj);
//                            }
//                            //if (!_list.Contains(_dependentList[j]) && _path != _dependentList[j]
//                            //    && !_dependentList[j].EndsWith(".shader") && !_dependentList[j].EndsWith(".dll"))
//                            //{
//                            //    _list.Add(_dependentList[j]);
//                            //}

//                        }
//                        //asInfo.AllDependencies = _list.ToArray();

//                        BuildCommon.CollecateAssetBundle(objs[0], objs.ToArray(), asInfo.Directories, asInfo.Guid + assetbundleFileSuffix);
//                    }
//                }

//            }

//        }
//        else if ((asset is GameObject || asset is Material) && asInfo.Suffix.ToLower() != "fbx")
//        {
//            //  Object c = CreateResLink(asset);
//            //   string cp = AssetDatabase.GetAssetPath(c);

//            BuildCommon.CollecateAssetBundle(asset, null, asInfo.Directories, asInfo.Guid + assetbundleFileSuffix);
//            //  AssetDatabase.DeleteAsset(cp);
//        }
//        else if (asset is GameObject && asInfo.Suffix.ToLower() == "fbx")
//        {
//            Object[] fbxAll = AssetDatabase.LoadAllAssetsAtPath(asInfo.longPath);
//            List<Object> list = new List<Object>();
//            for (int i = 0; i < fbxAll.Length; i++)
//            {
//                if (fbxAll[i] is Mesh || fbxAll[i] is AnimationClip || fbxAll[i] is Avatar)
//                    list.Add(fbxAll[i]);
//            }
//            fbxAll = list.ToArray();

//            BuildCommon.CollecateAssetBundle(fbxAll[0], fbxAll, asInfo.Directories, asInfo.Guid + assetbundleFileSuffix);


//        }
//        else
//        {

//            BuildCommon.CollecateAssetBundle(asset, null, asInfo.Directories, asInfo.Guid + assetbundleFileSuffix);
//        }

//        if (isCreateBuildAssetInfo)
//        {
//            _stringBuilder.Length = 0;
//            string infoPath = savePath.Replace("Assets/StreamingAssets/", "");
//            int fileSize = 0;
//            string fileMD5 = BuildCommon.GetFileMD5(savePath, ref fileSize);
//            string pathMD5 = GetStringMD5(infoPath);

//            _stringBuilder.Append(pathMD5 + "|");
//            _stringBuilder.Append(fileMD5 + "|");
//            _stringBuilder.Append(infoPath + "|");
//            _stringBuilder.Append(fileSize.ToString());

//            bulidAssetInfoList.Add(_stringBuilder.ToString());
//        }

//        return true;

//    }


//    public static string GetStringMD5(string sDataIn)
//    {
//        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
//        byte[] bytValue, bytHash;
//        bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
//        bytHash = md5.ComputeHash(bytValue);
//        md5.Clear();
//        string sTemp = "";
//        for (int i = 0; i < bytHash.Length; i++)
//        {
//            sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
//        }
//        return sTemp.ToLower();
//    }

//    [MenuItem("Youkia/AssetBundle打包管理/工具/生成热更MD5列表")]
//    public static void CreateMoveList()
//    {
//        string AssetsPath = EditorUtility.SaveFolderPanel("Select Floder To Bundle", "GameRes", "GameRes");

//        CreateMoveList(AssetsPath);
//    }

//    public static void CreateMoveList(string AssetsPath)
//    {
//        string path = AssetsPath + "/list.settings";
//        if (File.Exists(path))
//            File.Delete(path);
//        if (string.IsNullOrEmpty(AssetsPath))
//            return;

//        string[] files = Directory.GetFiles(AssetsPath, "*.*", SearchOption.AllDirectories);

//        FileStream fs = File.OpenWrite(path);
//        int subLen = new DirectoryInfo(AssetsPath).FullName.Length;

//        for (int i = 0; i < files.Length; i++)
//        {
//            string filePath = files[i];
//            string fPath = filePath.Replace("\\", "/");

//            string suffix = "";
//            if (fPath.LastIndexOf('.') != -1)
//                suffix = fPath.Substring(fPath.LastIndexOf('.')).ToLower();

//            if (string.IsNullOrEmpty(suffix))
//            {
//                Debug.LogError("*** BuildAssetBundle.CreateBuildAssetInfoFromExists Error :: some file have not suffix = " + fPath);
//                continue;
//            }

//            if (fPath.Contains("GameRes/list.settings"))
//                continue;
//            if (suffix == ".meta")
//                continue;

//            _stringBuilder.Length = 0;
//            string infoPath = fPath.Replace(AssetsPath + "/", "");
//            int fileSize = 0;
//            string fileMD5 = BuildCommon.GetFileMD5(fPath, ref fileSize);
//            string pathMD5 = GetStringMD5(infoPath);
//            _stringBuilder.Append(pathMD5 + "|");
//            _stringBuilder.Append(fileMD5 + "|");
//            _stringBuilder.Append(infoPath + "|");
//            _stringBuilder.Append(fileSize.ToString());
//            _stringBuilder.Append("\n");
//            byte[] data = System.Text.Encoding.UTF8.GetBytes(_stringBuilder.ToString());
//            fs.Write(data, 0, data.Length);
//        }

//        fs.Flush();
//        fs.Close();
//    }
//}