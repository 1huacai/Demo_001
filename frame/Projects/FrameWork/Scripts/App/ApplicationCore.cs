using System;
using System.Collections.Generic;
using UnityEngine;
using CoreFrameWork;
using CoreFrameWork.Event;
using CoreFrameWork.TimeTool;
using FrameWork.SceneManger;
using FrameWork.Audio;
using FrameWork.Manager;
using ResourceLoad;

namespace FrameWork.App
{
    public class ApplicationCore : MonoBehaviour, IGetTime
    {
        #region 单例
        protected static ApplicationCore m_instance;
        public static ApplicationCore Instance
        {
            get
            {
                return m_instance;
            }
        }
        #endregion
        /// <summary>
        /// 添加的全局事件管理器
        /// </summary>
        public FGameEvent GlobalEvent;
        public static bool IsInited = false;
        #region 获取shader
        public Shader GetConstShader(string name)
        {
            ResourcesManager _mng = SingletonManager.GetManager<ResourcesManager>();
            Shader _shader = _mng.GetShader(name);
            if (null == _shader)
            {
                UnityEngine.Debug.LogError("Can not find the shader: " + name);
            }
            return _shader;

        }
        #endregion
        #region 时间
        private float m_timeScale = 1;
        public float timeScale
        {
            get { return m_timeScale; }
            set
            {
                m_timeScale = value;
                Time.timeScale = m_timeScale;
            }
        }

        public float GetTime()
        {

            return Time.time;
        }

        public float GetUnscaledTime()
        {

            return Time.realtimeSinceStartup;
        }
        #endregion
        #region 生命周期

        protected virtual void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
                InintManager();
            }
            else
            {
                if (Application.isEditor)
                {
                    GameObject.DestroyImmediate(this);
                }
                else
                {
                    GameObject.Destroy(this);
                }
            }
        }
        protected virtual void Start()
        {
            ResourcesManager.Instance.Init(() =>
            {
                Log.Info("ResourcesManager启动完成");
                StartGameLoad();
            });
        
        }
        //在运行环境重载时,清除单例.防止编译Dll时因为Dll被占用导致Unity IDE崩溃.
        protected virtual void OnDestroy()
        {
            return;
        }
        protected virtual void Update()
        {


        }
        #endregion

        #region 基方法
        protected virtual void InitResRoot()
        {
        }
        protected string GetConfigPath(string path)
        {
            return "ConfigByte/" + path;
            //if (UsedAssetBundle)
            //    return "ConfigByte/" + path;
            //else
            //    return "Config/" + path;
        }
        /// <summary>
        /// 基础框架加载完成 开始游戏
        /// </summary>
        protected virtual void StartGameLoad()
        {
            InitResRoot();
        }
        private void InintManager()
        {
            TimerManager.Init(this);
            SingletonManager.AddManager(SceneManager.CreateInstance());
            SingletonManager.GetManager<SceneManager>().transform.parent = transform;
            //  GetManager<SceneManager>().Init();
            Log.Info("SceneManager启动完成");

            SingletonManager.AddManager(AudioManager.CreateInstance());
            SingletonManager.GetManager<AudioManager>().init();
            SingletonManager.GetManager<AudioManager>().transform.parent = transform;
            Log.Info("AudioManager启动完成");
            SingletonManager.AddManager(FEventManager.CreateInstance());
            Log.Info("FEventManager启动完成");
            //SingletonManager.AddManager(ResourcesManager.CreateInstance());
            


        }
        #endregion



        public virtual void ReleaseGame()
        {
            SingletonManager.Clear();
        }
    }
}

