
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using System.Reflection;
using System.IO;
using CoreFrameWork;
using UnityEngine;
using FrameWork.GUI.AorUI;
using System.Collections;
using FrameWork.App;
using FrameWork;
using CoreFrameWork.Scripts.Utils;
using CoreFrameWork.Com;
using CoreFrameWork.TimeTool;
using CoreFrameWork.Event;
using DG.Tweening.Core;
using FrameWork.GUI.AorUI.Core;
using Demo;
using FrameWork.Audio;
using FrameWork.Manager;
using ResourceLoad;

public enum codeType
{
    injectFix,//injectfix热更代码
    abHotFix,//ab包热更代码
}
public class Main : ApplicationCore
{
    public static Main Inst;
    /// <summary>
    /// 网络链接
    /// </summary>
    public static Connect CurrentConnect;
    private string UIRootPath = "TSUprefabs/UISystemRoot";
    protected override void InitResRoot()
    {
        Inst = this;
        ClearHotFixAB();
        InitHotUpdateTxtTo();

    }
    #region 热更资源信息
    private void InitHotUpdateTxtTo()
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        AnalyPersistentTxt(dic, "list-setting.txt");
        ResourcesManager.Instance.SetUpdateResList(dic);
    }
    private void AnalyPersistentTxt(Dictionary<string, string> dic, string txtName)
    {
        PathManager.RES_SDK_UPDATE_ROOT_PATH = SDKManager.Inst.SDKUpdateResRoot;
        string path = PathManager.RES_PERSISTENT_ROOT_PATH + txtName;
        if (File.Exists(path))
        {
            string[] datas = File.ReadAllLines(path);
            int len = datas.Length;
            for (int i = 0; i < len; i++)
            {
                string str = datas[i];
                if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()))
                {
                    continue;
                }
                string[] info = str.Split('|');
                if (info.Length < 2)
                {
                    continue;
                }
                dic.Add(info[2].Replace("StreamingResources/", ""), PathManager.RES_SDK_UPDATE_ROOT_PATH + info[2]);
            }
        }

    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        Log.S_IsWrite = Launcher.settingConfig.isWriteLog;
        AddWantsToQuit();
    }
    protected override void Update()
    {

        base.Update();
        #region 编辑器工具
        if (Application.isEditor)
        {
            GameNetBase.isDropRecv = Launcher.settingConfig.isDropRecv;
            GameNetBase.isDropSend = Launcher.settingConfig.isDropSend;

        }
        #endregion
       // if (Launcher.settingConfig.codeType != ResourceLoad.codeType.xlua)
            TimerManager.Update();
        ConnectManager.manager().Update(Time.realtimeSinceStartup);
    }
    protected override void StartGameLoad()
    {
        base.StartGameLoad();
        //防止编辑器bug延续到此
        ConfigManager.Instance.Clear();
        //inited后发消息,不可反转顺序
        IsInited = true;
        base.StartGameLoad();
        Log.Level = Launcher.settingConfig.logLevel;
        Log.Init(new UnityLoggerUtility(""));
        Application.targetFrameRate = 30;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;
        PrintSystemInfo();
        Log.Info("APP启动完成");
        ToLogin();
    }
    private void InitNet()
    {
        ConnectManager.manager().init();
    }
    private void Init()
    {
        InitNet();
        InitAudio();
        InitPicQuality();
    }
    /// <summary>
    /// 开始登录
    /// </summary>
    private void ToLogin()
    {
        Log.Debug("环境准备完成，启动");
        RegisterManagerEvent();
        GlobalEvent = SingletonManager.GetManager<FEventManager>().AddFGameEvent("GlobalEvent");
        DOTweenSettings.CreateInstance<DOTweenSettings>();
        AsyncCombiner asyncCombiner = new AsyncCombiner();
        LoadUIRoot(asyncCombiner);
        InitLanguage(asyncCombiner);
        InitLanguagePackage(asyncCombiner);
        asyncCombiner.AddCompletionCall(() =>
        {
            Init();
            InitConfig();
            RegisteEvent();
            //加载登录界面 成功后关闭launcherloading
            GameObject.Find("LauncherLoading").GetComponent("LauncherLoading").ref_InvokeMethod_Inst_Public("LauncherComplete", null);
            ConnectManager.manager().init();

            UIManager.Inst.OpenUI(UIDef.LoginView);
            //Main.CurrentConnect = ConnectManager.manager().CreateConnect(PlayerModel.Ip, PlayerModel.Port, () =>
            //{

            //});
        });
    }
    #region 初始化加载
    private void LoadUIRoot(AsyncCombiner asyncCombiner)
    {
        AsyncCombiner.AsyncHandle handle = asyncCombiner.CreateAsyncHandle();
        SingletonManager.GetManager<ResourcesManager>().LoadPrefabInstance(UIRootPath, (obj) => {
            if (null == obj)
            {
                return;
            }
            var uiManager = obj.GetComponent<AorUIManager>();
            uiManager.init();
            SingletonManager.AddManager(uiManager);
            handle.Finish();

        });
    }

    private void InitLanguage(AsyncCombiner asyncCombiner)
    {
        AsyncCombiner.AsyncHandle handle = asyncCombiner.CreateAsyncHandle();
        SingletonManager.GetManager<ResourcesManager>().LoadText(GetConfigPath("Language/Language"), (obj,refID) =>
        {
            if (null == obj)
            {
                return;
            }

            Lang.InitLanguage(GameUtils.EncryptyFile(obj.bytes, 0xDC));
            ResourcesManager.Instance.Release(refID);
            handle.Finish();
        },true);
    }
    private void InitLanguagePackage(AsyncCombiner asyncCombiner)
    {
        AsyncCombiner.AsyncHandle handle = asyncCombiner.CreateAsyncHandle();
        SingletonManager.GetManager<ResourcesManager>().LoadABText(GetConfigPath("Language/Language"), "LanguagePackage", (obj,refID) =>
        {
            if (null == obj)
            {
                return;
            }
            Lang.InitLanguagePackage(GameUtils.EncryptyFile(obj.bytes, 0xDC));
            ResourcesManager.Instance.Release(refID);
            handle.Finish();

        }, true);
    }
    #endregion
    #region 删除热更外的所有内容
    /// <summary>
    /// PC 删除热更外的所有内容
    /// </summary>
    private void ClearHotFixAB()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            string _root ="";
            Debug.LogError("_root:" + _root);
            string _settingListPath = _root + "URL/setting.txt";
            if (File.Exists(_settingListPath))
            {
                List<string> _realPathList = new List<string>();

                List<string> _list = File.ReadAllLines(_settingListPath).ToList();
                for (int i = 0; i < _list.Count; ++i)
                {
                    string _str = _list[i];
                    if (string.IsNullOrEmpty(_str))
                    {
                        continue;
                    }
                    string[] _info = _str.Split('|');
                    string _abPath = _info[2];
                    _realPathList.Add((_root + _abPath).Replace("\\", "/"));
                }

                string[] files = Directory.GetFiles(_root, "*.assetbundle", SearchOption.AllDirectories).ToArray();
                for (int i = 0; i < files.Length; ++i)
                {
                    string _tmp = files[i].Replace("\\", "/");
                    if (!_realPathList.Contains(_tmp))
                    {
                        File.Delete(_tmp);
                    }
                }
            }

        }
    }
    #endregion



    #region 事件
    private void RegisteEvent()
    {

        ConnectManager.manager().NetEvent.AddEvent(eYKNetEvent.AccessMsg, AccessMsg);
        Main.Instance.GlobalEvent.AddEvent(eYKNetEvent.ReceiveMsg, ReceiveMsg);
        ConnectManager.manager().NetEvent.AddEvent(eYKNetEvent.Ping, Ping);
        ConnectManager.manager().NetEvent.AddEvent(eYKNetEvent.Reconnect, Reconnect);


    }

    private void UnRegisteEvent()
    {
        ConnectManager.manager().NetEvent.RemoveEvent(eYKNetEvent.AccessMsg, AccessMsg);
        ConnectManager.manager().NetEvent.RemoveEvent(eYKNetEvent.Ping, Ping);
        ConnectManager.manager().NetEvent.RemoveEvent(eYKNetEvent.Reconnect, Reconnect);
        Main.Instance.GlobalEvent.RemoveEvent(eYKNetEvent.ReceiveMsg, ReceiveMsg);
    }
    #endregion


    private void ReceiveMsg(params object[] args)
    {
        Log.Error("ReceiveMsg++++");
    }


    private void SubCameraInfoChange(params object[] args)
    {

    }

    private void OnGameIniteCompleted(params object[] args)
    {
        Main.CurrentConnect.MaxFrameCount = 300;
    }



    public void ModifyTime()
    {

    }

    private float _accessTime = 0;

    private void AccessMsg(params object[] args)
    {
        if (null != args && args.Length > 0)
        {
            ErlKVMessage _msg = args[0] as ErlKVMessage;
            if (null != _msg)
            {
                if (_msg.Cmd == "echo")
                {
                    _accessTime = Time.realtimeSinceStartup;
                }
            }
        }
    }

    private void Ping(params object[] args)
    {
        long _timestamp = (long)args[0];
        int _millisecond = (int)args[1];

        TimeKit.ModifyTime(_timestamp, _millisecond, Time.realtimeSinceStartup);
    }
    private void Reconnect(params object[] args)
    {
    }
    #region 打印系统数据
    private void PrintSystemInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("系统信息：");
        sb.AppendLine(UnityEngine.SystemInfo.deviceModel);
        sb.AppendLine(UnityEngine.SystemInfo.deviceName);
        sb.AppendLine(UnityEngine.SystemInfo.deviceType.ToString());
        sb.AppendLine(UnityEngine.SystemInfo.graphicsDeviceName);
        sb.AppendLine(UnityEngine.SystemInfo.graphicsDeviceVendor);
        sb.AppendLine(UnityEngine.SystemInfo.graphicsDeviceVersion);
        sb.AppendLine(UnityEngine.SystemInfo.graphicsMemorySize.ToString());
        sb.AppendLine(UnityEngine.SystemInfo.graphicsShaderLevel.ToString());
        sb.AppendLine(UnityEngine.SystemInfo.maxTextureSize.ToString());
        sb.AppendLine(UnityEngine.SystemInfo.operatingSystem);
        sb.AppendLine(UnityEngine.SystemInfo.processorCount.ToString());
        sb.AppendLine(UnityEngine.SystemInfo.processorType);
        sb.AppendLine(UnityEngine.SystemInfo.systemMemorySize.ToString());

        Log.Info(sb.ToString());
    }
    #endregion

    private void InitConfig()
    {
        //初始化配置表
    }
    #region 配置表加载方法
    private void _ImportConfig<T>(string path, bool allowBinary = true, CallBack call = null) where T : Config
    {
        SingletonManager.GetManager<ResourcesManager>().LoadText(GetConfigPath(path), (obj,refID) =>
        {
            if (null == obj)
            {
                return;
            }
            byte[] _byteArray = GameUtils.EncryptyFile(obj.bytes, 0xDC);
            ConfigManager.Instance.ImportBinaryInfo<T>(_byteArray);
            ResourcesManager.Instance.Release(refID);
            if (call != null)
            {
                call();
            }
        });
    }

    private void _ImportConfig<T1, T2>(string path, bool allowBinary = true, CallBack call = null)
        where T1 : Config
        where T2 : T1
    {
        SingletonManager.GetManager<ResourcesManager>().LoadText(GetConfigPath(path), (obj,refID) =>
        {
            if (null == obj)
            {
                return;
            }
            byte[] _byteArray = GameUtils.EncryptyFile(obj.bytes, 0xDC);
            ConfigManager.Instance.ImportBinaryInfo<T1, T2>(_byteArray);
            ResourcesManager.Instance.Release(refID);
            if (call != null)
            {
                call();
            }
        });
    }

    private void _ImportGameConfig<T>(string path, bool allowBinary = true, CallBack call = null) where T : Config
    {
        SingletonManager.GetManager<ResourcesManager>().LoadText(GetConfigPath(path), (obj, refID) =>
        {
            if (null == obj)
            {
                return;
            }
            try
            {
                byte[] _byteArray = GameUtils.EncryptyFile(obj.bytes, 0xDC);
                ConfigManager.Instance.ImportBinaryInfo<T>(_byteArray);
            }
            catch (Exception e)
            {
                Log.Error("缺少配置 " + path);
                throw;
            }
            ResourcesManager.Instance.Release(refID);
            if (call != null)
                call();
        });
    }
    #endregion

    public void InitAudio()
    {
    }
    public void InitPicQuality()
    {
    }
    #region 注销游戏
    public override void ReleaseGame()
    {
        base.ReleaseGame();
        try
        {
            ReleaseData();
        }
        catch (System.Exception e)
        {
            Log.Error(e.StackTrace);
        }
    }
    public void ReleaseData()
    {
        Log.Error("释放数据，结束游戏。");
        InitAudio();
        SingletonManager.Clear();
        AorButton.IsDisableAllBtn = false;
        GameNetBase.Dispose();
        GameNetBase.isCounectEd = false;
        GameNetBase.isLogin = false;
        Time.timeScale = 1;
        TimerManager.Dispose();
        StartCoroutine(ResourcesManager.Instance.ReleaseAll());
        List<Type> types = new List<Type>();
        try
        {
            Type[] _array = Assembly.GetExecutingAssembly().GetTypes();
            if (null != _array)
            {
                types = _array.Where(item => item.IsSubclassOf(typeof(ModelBase))).ToList();
            }
            else
            {
                Log.Error("获取Type失败。");
            }
        }
        catch (Exception e)
        {
            Log.Error("xxxxxxxxxxxxxxxxxxx");
            Log.Error(e);

        }
        Log.Error("types count:" + types.Count);
        for (int i = 0; i < types.Count; i++)
        {
            Type sp = types[i];
            PropertyInfo[] scripts = sp.GetProperties(BindingFlags.Static | BindingFlags.Public);
            List<PropertyInfo> scriptLsit = scripts.Where(v =>
            v.Name == "inst" ||
            v.Name == "Inst" ||
             v.Name == "instance" ||
            v.Name == "Instance").ToList();
            PropertyInfo script = null;
            ModelBase module = null;
            if (scriptLsit == null || scriptLsit.Count <= 0)
            {
                Log.Error("没有实现代码，Model:" + sp.Name);
                continue;
            }
            script = scriptLsit[0];
            try
            {
                module = script.GetValue(null, null) as ModelBase;
                module.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        RegisteEvent();


    }
    #endregion

    #region 重连游戏
    public void ReConnect(CallBack<bool> callBack = null)
    {
        GameNetBase.isReCounecting = true;
        GameNetBase.isCounectEd = false;
        ConnectManager.manager().closeAllConnects();

        string ip = "";
        int port = 0;

        Main.CurrentConnect = ConnectManager.manager().CreateConnect(ip, port, () =>
        {
            Connect[] cons = ConnectManager.manager().factory.connectArray.ToArray();
            if (cons.Length > 1)
            {
                Log.Error("Too many connects." + cons.Length);
                ConnectManager.manager().closeAllConnects();
                if (callBack != null)
                    callBack(false);
                return;
            }
            else if (cons.Length <= 0)
            {
                if (callBack != null)
                    callBack(false);
                return;
            }
            List<ErlEntry> _list = ConnectManager.manager().GetErlEntryList(Main.CurrentConnect);
            if (1 == _list.Count && _list[0].type == NetDef.SendType.SendLogin)//登录断线重连
            {
                ConnectManager.manager().reaccess(Main.CurrentConnect as ErlConnect);
            }
            else
            {
            }
        });
    }
    #endregion


    public void QuitApplication()
    {
    }


    public static void AddWantsToQuit()
    {
        Application.wantsToQuit += WantsToQuit;
    }
    public static void RemoveWantsToQuit()
    {
        Application.wantsToQuit -= WantsToQuit;
    }
    private static bool WantsToQuit()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return false;
        }
        return true;
    }

    public void Quit()
    {
        Application.Quit();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnRegisteEvent();

    }
    #region 事件注册
    private void RegisterManagerEvent()
    {
        //注入音效播放实现
        AorUIAssetLoader.SoundPlayCustomFunc = s => { SingletonManager.GetManager<AudioManager>().PlaySound(s); };

        //注入从缓冲池中读取预制体的方法
        AorUIAssetLoader.LoadPrefabCustomFunc = (s, action) =>
        {
            SingletonManager.GetManager<ResourcesManager>().LoadPrefab(s, (obj,refID) =>
            {
                if (null != obj)
                {
                    action(obj);
                }
            });
        };
        AorImage.LoadAorImage = (path, img, callBack) =>
        {
            SingletonManager.GetManager<ResourcesManager>().LoadSpriteSingle(path, (obj,refID) =>
            {
                if (null == img)
                {
                    return;
                }
                if (null != obj)
                {
                    img.sprite = obj;
                    img.Alpha = 1;
                    if (null != callBack)
                    {

                        callBack(img, refID);
                        callBack = null;
                    }
                    return;
                }

            });
        };


        AorRawImage.LoadAorRawImageMat = (path, img, callBack) =>
        {
            SingletonManager.GetManager<ResourcesManager>().LoadMaterial(path, (obj,refID) =>
            {
                if (null == img)
                {
                    return;
                }
                if (null != obj)
                {
                    img.material = obj;

                    if (null != callBack)
                    {
                        callBack(refID);
                    }

                }
            });
        };
        AorRawImage.LoadAorRawImage = (path, img, callBack) =>
        {
            SingletonManager.GetManager<ResourcesManager>().LoadTexture(path, (obj,refID) =>
            {

                if (null == img)
                {
                    return;
                }

                if (path != img.m_currentPath)
                {
                    return;
                }

                if (null != obj)
                {
                    Texture tex = obj;
                    img.texture = tex;
                    img.Alpha = 1;
                    if (null != callBack)
                    {

                        callBack(img, refID);
                        callBack = null;
                    }
                }

            });
        };

        if (!Application.isEditor)
        {
            //AorMaterialCreater 注入GetShader方法
            AorMaterialCreater.CustomGetShaderFunc = (shaderName) =>
            {

                return GetConstShader(shaderName);

            };
        }

        //其他UI组件的注入方法
        AorButton.OnSoundPlay = AorUIAssetLoader.SoundPlay;

        AorText.OnAwake = (key, aorText) =>
        {
            if (Lang.IsExistLanguagePackageKey(key))
                aorText.text = Lang.get(key);
            else if (Lang.IsExistLanguageKey(key))
                aorText.text = Lang.getLang(key);
        };

        AorTextIncSprites.OnAwake = (key, aorText) =>
        {
            if (Lang.IsExistLanguagePackageKey(key))
                aorText.text = Lang.get(key);
            else if (Lang.IsExistLanguageKey(key))
                aorText.text = Lang.getLang(key);
        };
    }
    #endregion

    #region C#

    #endregion
}