using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using Object = UnityEngine.Object;
using IFix.Core;
/// <summary>
/// 热更新UI界面
/// 热更 0-50%
/// 加载 51-100%
/// </summary>
public class LauncherLoading : MonoBehaviour
{
    private string DATA_UTIL_B = "B";
    private string DATA_UTIL_K = "KB";
    private string DATA_UTIL_M = "MB";


    private Transform Tips;
    /// <summary>
    /// 进度条
    /// </summary>
    private RectTransform progressBar;
    /// <summary>
    /// 浮动游标
    /// </summary>
    private RectTransform cursors;
    /// <summary>
    /// 小提示
    /// </summary>
    private Text text;
    /// <summary>
    /// 进度提示
    /// </summary>
    private Text progressTxt;
    /// <summary>
    /// 当前进度
    /// </summary>
    private float currentProgress;
    /// <summary>
    /// 目标进度
    /// </summary>
    private float targetProgress;
    /// <summary>
    /// 动态更新总大小
    /// </summary>
    private int totalDownSize;
    /// <summary>
    /// 动态更新总大小
    /// </summary>
    private float totalDownSizeFixed;
    /// <summary>
    /// 动态更新总大小单位
    /// </summary>
    private string totalDownSizeUtil;
    /// <summary>
    /// 小提示
    /// </summary>
    private string[] loadingTipsArr;


    private TextAsset tipsAssets;

    private bool m_bStarDownload = false;
    public static List<Assembly> AssemblyList = new List<Assembly>();
    private static string STREAMING_ASSET_PATH = string.Empty;

    private static string rootPath;


    private Transform m_logo;


    private void Awake()
    {

        Transform tr = transform.Find("Canvas/loading/buttom");
        Tips = tr.Find("Tips");
        progressBar = tr.Find("bar/ProgressBar/Mask/bar").GetComponent<RectTransform>();
        cursors = tr.Find("bar/ProgressBar/point/image").GetComponent<RectTransform>();
        text = tr.Find("Text").GetComponent<Text>();
        progressTxt = tr.Find("bar/ProgressBar/Text").GetComponent<Text>();
        progressTxt.text = "资源加载中,请稍后...";
        tipsAssets = (Resources.Load("Launcher/Asset/LoadingTip") as TextAsset);
        loadingTipsArr = tipsAssets.text.Split('\r');
        m_logo = transform.Find("Canvas/loading/Logo");
        m_logo.gameObject.SetActive(false);
        ResetProgress();
        Tips.gameObject.SetActive(true);


        STREAMING_ASSET_PATH =

#if !UNITY_EDITOR && UNITY_ANDROID
         Application.dataPath + "!assets";   // 安卓平台
#else
         Application.streamingAssetsPath;  // 其他平台
#endif
    }
    private void Start()
    {
        ResetProgress();
        LetsGo();
    }
    private void LetsGo()
    {
        targetProgress = 1;
        progressTxt.text = "开始游戏...";
        int randomIndex = UnityEngine.Random.Range(0, loadingTipsArr.Length - 1);
        text.text = loadingTipsArr[randomIndex];
        Tips.gameObject.SetActive(false);



        if (Launcher.settingConfig.isSDK)
        {
        }
        else
        {
            StartCoroutine(LaunchGame());
        }


    }


    /// <summary>
    /// 设置小提示文本信息
    /// </summary>
    /// <param name="value"></param>
    private void SetText(string value)
    {
        text.text = value;
    }

    /// <summary>
    /// 设置当前进度
    /// </summary>
    /// <param name="progress"></param>
    public void SetProgress(float progress)
    {
        targetProgress = progress;
    }

    private void ResetProgress()
    {
        currentProgress = 0;
        targetProgress = 0;
        UpdateProgressBar(0);
    }

    /// <summary>
    /// 更新进度条位置
    /// </summary>
    /// <param name="progress"></param>
    private void UpdateProgressBar(float progress)
    {
        progress = Mathf.Clamp01(progress);
        float x = progressBar.rect.width * progress;
        progressBar.localPosition = new Vector3(x, progressBar.localPosition.y);
        cursors.localPosition = new Vector3(x, cursors.localPosition.y);
    }

    /// <summary>
    /// 更新信息获取成功
    /// </summary>
    /// <param name="args"></param>
    private void OnDynamicUpdateSuccess(params object[] args)
    {

        totalDownSize = int.Parse(args[0] as string);

        if (totalDownSize > 0)
        {
            //记录BI 玩家更新资源



            totalDownSizeUtil = FormatDataSize(totalDownSize, out totalDownSizeFixed);
            Debug.LogError("totalDownSize:" + totalDownSize);
            if (totalDownSize >= 5 * 1024 * 1024) // 5MB
            {

                string contentString = "";

                if (Application.platform == RuntimePlatform.Android)
                {
                    contentString = " 检测到有资源需要更新，更新内容为" + ColorTools.HandlerTextAddColor(Color.red, totalDownSizeFixed + totalDownSizeUtil) + "，建议在" + ColorTools.HandlerTextAddColor(Color.red, "WIFI") + "环境下下载。";
                }
                else
                {
                    contentString = " 检测到有资源需要重新解压，解压大小为" + ColorTools.HandlerTextAddColor(Color.red, totalDownSizeFixed + totalDownSizeUtil) + "，建议在" + ColorTools.HandlerTextAddColor(Color.red, "WIFI") + "环境下解压。";
                }

            }
            else
            {
            }
        }
        else
            StartCoroutine(LaunchGame());
        UnityEngine.Debug.Log("TotalDownSize:" + totalDownSize);
    }

    /// <summary>
    /// 格式化热更数据大小
    /// </summary>
    /// <param name="inSize"></param>
    /// <param name="outSize"></param>
    /// <returns></returns>
    private string FormatDataSize(int inSize, out float outSize)
    {
        string updateStr = DATA_UTIL_B;
        outSize = inSize;
        if (outSize >= 1024)
        {
            outSize /= 1024f;
            updateStr = DATA_UTIL_K;
            if (outSize >= 1024)
            {
                outSize /= 1024f;
                updateStr = DATA_UTIL_M;
            }
        }
        outSize = (float)Math.Round(Math.Max(0.01f, outSize), 1);
        return updateStr;
    }

    /// <summary>
    /// 更新进度
    /// </summary>
    /// <param name="args"></param>
    private void OnDynamicUpdate(params object[] args)
    {
        if (!m_bStarDownload)
        {
            m_bStarDownload = true;
        }
        int size = int.Parse(args[0] as string);
        targetProgress = ((float)size / totalDownSize) * 0.5f;
        if (size >= totalDownSize)
        {
            Caching.ClearCache();
            StartCoroutine(LaunchGame());
            progressTxt.text = "资源加载中,请稍后...";
        }
        UnityEngine.Debug.Log("Progress:" + targetProgress);

        //5M以上的资源显示加载进度信息
        if (totalDownSize > 5 * 1024 * 1024)
        {
            float csize = 0;
            string cutil = FormatDataSize(size, out csize);
            if (Application.platform == RuntimePlatform.Android)
                progressTxt.text = string.Format("更新下载中,请稍后...{0}{1}/{2}{3}", csize, cutil, totalDownSizeFixed, totalDownSizeUtil);
            else
                progressTxt.text = string.Format("加载中,请稍后...{0}{1}/{2}{3}", csize, cutil, totalDownSizeFixed, totalDownSizeUtil);
        }


    }


    private void OnDynamicUpdatePath(params object[] args)
    {
        string path = args[0] as string;
        Debug.LogError("热更新路径" + path);
        rootPath = path;
    }
    string[] injectAssemblys = new string[]
{
            //"Assembly-CSharp",
            "Demo",
            //"BattleSystem"
};
    IEnumerator LaunchGame()
    {
        SetProgress(0.51f);
        yield return null;
        float _time = Time.realtimeSinceStartup;
        //读取脚本，初始化程序集Launcher.Inst.UsedAssetBundle && 

        if (Launcher.settingConfig.codeType == ResourceLoad.codeType.editor)
        {
            AssemblyList.Add(Assembly.GetExecutingAssembly());
            List<string> _list = GetAllDll(Application.dataPath + "/Resources/Plugins");
            for (int i = 0; i < _list.Count; ++i)
            {
                Assembly _assembly = Assembly.LoadFrom(_list[i]);
                AssemblyList.Add(_assembly);
            }
        }
        else if (Launcher.settingConfig.codeType == ResourceLoad.codeType.abHotFix)//使用ab方式热更代码
        {
          
            string _finalPath = ResourceLoad.PathManager.URL("allscripts.assetbundle");
            if (!File.Exists(_finalPath))
            {
                _finalPath = GetStreamingAssetsPath("StreamingResources/allscripts");
            }
            Debug.LogError("尝试读取allscripts");
            AssetBundleCreateRequest _request = AssetBundle.LoadFromFileAsync(_finalPath, 0, 0xBA);//避免卡顿,用异步     

            yield return _request;

            AssetBundle assetBundle = _request.assetBundle;

            if (assetBundle != null)
            {
                Object[] _array = assetBundle.LoadAllAssets();
                for (int i = 0; i < _array.Length; ++i)
                {
                    TextAsset _ta = _array[i] as TextAsset;
                    if (null != _ta)
                    {
                        Assembly amy = Assembly.Load(EncryptyFile(_ta.bytes, 0xBB));
                        AssemblyList.Add(amy);
                    }
                }
                assetBundle.Unload(true);
            }

        }
        else if(Launcher.settingConfig.codeType == ResourceLoad.codeType.injectFix)//热更代码加载
        {
            if (Launcher.settingConfig.resourceLoadMode== ResourceLoad.ResourceLoadMode.eAssetbundle)
            {
                string _finalPath1 = ResourceLoad.PathManager.URL("allinject.assetbundle");
                Debug.LogError("尝试读取该injectfix文件");
                if (!File.Exists(_finalPath1))
                {
                    _finalPath1 = GetStreamingAssetsPath("StreamingResources/allinject");
                }
                if (File.Exists(_finalPath1))
                {
                    AssetBundleCreateRequest _request = AssetBundle.LoadFromFileAsync(_finalPath1, 0);//避免卡顿,用异步     

                    yield return _request;
                    if (_request == null)
                    {
                        Debug.LogError("injectfix文件null");

                    }

                    if (_request != null)
                    {
                        AssetBundle assetBundle = _request.assetBundle;


                        Object[] _array = assetBundle.LoadAllAssets();
                        for (int i = 0; i < _array.Length; ++i)
                        {
                            TextAsset _ta = _array[i] as TextAsset;
                            if (null != _ta)
                            {
                                var sw = System.Diagnostics.Stopwatch.StartNew();
                                PatchManager.Load(new MemoryStream(_ta.bytes));
                                Debug.LogError("完成读取该文件:" + _ta.name);
                            }
                        }
                        Debug.LogError("完成读取该injectfix文件");
                        assetBundle.Unload(true);
                    }
                }
            }
            else
            {
                for (int i = 0; i < injectAssemblys.Length; i++)
                {
                    string _finalPath2 = "Inject/" + injectAssemblys[i] + ".patch";
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var data = Resources.Load<TextAsset>(_finalPath2);
                    if (data == null)
                        continue;
                    PatchManager.Load(new MemoryStream(data.bytes));
                    Debug.LogError("读取inject assembly：" + injectAssemblys[i]);
                }
            }
        }

        UnityEngine.Debug.Log("初始化程序集耗时：" + (Time.realtimeSinceStartup - _time));

        SetProgress(1f);
        GameObject main = GameObject.Find("Main");
        if (null == main)
        {
            main = new GameObject();
            main.name = "Main";
        }
        main.AddComponent<Main>();
    }

    private void Update()
    {
        if (targetProgress == 0)
        {
            return;
        }
        if (currentProgress < targetProgress ||
            (targetProgress < 0.5f && totalDownSize > 0))//热更中 不走假进度
            currentProgress = targetProgress;
        else
        {
            currentProgress += 0.0005f;
            targetProgress = currentProgress;
        }
        UpdateProgressBar(currentProgress);
    }

    #region 加载完成后删除launcherloading界面
    public void LauncherComplete()
    {
        StartCoroutine(DelayRemove());
    }

    IEnumerator DelayRemove()
    {
        yield return new WaitForSeconds(0.5f);
        StopAllCoroutines();
        DestroyImmediate(gameObject);
    }
    #endregion

    private void OnDestroy()
    {

        Resources.UnloadAsset(tipsAssets);
        Resources.UnloadUnusedAssets();
    }


    #region 启动项
    public static string GetURLPath(string p_filename, string PreFix, string Suffix)
    {
        string path;
        path = "";

        if (Application.platform == RuntimePlatform.OSXEditor)
            path = Application.persistentDataPath + "/" + p_filename;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
            path = rootPath + p_filename;


        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            path = Application.dataPath + "/StreamingAssets/" + p_filename;
        }


        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            path = rootPath + p_filename;
            // path = Application.dataPath + "/StreamingAssets/" + p_filename;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            path = rootPath + p_filename;
        }
        path = PreFix + path;
        path = path + Suffix;
        Debug.LogError("path========" + path);
        return path;
    }


    public static string GetStreamingAssetsPath(string p_filename)
    {
        string Suffix = "";

        if (!string.IsNullOrEmpty(p_filename))
            Suffix = ".assetbundle";

        string path = STREAMING_ASSET_PATH + "/" + p_filename + Suffix;
        return path;
    }

    public static Component AddScript(GameObject go, string script)
    {
        return go.AddComponent(GetType(script));
    }

    public static Type GetType(string name, bool ignoreCase = false)
    {
        if (0 == AssemblyList.Count)
        {
            return Type.GetType(name);
        }
        for (int i = 0; i < AssemblyList.Count; ++i)
        {
            Type type = AssemblyList[i].GetType(name, false, ignoreCase);
            if (null != type)
            {
                return type;
            }
        }
        return null;
    }
    public static List<string> GetAllDll(string resourcePath)
    {


        if (string.IsNullOrEmpty(resourcePath) || !resourcePath.Contains(Application.dataPath))
        {
            Debug.LogError("非法路径");
            return null;
        }

        string[] files = Directory.GetFiles(resourcePath, "*.*", SearchOption.AllDirectories).Where(s =>

          s.ToLower().EndsWith(".dll")
 ).ToArray();


        for (int i = 0; i < files.Length; i++)
        {

            files[i] = files[i].Replace(@"\", "/");
        }

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
                string _value = _list[i].Replace(".dll", "");
            }
        }

        return _list;
    }

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

}


