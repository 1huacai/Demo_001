using FrameWork.GUI.AorUI;
using System;
using System.Collections;
using UnityEngine;
namespace FrameWork.App
{

    /*
     * ScriptDataStorage Awake 
     * 从最低端子节点开始ScriptDataStorage Cover ->MonoSwitch.Awake ->MonoSwitch.OnEnable
     * MonoSwitch.Start
     *  
     */

    /// <summary>
    /// 公共出来的可连接字段,只能是unity内置的组建
    /// **** 继承此类,请使用 OnScriptCoverFinish方法替代Awake方法,  initialization方法替代Start方法.
    /// </summary>
    public class MonoSwitch : MonoBehaviour, IMonoSwitch
    {
        //------------------ IMonoSwitch 实现 ---------------
        #region IMonoSwitch 实现

        public virtual string ExportData()
        {
            return "";
        }
        public virtual void ImportData(string stringData)
        {

        }
        public void SetOtherParma(string target, string stringData)
        {
            if (target == GetType().ToString())
            {
                ImportData(stringData);
            }
        }


        public void RemoveCall(string className)
        {
            if (className == GetType().ToString())
            {
                OnRemoved();
            }
        }

        /// <summary>
        /// ykapp启动后才会调用
        /// </summary>
        public virtual void OnAwake()
        {

        }
        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {
            //
        }
        protected virtual void Initialization()
        {

        }
        protected virtual void OnUpdate()
        {

        }
        protected virtual void OnRemoved()
        {

        }
        protected virtual void OnDestroy()
        {


        }

        public virtual void OnEditorAwake()
        {


        }
        protected virtual void OnEditorStart()
        {

        }

        protected virtual void OnEditorUpdate()
        {


        }


        public static void waitYKApplicationWhenDo(MonoBehaviour who, Action callback)
        {
            if (ApplicationCore.IsInited)
            {
                callback();
            }
            else
            {
                who.StartCoroutine(waitForYKApplication(callback));
            }

        }

        static IEnumerator waitForYKApplication(Action callback)
        {

            while (!ApplicationCore.IsInited)
            {

                yield return 1;
            }

            if (callback != null)
                callback();


        }

        /// <summary>
        /// 所有IMonoSwitch用的关键启动过程
        /// </summary>
        /// <param name="mono"></param>
        public static void PublicStaticProcess(IMonoSwitch mono)
        {
            if (!Application.isPlaying)
            {
                mono.OnEditorAwake();
                return;
            }
            Finish(mono);


            //非正常流程 直接调finish以保证运行结果 *真机不可能出现这种结果*
            if (ApplicationCore.Instance == null)
            {

                GameObject auiPrefab = GameObject.Find(AorUIManager.PrefabName);
                if (auiPrefab != null && auiPrefab.GetComponent<AorUIManager>() != null)
                {
                    Debug.LogWarning("AorUIManager独立运行流程...");
                    Finish(mono);
                    return;
                }

                Debug.LogWarning("非正常流程,现在启动YKApplication");
                GameObject main = new GameObject();
                main.name = typeof(ApplicationCore).Name;
                main.AddComponent<ApplicationCore>();
                //                YKApplication.Instance.GlobalEvent.AddEvent(GlobalEvents.ApplicationEvents.ApplicationInitFinish, (args) =>
                //                {
                //
                //                    Finish(mono);
                //
                //                });
            }
            //            else
            //            {
            //                //已经有ykAPP的情况
            //
            //                if (!YKApplication.Instance.UsedAssetBundle)
            //                {
            //                    //非动态脚本模式
            //                    if (YKApplication.IsInited)
            //                    {
            //                        // 启动完成直接回调
            //                        Finish(mono);
            //                    }
            //                    else
            //                    {
            //                        //启动未完成就添callBack
            //                        YKApplication.Instance.GlobalEvent.AddEvent(GlobalEvents.ApplicationEvents.ApplicationInitFinish, (args) =>
            //                        {
            //
            //                            Finish(mono);
            //
            //                        });
            //                    }
            //                }
            //                else
            //                {
            //                    //动态脚本模式
            //                    if (YKApplication.IsInited)
            //                    {
            //                        
            //                        Finish(mono);
            //                    }
            //                }
            //            }
        }

        //--------------------------------------------------- 容易修改的区域
        private void Awake()
        {
            PublicStaticProcess(this);
        }


        private void Start()
        {
            float time = Time.realtimeSinceStartup;
            Initialization();
           // float dt = (Time.realtimeSinceStartup - time) * 1000f;
           // if (dt < 10)
             //   return;
           // Log.Error("UI Init " + gameObject.name +" "+ (dt));
        }



        //--------------------------------------------------- 容易修改的区域 End
        private void Update()
        {
            if (!Application.isPlaying)
            {
                OnEditorUpdate();

            }
            else if (ApplicationCore.IsInited)
            {

                OnUpdate();
            }
        }
        public static void Finish(IMonoSwitch mono)
        {
            float time = Time.realtimeSinceStartup;
            mono.OnAwake();
          //  float dt = (Time.realtimeSinceStartup - time)*1000f;
           // if (dt < 10)
           //     return;
           // Log.Error("UI OnAwake " + mono.gameObject.name + " " + (dt));
        }

        #endregion
        //------------------ IMonoSwitch 实现 --------------- End



    }
}
