
using CoreFrameWork;
using CoreFrameWork.Event;
using CoreFrameWork.Scripts.Utils;
using FrameWork.App;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ResourceFrameWork
{
    internal class FSceneProcess : HandleBase
    {
        internal string Name;
        internal List<CallBack> callBackList;


        internal FrameDef.AsyncState ProcessState;
        internal bool LoadAsync;
        internal LoadSceneMode Mode;
        internal FResourcesManager m_ResourcesManager;

        public FSceneProcess(string name, FResourcesManager manager, List<CallBack> list, bool async, LoadSceneMode mode)
        {
            ProcessState = FrameDef.AsyncState.Hanging;

            Name = name;
            this.m_ResourcesManager = manager;
            LoadAsync = async;
            Mode = mode;
            callBackList = list;

        }


        public void AddCallBack(List<CallBack> list)
        {
            if (null == list)
            {
                return;
            }

            for (int i = 0; i < list.Count; ++i)
            {
                if (!callBackList.Contains(list[i]))
                {
                    if (null != list[i])
                    {
                        callBackList.Add(list[i]);
                    }
                }
            }

        }

        /// <summary>
        /// 场景加载完成
        /// </summary>
        public void ProcessComplete()
        {
            ProcessState = FrameDef.AsyncState.Done;
         FResourcesManager.Inst.ResourceEvent.DispatchEvent(FrameDef.SceneProcessType.SceneLoaded, Name);
            m_ResourcesManager.SceneProcessList.Remove(this);

            int _count = 0;
            if (null != callBackList)
            {
                _count = callBackList.Count;

                for (int i = 0; i < _count; ++i)
                {
                    if (null != callBackList[i])
                    {
                        callBackList[i]();

                        callBackList[i] = null;
                    }
                }
            }

            callBackList = null;

        }

        internal void TryLoad()
        {
            if (ProcessState == FrameDef.AsyncState.Ready)
            {
                ProcessState = FrameDef.AsyncState.Loading;
                m_ResourcesManager.StartCoroutine(_loadScene(Name));
            }

        }

        private IEnumerator _loadScene(string name)
        {

            if (!UsedAssetBundle)
            {
                if (LoadAsync)
                {
                    AsyncOperation asy = SceneManager.LoadSceneAsync(name, Mode);
                    yield return asy;
                }
                else
                {
                    SceneManager.LoadScene(name, Mode);
                }

                ProcessComplete();
                yield break;
            }
            else
            {
                string _finalPath = PathUtils.GetURLPath("StreamingResources/" + Info.LoadPath,
                    string.Empty, FResourceCommon.sceneFileSuffix);
                if (!File.Exists(_finalPath))
                {
#if Japan
                    _finalPath = FResourceCommon.GetZipPath("StreamingResources/" + Info.LoadPath,string.Empty, FResourceCommon.sceneFileSuffix);
                    if (!File.Exists(_finalPath))
                    {
                        _finalPath = FResourceCommon.GetStreamingAssetsPath("StreamingResources/" + Name,FResourceCommon.sceneFileSuffix);
                    }
#else
                    _finalPath = PathUtils.GetStreamingAssetsPath("StreamingResources/" + Name, FResourceCommon.sceneFileSuffix);
#endif
                }
                AssetBundle _assetBundle = null;
                if (LoadAsync)
                {
                    AssetBundleCreateRequest _request = AssetBundle.LoadFromFileAsync(_finalPath);//避免卡顿,用异步     
                    yield return _request;
                    _assetBundle = _request.assetBundle;
                }
                else
                {
                    _assetBundle = AssetBundle.LoadFromFile(_finalPath);
                }


                yield return null;
                if (LoadAsync)
                {
                    AsyncOperation asy = SceneManager.LoadSceneAsync(name, Mode);
                    yield return asy;

                }
                else
                {
                    SceneManager.LoadScene(name, Mode);
                }

                ProcessState = FrameDef.AsyncState.SelfLoadEnd;
                afterLoad();
                yield return null;//必须延迟一帧，不然加载场景可能失败
                _assetBundle.Unload(false);
            }

        }

        void afterLoad()
        {
            ProcessComplete();
        }

    }
}


