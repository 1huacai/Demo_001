using CoreFrameWork;
using CoreFrameWork.Scripts.Utils;
using System;
using UnityEngine;

namespace ResourceFrameWork
{
    public class ResourcesManager:MonoBehaviour
    {

        private static ResourcesManager m_instance;
        public static ResourcesManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance=new GameObject("ResourcesManager").AddComponent<ResourcesManager>();
                }
                return m_instance;
            }
        }

        /// <summary>
        /// SDK提供的外部资源文件路径，热更用
        /// </summary>
        public static string RESROOT;
        /// 调试模式
        /// </summary>
        [HideInInspector]
        public static bool DebugMode = false;

        /// <summary>
        /// 是否通过assetbundle加载资源
        /// </summary>
        public static bool UsedAssetBundle = false;
        public static bool WebGL = false;
        

        private FResourcesManager m_newRM;
        void Awake()
        {
            DontDestroyOnLoad(gameObject);

        }


        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        /// <param name="callback">成功后的回调</param>

        public void Init(CallBack callback)
        {
            FResourcesManager.UsedAssetBundle = UsedAssetBundle;
            FResourcesManager.WebGL = WebGL;
            PathUtils.RESROOT = RESROOT;
            m_newRM = FResourcesManager.Inst;
            m_newRM.DebugMode = DebugMode;
            StartCoroutine(m_newRM.Init(callback));



        }

        public void UnloadUnusedAssets(CallBack callBack)
        {
            m_newRM.UnloadUnusedAssets(callBack);
        }

        #region LegacyLoadWay
        /// <summary>
        /// 通过路径获得一个资源,如果资源是GameObject,返回GameObject上的resourceKeeper脚本.否则返回resourceRef(引用保持器);
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="handle"></param>
        /// <param name="async">是否异步加载</param>
        /// <param name="priority"></param>
        public void LoadObject(string path, Type type, CallBack<object> handle, bool async=true, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal)
        {
            m_newRM.LoadObject(async, path, type, handle, priority);
        }
        #endregion



        public Shader GetShader(string name)
        {
            Shader _shader = m_newRM.GetShader(name);
            return _shader;
        }



    }
}