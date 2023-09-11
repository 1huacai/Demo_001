using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Debug = UnityEngine.Debug;
using UnityEngine.Rendering;
using ResourceLoad;
/**
* SLG一键出包
* 
* XXX为平台名
* SDK : Assets/Plugins/..Android_{XXX} -> Assets/Plugins/Android
* 平台: Assets/Plugins/platform.txt
* 
*/
public class ExportPackage : EditorWindow
{
    /* 每页多少个 */
    private const int ITEM_COUNT = 25;
    /* 所有平台配置信息 */
    private List<Config> list = new List<Config>();
    /* 翻页当前页 */
    private int pageIndex = 0;

    private string version;
   [MenuItem("AssetBundleTool/出apk包")]
    private static void ShowWindow()
    {
        SetApplicationConfig();
        //DelUselessDll();
        DeletManifestFile();

        ExportPackage win = GetWindow<ExportPackage>(false, "一键打包", true);
        win.minSize = new Vector2(650, 300);
        win.LoadPlatformConfigs();
        win.Show();

    }

    private static void DelUselessDll()
    {
        string _path = Application.dataPath + "/Resources/Plugins/";
        DirectoryInfo direction = new DirectoryInfo(_path);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        Debug.Log(files.Length);

        List<FileInfo> _list = files.ToList();
        files = _list.OrderBy(v => v.FullName.Length).ToArray();

        for (int i = files.Length - 1; i >= 0; i--)
        {
            if (files[i].FullName.Contains("FrameWorkScriptableObject") || files[i].FullName.Contains("PostProcessing"))
            {
                continue;
            }
            FileUtil.DeleteFileOrDirectory(files[i].FullName);
        }
        AssetDatabase.Refresh();
    }
    private string m_sdkPach;
    private string m_platformpath;
    private void LoadPlatformConfigs()
    {
        list.Clear();
        if (!File.Exists(Application.dataPath + "/Plugins/SDKInfo.txt"))
        {
            SavePlatformConfigs();
            EditorUtility.DisplayDialog("", "请在\"Plugins/SDKInfo.txt\"配置SDK路劲信息!", "OK");
            return;
        }
        string[] _data = File.ReadAllLines(Application.dataPath + "/Plugins/SDKInfo.txt");
        m_platformpath = _data[0]+ "/platform.txt";
        //m_sdkPach = _data[1];
        if (!File.Exists(m_platformpath))
        {
            SavePlatformConfigs();
            EditorUtility.DisplayDialog("", "请在"+ m_platformpath + "配置平台信息!", "OK");
            return;
        }

        List<Config> list0 = new List<Config>();
        List<Config> list1 = new List<Config>();

        string[] lines = File.ReadAllLines(m_platformpath);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("#"))
            {
                Config _config = new Config(line);
                list0.Add(_config);
                //if (_config.platformName.Contains("xiaoxiaole"))
                //{
                //    list0.Add(_config);
                //}
                //else
                //{
                //    list1.Add(_config);
                //}
            }
        }

        list0 = list0.OrderBy(v => v.platformName).ToList();
        //list1 = list1.OrderBy(v => v.platformName).ToList();
        list1=list1.OrderBy(v => v.BuildingModel != "Internal").ThenBy(v=>v.IL2CPP).ToList();
        list.AddRange(list0);
        list.AddRange(list1);

        version = list[0].bundleVersion;
    }

    private void SavePlatformConfigs()
    {
        string[] lines = new string[list.Count];
        for (int i = 0; i < lines.Length; i++)
            lines[i] = list[i].ToFileString();
        File.WriteAllLines(m_platformpath, lines, Encoding.UTF8);
    }
    private void ModifyPlatformconfigs(string lable)
    {
        string[] lines = new string[list.Count];
        for (int i = 0; i < lines.Length; i++)
        {
            list[i].bundleVersion = lable;
            lines[i] = list[i].ToFileString();
        }

        File.WriteAllLines(m_platformpath, lines, Encoding.UTF8);
    }

    private static void DeletManifestFile()
    {
        if (!Directory.Exists(Application.dataPath + "/StreamingAssets"))
            return;
        string[] files = Directory.GetFiles(Application.dataPath + "/StreamingAssets", "*.manifest", SearchOption.AllDirectories).ToArray();
        for (int i = 0; i < files.Length; ++i)
        {
            File.Delete(files[i]);
        }

        string _path = Application.dataPath + "/StreamingAssets/StreamingResources/StreamingResources";
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
        AssetDatabase.Refresh();
    }
    private  void InintEventment()
    {
        if (GUILayout.Button("删除dll", GUILayout.Height(34), GUILayout.Width(100)))
        {
            if (EditorUtility.DisplayDialog("提示", "你确定要删除dll吗？", "确定", "取消"))
            {
                EditorApplication.delayCall += () => {
                    DelUselessDll();
                };
            }
        }
        //if (GUILayout.Button("删除Manifest", GUILayout.Height(34), GUILayout.Width(100)))
        //{
        //    if (EditorUtility.DisplayDialog("提示", "你确定要删除Manifest吗？", "确定", "取消"))
        //    {
        //        EditorApplication.delayCall += () => {
        //            DeletManifestFile();
        //        };
        //    }
        //}
        version = EditorGUILayout.TextField(version, GUILayout.Height(34), GUILayout.Width(100));
        if (GUILayout.Button("修改版本信息", GUILayout.Height(34), GUILayout.Width(100)))
        {
            if (EditorUtility.DisplayDialog("提示", "你确定要修改版本信息吗？", "确定", "取消"))
            {
                EditorApplication.delayCall += () =>
            {
                ModifyPlatformconfigs(version);
            };
            }
        }

    }
    private void OnGUI()
    {
        GUISkin curSkin = GUI.skin;
        GUIStyle style;
        style = new GUIStyle(curSkin.label);
        style.alignment = TextAnchor.MiddleLeft;
        EditorGUILayout.BeginHorizontal();
        InintEventment();
        EditorGUILayout.EndHorizontal();
        #region -标题
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("id", style, GUILayout.Height(34), GUILayout.Width(20));
        GUILayout.Label("平台名称", style, GUILayout.Height(34), GUILayout.Width(200));
        GUILayout.Label("打包模式", style, GUILayout.Height(34), GUILayout.Width(80));
        GUILayout.Label("游戏名称", style, GUILayout.Height(34), GUILayout.Width(80));
        GUILayout.Label("包名", style, GUILayout.Height(34), GUILayout.Width(200));
        GUILayout.Label("版本号", style, GUILayout.Height(34), GUILayout.Width(50));
        GUILayout.Label("vCode", style, GUILayout.Height(34), GUILayout.Width(50));
        GUILayout.Label("多线程渲染", style, GUILayout.Height(34), GUILayout.Width(100));
        GUILayout.Label("TargetAPILevel", style, GUILayout.Height(34), GUILayout.Width(100));
        GUILayout.Label("MinSDK", style, GUILayout.Height(34), GUILayout.Width(50));
        GUILayout.Label("IL2CPP", style, GUILayout.Height(34), GUILayout.Width(100));

        EditorGUILayout.EndHorizontal();
        #endregion
        int maxPage = Math.Max(1, Mathf.CeilToInt(list.Count / (float)ITEM_COUNT));
        pageIndex = Math.Max(0, Math.Min(pageIndex, maxPage - 1));
        #region 平台列表
        //-平台设置
        int tmpI = pageIndex * ITEM_COUNT;
        int tmp2 = Mathf.Min(list.Count, tmpI + ITEM_COUNT);
        GUILayoutOption _hightOption = GUILayout.Height(18);
        for (int i = tmpI; i < tmp2; i++)
        {
            Config conf = list[i];
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label((i + 1).ToString(), _hightOption, GUILayout.Width(20));
            GUILayout.Label(conf.platformName, _hightOption, GUILayout.Width(200));
            conf.BuildingModel = GUILayout.TextField(conf.BuildingModel, _hightOption, GUILayout.Width(80));
            conf.productName = GUILayout.TextField(conf.productName, _hightOption, GUILayout.Width(80));
            conf.bundleIdentifier = GUILayout.TextField(conf.bundleIdentifier, _hightOption, GUILayout.Width(200));
            conf.bundleVersion = GUILayout.TextField(conf.bundleVersion, _hightOption, GUILayout.Width(50));
            string vcode = GUILayout.TextField(conf.bundleVersionCode.ToString(), _hightOption, GUILayout.Width(50));
            int.TryParse(vcode, out conf.bundleVersionCode);

            bool ismult = GUILayout.Toggle(conf.MTRendering, "多线程渲染", _hightOption, GUILayout.Width(100));
            if (conf.MTRendering != ismult)
            {
                conf.MTRendering = ismult;

            }
            conf.APILevel = GUILayout.TextField(conf.APILevel, _hightOption, GUILayout.Width(80));
            conf.minSdkVersion = GUILayout.TextField(conf.minSdkVersion, _hightOption, GUILayout.Width(80));
            
            bool isSelect1 = GUILayout.Toggle(conf.IL2CPP, "ILL2CPP", _hightOption, GUILayout.Width(50));
            if (conf.IL2CPP != isSelect1)
            {
                conf.IL2CPP = isSelect1;
            }
            bool isSelect = GUILayout.Toggle(conf.buildApk, "打包", _hightOption, GUILayout.Width(50));
            if (conf.buildApk != isSelect)
            {
                conf.buildApk = isSelect;
                if (conf.buildApk && conf.mustSingleExport)
                    EditorUtility.DisplayDialog("", conf.remarks, "OK.");
            }

            
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 翻页操作
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Separator();
        if (GUILayout.Button("上一页", GUILayout.Height(20), GUILayout.Width(50)))
        {
            pageIndex = Mathf.Max(0, --pageIndex);
        }

        style.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label((pageIndex + 1) + "/" + (maxPage), style, GUILayout.Height(20), GUILayout.Width(50));
        if (GUILayout.Button("下一页", GUILayout.Height(20), GUILayout.Width(50)))
        {
            pageIndex = Mathf.Min(maxPage - 1, ++pageIndex);
        }
        //翻页
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 操作按钮
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        //-按钮些
        if (GUILayout.Button("全选", GUILayout.Height(20), GUILayout.Width(100)))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].mustSingleExport)
                {
                    list[i].buildApk = false;
                    continue;
                }
                list[i].buildApk = true;
            }
        }
        //EditorGUILayout.Separator();
        if (GUILayout.Button("反选", GUILayout.Height(20), GUILayout.Width(100)))
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].mustSingleExport)
                {
                    list[i].buildApk = false;
                    continue;
                }
                list[i].buildApk = !list[i].buildApk;
            }
        }
        if (GUILayout.Button("一键设置打包配置", GUILayout.Height(20), GUILayout.Width(100)))
        {
            Config _conf = null;
            int index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].buildApk)
                {
                    index++;
                    _conf = list[i];
                }
            }
            if (index > 1)
            {
                EditorUtility.DisplayDialog("错误", "选中了多个需要设置的渠道", "我去改");
            }
            else if (_conf == null)
            {
                EditorUtility.DisplayDialog("错误", "未选中需要设置的渠道", "我去改");
            }
            else
            {
                if (SetBuildingSetting(_conf) != null)
                {
                    EditorUtility.DisplayDialog("", _conf.platformName + "设置已完成", "好的.");
                }
            }
        }
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        if (GUILayout.Button("保存设置", GUILayout.Height(20), GUILayout.Width(100)))
        {
            SavePlatformConfigs();
            EditorUtility.DisplayDialog("", "done!", "OK");
        }
        //打包完成后执行会抛错误
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        if (GUILayout.Button("开始打包"))
        {

            int total = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].buildApk)
                    total++;
            }
            if (total <= 0)
            {
                EditorUtility.DisplayDialog("", "请勾选择要打包的平台", "好的.");
                return;
            }
            if (EditorUtility.DisplayDialog("", "已经选择 " + total + " 款平台 开始打包?", "我要打包", "我再想想"))
            {
                string saveDir = EditorUtility.OpenFolderPanel("临时目录", Application.dataPath, "");
                if (string.IsNullOrEmpty(saveDir))
                {
                    return;
                }
                string saveDir1 = EditorUtility.OpenFolderPanel("保存5.10目录", Application.dataPath, "");
                if (string.IsNullOrEmpty(saveDir1))
                {
                    return;
                }
                int count = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].buildApk && BuildPlatform(list[i], saveDir,saveDir1))
                    {
                        count++;
                        SavePlatformConfigs();
                        //EditorApplication.delayCall += SavePlatformConfigs;
                    }
                }


                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("", "已完成(" + count + "/" + total + ")", "好的.");
                EditorUtility.OpenWithDefaultApp(saveDir1);

                Close();

            }
        }

        #endregion
    }

    /// <summary>
    /// 设置应用程序配置
    /// </summary>
    private static void SetApplicationConfig()
    {

        QualitySettings.skinWeights = SkinWeights.TwoBones;
        PlayerSettings.runInBackground = true;

    }

    [MenuItem("FUnityExtends/AssetBundle/BuildAPKMD5")]
    public static void buildApkMd5()
    {
        string saveDir = EditorUtility.OpenFolderPanel("保存目录", Application.dataPath, "");
        BuildAPKMD5(saveDir);
    }


    public static void BuildAPKMD5(string dir)
    {

        string[] files = Directory.GetFiles(dir, "*.apk", SearchOption.TopDirectoryOnly).ToArray();

        int _size = 0;
        StringBuilder _sb = new StringBuilder();
        for (int i = 0; i < files.Length; ++i)
        {
            DisplayProgressBar("生成MD5:  ", "处理中 ", i, files.Length);
            string apkName = Path.GetFileName(files[i]);
            string md5 = GetFileMD5(files[i], ref _size);

            _sb.Append(md5);
            _sb.Append("\t");
            _sb.Append(apkName);
            _sb.Append("\r\n");
        }
        EditorUtility.ClearProgressBar();
        byte[] _byteArray = System.Text.Encoding.UTF8.GetBytes(_sb.ToString());
        FileStream _fs = new FileStream(dir + "/APKMD5.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _fs.Write(_byteArray, 0, _byteArray.Length);
        _fs.Flush();
        _fs.Close();
        _fs.Dispose();
        _fs = null;

        EditorUtility.DisplayDialog("", "生成APK MD5 成功", "OK");
    }


    private void OnDestroy()
    {
        //SavePlatformConfigs();
    }

    private void SwitchSplash(string platformName)
    {
        string url = "Assets/Resources/Launcher/splash{0}.jpg";

        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Resources/Launcher/Prefabs/Loading.prefab", typeof(GameObject));
        GameObject prefab2 = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        Texture tex = (Texture)AssetDatabase.LoadAssetAtPath(string.Format(url, ""), typeof(Texture));
        prefab2.transform.Find("Canvas/splash").GetComponent<UnityEngine.UI.RawImage>().texture = tex;

        tex = (Texture)AssetDatabase.LoadAssetAtPath(string.Format(url, "1"), typeof(Texture));
        prefab2.transform.Find("Canvas/splash1").GetComponent<UnityEngine.UI.RawImage>().texture = tex;

        tex = (Texture)AssetDatabase.LoadAssetAtPath(string.Format(url, "2"), typeof(Texture));
        prefab2.transform.Find("Canvas/splash2").GetComponent<UnityEngine.UI.RawImage>().texture = tex;

        PrefabUtility.ReplacePrefab(prefab2, prefab, ReplacePrefabOptions.ConnectToPrefab);
        GameObject.DestroyImmediate(prefab2);
        AssetDatabase.SaveAssets();
    }

    private bool CopySdk(Config conf)
    {
        //-复制SDK
        string sdkSrc = m_sdkPach + conf.platformName;
        string sdkDst = Application.dataPath + "/Plugins/Android";
        FileUtil.DeleteFileOrDirectory(sdkDst);
        if (!System.IO.Directory.Exists(sdkSrc))
        {
            UnityEngine.Debug.LogError("SDK:" + sdkSrc + " 不存在!");
            return false;
        }
        FileUtil.CopyFileOrDirectory(sdkSrc, sdkDst);
        return true;
    }

    private bool SetIcon(Config config)
    {

        string _path = string.Empty;

        _path = Application.dataPath + "/Resources/Icon";
        //DirectoryInfo _info = new DirectoryInfo(_path);
        //FileInfo[] _array = _info.GetFiles();
        //foreach (FileInfo f in _array)
        //{
        //    if (f.FullName.Contains("*.meta"))
        //    {
        //        continue;
        //    }
        //    f.Delete();
        //}


        //_path = m_sdkPach + "/Icon/" + config.platformName;
        //_info = new DirectoryInfo(_path);
        //_array = _info.GetFiles();

        //foreach (FileInfo f in _array)
        //{
        //    f.CopyTo(Application.dataPath + "/Resources/Icon/" + f.Name, true);
        //}
        //AssetDatabase.Refresh();
        //return true;


        TextureImporter _importer = AssetImporter.GetAtPath("Assets/Resources/Icon/ic_launcher.png") as TextureImporter;
        _importer.textureType = TextureImporterType.Default;
        _importer.sRGBTexture = true;
        _importer.alphaIsTransparency = false;
        _importer.mipmapEnabled = false;
        _importer.wrapMode = TextureWrapMode.Clamp;
        _importer.filterMode = FilterMode.Point;

        TextureImporterPlatformSettings _settings = _importer.GetPlatformTextureSettings("Android");
        _settings.overridden = true;
        _settings.textureCompression = TextureImporterCompression.Uncompressed;
        _settings.format = TextureImporterFormat.RGBA32;
        _importer.SetPlatformTextureSettings(_settings);

        AssetDatabase.ImportAsset(_path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Texture2D _ic_launcher = Resources.Load<Texture2D>("Icon/ic_launcher");
        Texture2D[] _TextureArray = new Texture2D[6];
        _TextureArray[0] = _ic_launcher;
        _TextureArray[1] = _ic_launcher;
        _TextureArray[2] = _ic_launcher;
        _TextureArray[3] = _ic_launcher;
        _TextureArray[4] = _ic_launcher;
        _TextureArray[5] = _ic_launcher;
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, _TextureArray, IconKind.Any);



        return true;
    }
    /// <summary>
    /// 设置打包参数
    /// </summary>
    /// <param name="conf"></param>
    /// <returns></returns>
    private List<string> SetBuildingSetting(Config conf)
    {
        //if (!CopySdk(conf))
        //{
        //    return null;
        //}



        if (!SetIcon(conf))
        {
            return null;
        }


        //unity闪屏
        string _unitySplashPath = m_sdkPach + conf.splash + ".jpg";
        if (File.Exists(_unitySplashPath))
        {
            FileUtil.ReplaceFile(_unitySplashPath, Application.dataPath + "/splash.jpg");
        }

        //第一张程序闪屏（健康公告）

        //第二张程序闪屏
        //string _splashPath1 = Application.dataPath.Replace("Assets", "") + "/SDK/" + conf.splash + ".jpg";
        //if (File.Exists(_unitySplashPath))
        //{
        //    FileUtil.ReplaceFile(_splashPath1, Application.dataPath + "/Resources/Launcher/splash1.jpg");
        //}

        //第三张程序闪屏（光荣图片）

        //闪屏
        SwitchSplash(conf.platformName);
        //设置打包参数
        SettingConfig _settingConfig = (SettingConfig)AssetDatabase.LoadAssetAtPath("Assets/Resources/Launcher/Asset/SettingConfig.asset", typeof(ScriptableObject));
        _settingConfig.isSDK = false;
        _settingConfig.isDropRecv = false;
        _settingConfig.isDropSend = false;
        _settingConfig.releaseDate = DateTime.Now.ToString("yyyyMMddHHmmss");
        PlayerSettings.companyName = conf.companyName;
        PlayerSettings.productName = conf.productName;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        //PlayerSettings.MTRendering = conf.MTRendering;//关闭多线程渲染
        PlayerSettings.MTRendering = false;

#if UNITY_5
        PlayerSettings.mobileMTRendering = conf.MTRendering;
#elif UNITY_2017 || UNITY_2018
        //PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, conf.MTRendering);//关闭多线程渲染
        //PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, conf.MTRendering);//关闭多线程渲染
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false);
        PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, false);
#endif

        PlayerSettings.gpuSkinning = false;
        PlayerSettings.graphicsJobs = false;

        conf.bundleVersionCode += 1;//自增1
        PlayerSettings.Android.bundleVersionCode = conf.bundleVersionCode;
        PlayerSettings.Android.splashScreenScale = AndroidSplashScreenScale.ScaleToFill;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;

        if ("22" == conf.APILevel)
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        }
        else if ("26" == conf.APILevel)
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        }
        else
        {
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)int.Parse(conf.APILevel);
        }


        //if ("jingli" == conf.platformName)
        //{
        //    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
        //}
        //else if ("lianxiang" == conf.platformName)
        //{
        //    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        //}
        //else if ("yingyongbao" == conf.platformName)
        //{
        //    PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel16;
        //    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        //}
        //else
        //    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel16;
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)(int.Parse(conf.minSdkVersion));
        //if("sougou"==conf.platformName)
        //    EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        //else
        //    EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
        if (conf.BuildingModel == "Internal")
        {
            //EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
        }
        else if (conf.BuildingModel == "Gradle")
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        if (conf.IL2CPP)
        {
            PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.IL2CPP, BuildTarget.Android);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        }
        else
            PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.Mono2x, BuildTarget.Android);
        //if (!SetKeyStore(conf))
        //{
        //    return null;
        //}
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                scenes.Add(e.path);
        }
        if (scenes.Count <= 0)
        {
            EditorUtility.DisplayDialog("", "请设置打包主场景!", "OK");
            return null;
        }
        return scenes;
    }

    private bool SetKeyStore(Config conf)
    {
        string keystorePath = string.Format(Application.dataPath + "/../keystore/{0}", conf.platformName);
        if (!Directory.Exists(keystorePath))
        {
            keystorePath = string.Format(Application.dataPath + "/../keystore/Default");
            if (!Directory.Exists(keystorePath))
            {
                EditorUtility.DisplayDialog("配置签名错误", "请检查签名文件!\n" + keystorePath, "OK");
                return false;
            }
        }
        DirectoryInfo direction = new DirectoryInfo(keystorePath);
        FileInfo[] keyfiles = direction.GetFiles("*.keystore", SearchOption.TopDirectoryOnly);
        if (keyfiles.Length == 0)
        {
            EditorUtility.DisplayDialog("配置签名错误", "不存在签名文件(*.keystore)!\n" + keystorePath, "OK");
            return false;
        }

        string infoPath = keystorePath + "/info.txt";
        if (!File.Exists(infoPath))
        {
            EditorUtility.DisplayDialog("配置签名错误", "不存在配置文件(info.txt)!\n" + keystorePath, "OK");
            return false;
        }
        string[] lines = File.ReadAllLines(infoPath);
        PlayerSettings.Android.keystoreName = keyfiles[0].FullName.Replace(@"\", "/");
        PlayerSettings.Android.keystorePass = lines[0];
        PlayerSettings.Android.keyaliasName = lines[1];
        PlayerSettings.Android.keyaliasPass = lines[2];
        return true;
    }

    private bool BuildPlatform(Config conf, string saveDir, string saveDir1)
    {
        AssetDatabase.Refresh();
        List<string> scenes = SetBuildingSetting(conf);
        if (scenes == null)
            return false;
        //string sdkDst = Application.dataPath + "/Plugins/Android";
        //string apkName = saveDir + "/xiaoxiaole" + "_" + conf.platformName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        string apkName1 = saveDir + "/xiaoxiaole" + "_" + conf.platformName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".apk";
        string apkName2 = saveDir1 + "/xiaoxiaole" + "_" + conf.platformName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        BuildPipeline.BuildPlayer(scenes.ToArray(), apkName1, BuildTarget.Android, BuildOptions.None);
        Caching.ClearCache();
        AssetDatabase.Refresh();
        //FileUtil.DeleteFileOrDirectory(sdkDst);
        int _size = 0;
        string md5 = GetFileMD5(apkName1, ref _size);
        FileInfo fi = new FileInfo(apkName1);
        fi.MoveTo(apkName2 + "_" + md5 + ".apk");

        AssetDatabase.Refresh();
        return true;

    }


    /**配置文件各类属性**/
    private class Config
    {
        public string platformName;//平台名称
        public string companyName;//公司名称
        public string productName;//游戏名称
        public string bundleIdentifier;//包名
        public string bundleVersion;//版本号
        public int bundleVersionCode;//versionCode保证每次大平台递增

        public string splash;//闪屏
        public string splash1;//闪屏1
        public readonly bool mustSingleExport;//是否必须单独打包
        public readonly string remarks = string.Empty;//提示语
        /// <summary>
        /// 是否采用多线程渲染
        /// </summary>
        public bool MTRendering;

        public string APILevel = "26";

        public string minSdkVersion = "16";

        public bool buildApk;
        public bool ShowSplash = true;
        public bool Show4Splash = true;

        public string BuildingModel = "Internal";
        public bool IL2CPP = false;
        public Config(String param)
        {
            string[] strs = param.Split('#');
            platformName = strs[1];
            companyName = strs[2];
            productName = strs[3];
            bundleIdentifier = strs[4];
            bundleVersion = strs[5];
            bundleVersionCode = int.Parse(strs[6]);
            splash = strs[7];
            splash1 = strs[8];
            string[] tipInfo = strs[9].Split('+');
            mustSingleExport = tipInfo[0] == "True";
            if (tipInfo.Length > 1)
                remarks = tipInfo[1];
            MTRendering = strs[10] == "True";
            APILevel = strs[11];
            ShowSplash = strs[12] == "True";
            Show4Splash = strs[13] == "True";
            BuildingModel = strs[14];
            IL2CPP = strs[15] == "True";
            minSdkVersion = strs[16];
        }

        public String ToFileString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('#').Append(platformName);
            sb.Append('#').Append(companyName);
            sb.Append('#').Append(productName);
            sb.Append('#').Append(bundleIdentifier);
            sb.Append('#').Append(bundleVersion);
            sb.Append('#').Append(bundleVersionCode);
            sb.Append('#').Append(splash);
            sb.Append('#').Append(splash1);
            sb.Append('#').Append(mustSingleExport.ToString());
            if (!string.IsNullOrEmpty(remarks))
                sb.Append('+').Append(remarks.ToString());
            sb.Append('#').Append(MTRendering.ToString());
            sb.Append('#').Append(APILevel.ToString());
            sb.Append('#').Append(ShowSplash.ToString());
            sb.Append('#').Append(Show4Splash.ToString());
            sb.Append('#').Append(BuildingModel.ToString());
            sb.Append('#').Append(IL2CPP.ToString());
            sb.Append('#').Append(minSdkVersion.ToString());
            
            sb.Append('#').Append(remarks);
            return sb.ToString();
        }
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
}


