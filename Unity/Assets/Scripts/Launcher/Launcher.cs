using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CoreFrameWork;
using ResourceLoad;

/// <summary>
/// 传递面板参数
/// 初始化SDK
/// 闪屏处理
/// </summary>
public class Launcher : MonoBehaviour
{
    public enum LogLevelShow
    {
        Off = 0,
        Error = 8,
        Warning = 12,
        Info = 14,
        Debug = 15,
        All = 65535,
    }
    #region 设置
    private static SettingConfig mConfig;
    public static SettingConfig settingConfig
    {
        get
        {
            if (mConfig == null)
            {
                SettingConfigRef configRef = GameObject.FindObjectOfType<SettingConfigRef>();
                if (configRef != null)
                {
                    mConfig = configRef.mConfig;
                }
            }

            if (mConfig == null)
            {
                Debug.LogError("!!!!!!!!!!!!!没再场景中找到SettingConfig脚本!!!!!!!!!!!!!!!!");
            }

            return mConfig;
        }
    }
    #endregion
    /// <summary>
    /// 闪屏
    /// </summary>
    private GameObject splash;
    private GameObject splash1;
    private GameObject splash2;
    /// <summary>
    /// 出包日期 用来清理老包缓存
    /// 不用管 自动填写的
    /// </summary>
    /// <summary>
    /// SDK初始化
    /// </summary>
    private void SDKInint()
    {
        //new GameObject("AndroidSDKManager").AddComponent<SDKManager>();
        SDKManager.CreateInstance();
        Log.Info("AndroidSDKManager启动完成");
    }
    void Awake()
    {
        DontDestroyOnLoad(this);
        SDKInint();
    }

    private void Start()
    {
        RStart();
        gameObject.AddComponent<FPS>();
    }

    private void RStart()
    {
        //检测出包的日期标记,清理掉老包的缓存信息
        String str = PlayerPrefs.GetString("releaseDate");
        if (str != settingConfig.releaseDate)
        {
            Caching.ClearCache();
            PlayerPrefs.SetString("releaseDate", settingConfig.releaseDate);
            PlayerPrefs.Save();
        }
        //闪屏然后进入游戏
        StartGame();
    }
    private void StartGame()
    {
        //更新UI
        GameObject win = Resources.Load("Launcher/Prefabs/Loading") as GameObject;
        win = GameObject.Instantiate(win) as GameObject;
        win.name = "LauncherLoading";
        win.transform.SetParent(transform);
        splash = win.transform.Find("Canvas/splash").gameObject;
        splash1 = win.transform.Find("Canvas/splash1").gameObject;
        splash2 = win.transform.Find("Canvas/splash2").gameObject;
        if (!settingConfig.isShowSplash)
        {
            if (!Application.isEditor)
            {
                RawImage _bg = win.transform.Find("Canvas/loading/bg").GetComponent<RawImage>();
                _bg.texture = Resources.Load("Launcher/Texture/Loadingbg") as Texture2D;
            }

            splash.gameObject.SetActive(false);
            splash1.gameObject.SetActive(false);
            splash2.gameObject.SetActive(false);

            RawImage _img = splash.GetComponent<RawImage>();
            _img = null;
            _img = splash1.GetComponent<RawImage>();
            _img = null;
            _img = splash2.GetComponent<RawImage>();
            _img = null;
            win.AddComponent<LauncherLoading>();
            win.transform.Find("Canvas/loading").gameObject.SetActive(true);
        }
        else
        {
            if (!Application.isEditor)
            {
                RawImage _bg = win.transform.Find("Canvas/loading/bg").GetComponent<RawImage>();
                _bg.texture = Resources.Load("Launcher/Texture/Loadingbg") as Texture2D;
            }

            RawImage _img = splash.GetComponent<RawImage>();
            RawImage _img1 = splash1.GetComponent<RawImage>();
            RawImage _img2 = splash2.GetComponent<RawImage>();
            Color _color = _img.color;
            _color.a = 0f;
            _img.color = _color;

            _color = _img1.color;
            _color.a = 0f;
            _img1.color = _color;

            _color = _img2.color;
            _color.a = 0f;
            _img2.color = _color;

            splash.gameObject.SetActive(true);
            splash1.gameObject.SetActive(true);
            splash2.gameObject.SetActive(true);

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor)
            {
                StartCoroutine(_fade(_img, 1f, 3f, () =>
                {
                    StartCoroutine(_fade(_img, 0f, 1.5f, null));

                    StartCoroutine(_fade(_img1, 1f, 3f, () =>
                    {
                        StartCoroutine(_fade(_img1, 0f, 1.5f, null));

                        StartCoroutine(_fade(_img2, 1f, 3f, () =>
                        {

                            StartCoroutine(_delay(1f, () =>
                            {
                                _img = null;
                                _img1 = null;
                                _img2 = null;
                                splash.gameObject.SetActive(false);
                                splash1.gameObject.SetActive(false);
                                splash2.gameObject.SetActive(false);
                                win.transform.Find("Canvas/loading").gameObject.SetActive(true);
                                win.AddComponent<LauncherLoading>();
                                StopAllCoroutines();
                            }));
                        }));
                    }));
                }));
            }
            else
            {

                StartCoroutine(_fade(_img1, 1f, 3f, () =>
                {
                    StartCoroutine(_fade(_img1, 0f, 1.5f, null));

                    StartCoroutine(_fade(_img2, 1f, 3f, () =>
                    {

                        StartCoroutine(_delay(1f, () =>
                        {
                            _img = null;
                            _img1 = null;
                            _img2 = null;
                            splash.gameObject.SetActive(false);
                            splash1.gameObject.SetActive(false);
                            splash2.gameObject.SetActive(false);
                            win.transform.Find("Canvas/loading").gameObject.SetActive(true);
                            win.AddComponent<LauncherLoading>();
                            StopAllCoroutines();
                        }));
                    }));
                }));
            }

        }
    }

    private void OnDestroy()
    {
    }

    private IEnumerator _delay(float second, Action action)
    {

        yield return new WaitForSeconds(second);
        if (null != action)
        {
            action();
        }

    }

    private IEnumerator _fade(RawImage image, float alphaValue, float duration, Action onComplete)
    {
        float _alpha = image.color.a;

        int _direction = _alpha < alphaValue ? 1 : -1;

        float _speed = 0 == duration ? float.MaxValue : (alphaValue - _alpha) / duration;

        while (true)
        {
            yield return null;
            if (1 == _direction)
            {
                if (_alpha >= alphaValue)
                {
                    if (null != onComplete)
                    {
                        onComplete();
                    }
                    yield break;
                }
            }
            else if (-1 == _direction)
            {
                if (_alpha <= alphaValue)
                {
                    if (null != onComplete)
                    {
                        onComplete();
                    }
                    yield break;
                }
            }
            else
            {
                if (null != onComplete)
                {
                    onComplete();
                }
                yield break;
            }
            _alpha = image.color.a;
            _alpha += Time.deltaTime * _speed;

            Color _color = image.color;
            _color.a = _alpha;
            image.color = _color;
        }

    }
}
