using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreFrameWork;
using CoreFrameWork.Scripts.Utils;
using CoreFrameWork.Event;
using FrameWork.App;
using CoreFrameWork.Utils;
using UnityEngine.Networking;

namespace ResourceFrameWork
{
    /// <summary>
    /// 资源加载处理
    /// </summary>
    public class FProcess : IDisposable
    {
        public bool Dirty = false;
        internal FRequest m_FRequest;

        /// <summary>
        /// 关键字为资源路径+资源类型
        /// </summary>
        internal Dictionary<InstanceKeyInfo, List<CallBackInfo>> CallBackDic
        {
            get
            {
                return m_FRequest.CallBackDic;
            }
            set
            {
                m_FRequest.CallBackDic = value;
            }
        }
        /// <summary>
        /// 依赖的所有ab包路径
        /// </summary>
        internal List<string> AllDependenceList;
        internal FResourceUnit ResUnit;

        /// <summary>
        /// 进程状态
        /// </summary>
        internal FrameDef.AsyncState State;
        /// <summary>
        /// 异步加载
        /// </summary>
        internal bool LoadAsync;
        internal bool isMainProcess;

        public FAssetBundleData AssetBundleData;

        internal string LoadPath
        {
            get
            {
                return m_FRequest.LoadPath;
            }
        }

        internal bool UsedAssetBundle
        {
            get
            {
                return m_FRequest.UsedAssetBundle;
            }
        }

        public FProcess(FRequest request,bool async)
        {
            m_FRequest = request;

            State = FrameDef.AsyncState.Hanging;
            LoadAsync = async;
            this.isMainProcess = request.isMainRequest;
            this.AssetBundleData = m_FRequest.AssetBundleData;
            ResUnit = new FResourceUnit(this, request.StayMemory);
            AllDependenceList = new List<string>();
         FResourcesManager.Inst.ResourceEvent.AddEvent(FrameDef.ResourcesProcessType.AssetLoaded, AssetLoaded);

        }
        public void CalculateAndLoadDependenceList(CallBack<string> call)
        {
            //处理依赖
            List<string> _AllDependencies = m_FRequest.AssetBundleData.AssetBundlePathList;
            if (_AllDependencies.Count > 0)
            {
                for (int i = 0; i < _AllDependencies.Count; i++)
                {
                    string _loadPath = _AllDependencies[i];
                    AllDependenceList.Add(_loadPath);
                    call(_loadPath);
                }
            }
        }
        public void Dispose()
        {
        }
        internal void TryLoad()
        {
            if (State == FrameDef.AsyncState.Ready)
            {
                if (LoadAsync)
                {
                    FResourcesManager.Inst.ResourceEvent.DispatchEvent(FrameDef.ResourcesProcessType.CoroutinCountChange, 1);
                }
                    FResourcesManager.Inst.StartCoroutine(StartLoad(LoadPath));
            }
        }
        /// <summary>
        /// 加载完成
        /// </summary>
        public void ProcessComplete()
        {
            if (LoadAsync)
            {
                FResourcesManager.Inst.ReduceCoroutinCount();
            }
            State = FrameDef.AsyncState.SelfLoadEnd;
            FResourcesManager.Inst.ResourceEvent.DispatchEvent(FrameDef.ResourcesProcessType.ProcessComplete, LoadPath, ResUnit);
            FResourcesManager.Inst.ResourceEvent.DispatchEvent(FrameDef.ResourcesProcessType.AssetLoaded, LoadPath, ResUnit);

        }
        private IEnumerator StartLoad(string loadPath)
        {
            State = FrameDef.AsyncState.Loading;

            if (!UsedAssetBundle)
            {
                //非打包模式
                if (!LoadAsync)
                {
                    this.ResUnit.InputResource(null, Resources.LoadAll(loadPath));
                }
                else
                {
                    yield return 2;
                    this.ResUnit.InputResource(null, Resources.LoadAll(loadPath));

                }
                //非打包模式算数量
                this.ResUnit.Size = 1;
                ProcessComplete();
            }
            else
            {
                string _finalPath = PathUtils.GetFinalPath(LoadPath, FResourcesManager.WebGL);
                ulong _offset = AssetBundleData.offset;
                AssetBundle _assetBundle = null;
                if (FResourcesManager.WebGL)
                {
                    //UnityWebRequest获取ab包
                    UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(_finalPath,0);
                    yield return www.SendWebRequest();

                    if (www.result!= UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(_finalPath);
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        _assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                        if (null == _assetBundle)
                        {
                            Log.Error("加载AB包失败");
                            ResUnit.InputResource(null, null);

                            ProcessComplete();
                        }
                        ResUnit.InputResource(_assetBundle, _assetBundle.LoadAllAssets());
                        ProcessComplete();
                    }
                }
                else
                {
                    if (!LoadAsync)
                    {
                        _assetBundle = AssetBundle.LoadFromFile(_finalPath, 0, _offset);
                        if (null == _assetBundle)
                        {
                            Log.Error("加载AB包失败");
                            ResUnit.InputResource(null, null);

                            ProcessComplete();
                        }
                        else
                        {
                            ResUnit.InputResource(_assetBundle, _assetBundle.LoadAllAssets());
                            ProcessComplete();
                        }
                    }
                    else
                    {
                        AssetBundleCreateRequest _request = AssetBundle.LoadFromFileAsync(_finalPath, 0, _offset);//避免卡顿,用异步     

                        yield return _request;

                        //出错处理
                        if (null == _request)
                        {
                            Log.Error("加载AB包失败 2 ");
                            ResUnit.InputResource(null, null);

                            ProcessComplete();
                            yield break;
                        }
                        else
                        {
                            _assetBundle = _request.assetBundle;

                            if (null == _assetBundle)
                            {
                                Log.Error("加载AB包失败 3 ");
                                ResUnit.InputResource(null, null);
                                ProcessComplete();
                            }
                            else
                            {
                                AssetBundleRequest _assetRequest = _assetBundle.LoadAllAssetsAsync();
                                yield return _assetRequest;
                                ResUnit.InputResource(_assetBundle, _assetRequest.allAssets);
                                ProcessComplete();
                            }
                            yield break;
                        }
                    }
                }
            }
        }

        private IEnumerator _unLoadAudio(AudioClip clip)
        {
            while (clip.loadState == AudioDataLoadState.Loading)
            {
                   yield return null;
            }
            ResUnit.TryToUnloadAssetBundle();
        }
        /// <summary>
        /// FResourceUnit 加载完成事件
        /// </summary>
        /// <param name="args"></param>
        private void AssetLoaded(params object[] args)
        {
            string _loadPath = args[0] as string;

            FResourceUnit _unit = args[1] as FResourceUnit;
            if (null != AllDependenceList && AllDependenceList.Count > 0)
            {
                for (int i = 0; i < AllDependenceList.Count; ++i)
                {
                    if (AllDependenceList[i] == _loadPath)
                    {
                        ResUnit.AddResourceRefToDic(_loadPath, _unit.BeUsed());
                        AllDependenceList.RemoveAt(i--);
                        break;
                    }
                }
            }
            if (State == FrameDef.AsyncState.SelfLoadEnd && null != AllDependenceList && 0 == AllDependenceList.Count)
            {
                AllDone();
                FResourcesManager.Inst.ResourceEvent.RemoveEvent(FrameDef.ResourcesProcessType.AssetLoaded, AssetLoaded);
                AllDependenceList = null;
            }
        }


        /// <summary>
        /// 整个加载任务完成，（其依赖的资源也加载完成了）
        /// 处理回调
        /// </summary>
        public void AllDone()
        {
            ResUnit.LoadComplete = true;
            State = FrameDef.AsyncState.Done;
            InstanceKeyInfo[] _keyArray = FUtils.DicKeysToArray(CallBackDic);
            if (null != _keyArray && _keyArray.Length > 0)
            {
                for (int i = 0; i < _keyArray.Length; ++i)
                {
                    InstanceKeyInfo _key = _keyArray[i];
                    List<CallBackInfo> _value = CallBackDic[_key];
                    for (int j = 0; j < _value.Count; ++j)
                    {
                        if (null != _value[j] && null != _value[j].callBack)
                        {
                            _value[j].callBack(ResUnit.GetInstance(_key));
                            _value[j] = null;
                        }
                    }
                }
            }
            if (isMainProcess)
            {
                if (null != ResUnit.MainAsset && ResUnit.MainAsset is AudioClip)
                {
                    FResourcesManager.Inst.StartCoroutine(_unLoadAudio(ResUnit.MainAsset as AudioClip));
                }
                else
                {
                    ResUnit.TryToUnloadAssetBundle();
                }
            }


            CallBackDic = null;
        }

        public void AddCallBack(Dictionary<InstanceKeyInfo, List<CallBackInfo>> dic)
        {
            if (null == dic)
            {
                return;
            }
            InstanceKeyInfo[] _keyArray = FUtils.DicKeysToArray(dic);
            if (null != _keyArray)
            {
                for (int i = 0; i < _keyArray.Length; ++i)
                {
                    InstanceKeyInfo _key = _keyArray[i];
                    List<CallBackInfo> _value = dic[_key];

                    AddCallBack(_key, _value);
                }
            }
        }

        public void AddCallBack(InstanceKeyInfo keyInfo, List<CallBackInfo> list)
        {
            if (null == list)
            {
                return;
            }

            if (null == CallBackDic)
            {
                CallBackDic = new Dictionary<InstanceKeyInfo, List<CallBackInfo>>();
            }

            InstanceKeyInfo _key = null;
            foreach (InstanceKeyInfo v in CallBackDic.Keys)
            {
                if (v.Path == keyInfo.Path && v.Type == keyInfo.Type)
                {
                    _key = v;
                    break;
                }
            }

            List<CallBackInfo> _value = null;
            if (null == _key)
            {
                _key = keyInfo;
                CallBackDic.Add(_key, new List<CallBackInfo>());
            }
            _value = CallBackDic[_key];

            for (int i = 0; i < list.Count; ++i)
            {
                if (!_value.Contains(list[i]))
                {
                    if (null != list[i])
                    {
                        _value.Add(list[i]);
                    }
                }
            }


        }

    }


}
