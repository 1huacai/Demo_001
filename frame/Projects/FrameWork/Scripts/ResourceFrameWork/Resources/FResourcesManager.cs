using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Object = UnityEngine.Object;
using CoreFrameWork;
using CoreFrameWork.Scripts.Utils;
using CoreFrameWork.Event;
using UnityEngine.Networking;

namespace ResourceFrameWork
{
    /// <summary>
    /// 1 放在工程目录Assets\Resources\下
    /// 2 挂在scene中名为ResourcesManager的gameObject上
    /// 加载出来的资源非主资源不保存AB包，根据FResourceUnit的引用来卸载资源
    /// </summary>
    public class FResourcesManager : MonoSingleton<FResourcesManager>, IDisposable
    {
        public Event ResourceEvent = new Event();
        private static readonly object locker = new object();


        #region 字段

        /// <summary>
        /// 调试模式
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// 是否通过assetbundle加载资源
        /// </summary>
        public static bool UsedAssetBundle = false;
        public static bool WebGL = false;
        /// <summary>
        /// 
        /// </summary>
        public static FrameDef.eCompressType CompressType = FrameDef.eCompressType.LZMA;
        /// <summary>
        /// 交叉引用的AB包释放规则，目前只有无引用图片会释放AB包
        /// </summary>
        public FrameDef.eCrossRefBundleReleaseType CrossRefBundleReleaseType = FrameDef.eCrossRefBundleReleaseType.ReleaseAssetBundleAndAsset;

        /// <summary>
        /// 最大协程数
        /// </summary>
        public int MaxCoroutineCount = 64;

        /// <summary>
        /// 当前协程数
        /// </summary>
        private int CurrentCoroutineCount;

        /// <summary>
        /// 当前已缓存的资源内存大小
        /// </summary>
        public float LoadMemorySize = 0;


        /// <summary>
        /// 0引用的缓存区大小 单KB (垃圾桶容积 20M)
        /// </summary>
        public float MaxZeroRefCacheSize = 20480;

        /// <summary>
        /// 运行时0引用的缓存区大小
        /// </summary>
        public int CurrentZeroRefCacheSize = 0;

        public int m_ReadyRequestCount = 0;
        public int ProcessCount = 0;
        public int ProcessingCount = 0;

        public string Line0 = "---------------------------------------------";
        public string AssetBundlePath = string.Empty;
        public string AassetState;
        public string Line1 = "---------------------------------------------";
        public string[] AssetsDebug;


        private FrameDef.eResManagerState mInit = FrameDef.eResManagerState.NoStart;

        /// <summary>
        /// 就绪请求字典
        /// </summary>
        internal readonly Dictionary<int, List<FRequest>> ReadyRequestDic = new Dictionary<int, List<FRequest>>();

        /// <summary>
        /// 处理字典，关键字为加载路径
        /// </summary>
        internal readonly Dictionary<string, FProcess> ProcessDic = new Dictionary<string, FProcess>();

        /// <summary>
        /// 场景处理链表
        /// </summary>
        internal readonly List<FSceneProcess> SceneProcessList = new List<FSceneProcess>();

        /// <summary>
        /// 资源回收站
        /// </summary>
        internal readonly List<FResourceUnit> RecycleBin = new List<FResourceUnit>();

        /// <summary>
        /// 加载的资源信息 关键字为加载路径
        /// </summary>
        internal readonly Dictionary<string, FResourceUnit> LoadedResourceUnitDic = new Dictionary<string, FResourceUnit>();

        #region 已加载资源处理

        #endregion

        /// <summary>
        /// 缓存链表
        /// </summary>
        internal readonly List<FResourceRef> CacheList = new List<FResourceRef>();

        /// <summary>
        /// 关键字为资源类型
        /// </summary>
        internal Dictionary<Type, Dictionary<string, FAssetInfo>> AssetInfoDic;


        /// <summary>
        /// 关键字为包路径
        /// </summary>
        internal Dictionary<string, FAssetBundleData> AssetBundleDataDic;

        private Dictionary<string, Shader> m_AllShaderDic = new Dictionary<string, Shader>();

        public List<string> WaitAssetList = new List<string>();

        private int TaskPriorityLength
        {
            get
            {
                if (-1 == m_taskPriorityLength)
                {
                    m_taskPriorityLength = FrameDef.TaskPriority.end.GetHashCode();
                }

                return m_taskPriorityLength;
            }
        }
        private int m_taskPriorityLength = -1;

        #endregion

        public class ResUnitStateData
        {
            public FrameDef.ResUnitStateType type = FrameDef.ResUnitStateType.end;
            public string info;

        }
        #region Update
        private void Update()
        {
            if (mInit < FrameDef.eResManagerState.Prepare)
            {
                return;
            }

            if (DebugMode && FResourceCommon.IsEditor())
            {

                m_ReadyRequestCount = ReadyRequestCount;
                ProcessCount = ProcessDic.Count;
                ProcessingCount = ProcessDic.Values.ToList().FindAll(v => v.State == FrameDef.AsyncState.Loading).Count;
                DebugLog(false);
            }

            int _readyRequestCount = ReadyRequestCount;

            if (_readyRequestCount > 0)
            {
                if (CurrentCoroutineCount <= MaxCoroutineCount)
                {
                    int _leftCount = MaxCoroutineCount - CurrentCoroutineCount;
                    int _targetCount = _readyRequestCount < _leftCount ? _readyRequestCount : _leftCount;
                    for (int i = 0; i < _targetCount; i++)
                    {
                        handleRequest();
                    }
                }
            }

            if (ProcessDic.Count > 0)
            {
                lock (locker)
                {
                    foreach (var item in ProcessDic)
                    {
                        if (item.Value.Dirty)
                        {
                            continue;
                        }
                        item.Value.TryLoad();
                    }
                }
            }

            if (SceneProcessList.Count > 0)
            {
                int _count = SceneProcessList.Count;
                for (int i = 0; i < _count; ++i)
                {
                    SceneProcessList[i].TryLoad();

                }
            }



            //释放资源
            if (CurrentZeroRefCacheSize >= MaxZeroRefCacheSize ||
                CrossRefBundleReleaseType == FrameDef.eCrossRefBundleReleaseType.ReleaseAssetBundleAndAsset)
            {
                ClearRecycleBin();
            }


        }
        /// <summary>
        /// 处理请求，生成处理任务，加入就绪任务列表
        /// </summary>
        private void handleRequest()
        {

            FRequest _Request = null;

            for (int i = 0; i < TaskPriorityLength; ++i)
            {
                if (ReadyRequestDic[i].Count > 0)
                {
                    _Request = ReadyRequestDic[i][0];

                    ReadyRequestDic[i].RemoveAt(0);
                    if (null != _Request && !_Request.Dirty)
                    {
                        break;
                    }
                }
            }

            if (_Request == null)
            {
                return;
            }

            FProcess _Process = CreateProcess(_Request, true);
            if (null != _Process)
            {
                if (!ProcessDic.ContainsKey(_Process.LoadPath))
                {
                    ProcessDic.Add(_Process.LoadPath, _Process);
                }
            }
        }

        /// <summary>
        /// 总 就绪请求数量
        /// </summary>
        private int ReadyRequestCount
        {
            get
            {
                int _count = 0;

                for (int i = 0; i < TaskPriorityLength; ++i)
                {
                    _count += ReadyRequestDic[i].Count;
                }
                return _count;
            }
        }
        #endregion

        #region 资源管理器初始化
        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        /// <param name="callback">成功后的回调</param>



        public IEnumerator Load(string finalPath, CallBack<AssetBundle, Object[]> call)
        {
            //UnityWebRequest获取ab包
            UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(finalPath, 0);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(finalPath);
                Debug.LogError(www.error);
            }
            else
            {
                var _assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                if (null == _assetBundle)
                {
                    Log.Error("加载AB包失败");
                }
                if (call != null)
                {
                    call(_assetBundle, _assetBundle.LoadAllAssets());
                }
            }
        }
        public IEnumerator Init(CallBack callback = null)
        {

            ClearAllAssets();
            ReadyRequestDic.Clear();
            ProcessDic.Clear();
            SceneProcessList.Clear();
            RecycleBin.Clear();
            LoadedResourceUnitDic.Clear();
            CacheList.Clear();

            for (int i = 0; i < TaskPriorityLength; ++i)
            {
                ReadyRequestDic.Add(i, new List<FRequest>());
            }

            if (mInit > FrameDef.eResManagerState.NoStart)
                yield break;

            ResourceEvent.AddEvent(FrameDef.ResourcesProcessType.ProcessComplete, ProcessCompleteEvnent);
            ResourceEvent.AddEvent(FrameDef.ResourcesProcessType.AddToRecycleBin, AddToRecycleBinEvent);
            ResourceEvent.AddEvent(FrameDef.ResourcesProcessType.CoroutinCountChange, CoroutinCountChangeEvent);

            //使用assetbundle打包功能
            if (UsedAssetBundle)
            {
                //读取依赖关系网
                string _finalPath = PathUtils.GetURLPath("StreamingResources/" + FrameDef.ManiFest.ToLower() + FResourceCommon.assetbundleFileSuffix,
                    string.Empty, string.Empty,FResourcesManager.WebGL);
                if (!File.Exists(_finalPath)&&!WebGL)
                {
                    _finalPath = PathUtils.GetStreamingAssetsPath("StreamingResources/" + FrameDef.ManiFest.ToLower(), "") + FResourceCommon.assetbundleFileSuffix;
                }
                if (WebGL)
                {
                    StartCoroutine(Load(_finalPath, (ab, asset) =>
                    {


                        FAssetBundleDataNest _assetBundleNest1 = ab.LoadAsset(FrameDef.AssetBundleNest) as FAssetBundleDataNest;
                        FManiFestDataNest _maniFestNest1 = ab.LoadAsset(FrameDef.ManiFestNest) as FManiFestDataNest;
                        parseAssetBundleData(_assetBundleNest1);
                        parseManiFest(_maniFestNest1);
                        ab.Unload(true);
                        ShaderVariantCollection _collection1 = null;
                        LoadObject(false, FrameDef.AllShaders, typeof(UnityEngine.GameObject), (matAll) =>
                        {
                            FResourceUnit _allShaderUnit = LoadedResourceUnitDic[FrameDef.AllShaders.ToLower() + FResourceCommon.assetbundleFileSuffix];
                            _allShaderUnit.StayMemory = true;

                            Object[] _allShader = _allShaderUnit.AllAssets;

                            for (int i = 0; i < _allShader.Length; ++i)
                            {
                                Shader _shader = _allShader[i] as Shader;
                                if (null != _shader)
                                {
                                    if (!m_AllShaderDic.ContainsKey(_shader.name))
                                    {
                                        m_AllShaderDic.Add(_shader.name, _shader);
                                    }
                                }
                                else
                                {
                                    if (_allShader[i] is ShaderVariantCollection)
                                    {
                                        _collection1 = _allShader[i] as ShaderVariantCollection;
                                    }
                                }
                            }
#if UNITY_5
                    Shader.WarmupAllShaders();
#elif UNITY_2017 || UNITY_2018
                    if (null != _collection)
                    {
                        _collection.WarmUp();
                        Debug.LogError("Log: ShaderVariantCollection WarmUp.");
                    }
#endif

                        MonoBehaviour.Destroy((matAll as FResourceRefKeeper).gameObject);

                            mInit = FrameDef.eResManagerState.Ready;

                            if (callback != null)
                            {
                                callback();
                            }

                        });


                        mInit = FrameDef.eResManagerState.Prepare;
                    }));
          
                }
                else
                {
                    AssetBundle _assetBundle = AssetBundle.LoadFromFile(_finalPath, 0);

                    FAssetBundleDataNest _assetBundleNest = _assetBundle.LoadAsset(FrameDef.AssetBundleNest) as FAssetBundleDataNest;
                    FManiFestDataNest _maniFestNest = _assetBundle.LoadAsset(FrameDef.ManiFestNest) as FManiFestDataNest;
                    parseAssetBundleData(_assetBundleNest);
                    parseManiFest(_maniFestNest);
                    _assetBundle.Unload(true);
                    ShaderVariantCollection _collection = null;
                    LoadObject(false, FrameDef.AllShaders, typeof(UnityEngine.GameObject), (matAll) =>
                    {
                        FResourceUnit _allShaderUnit = LoadedResourceUnitDic[FrameDef.AllShaders.ToLower() + FResourceCommon.assetbundleFileSuffix];
                        _allShaderUnit.StayMemory = true;

                        Object[] _allShader = _allShaderUnit.AllAssets;

                        for (int i = 0; i < _allShader.Length; ++i)
                        {
                            Shader _shader = _allShader[i] as Shader;
                            if (null != _shader)
                            {
                                if (!m_AllShaderDic.ContainsKey(_shader.name))
                                {
                                    m_AllShaderDic.Add(_shader.name, _shader);
                                }
                            }
                            else
                            {
                                if (_allShader[i] is ShaderVariantCollection)
                                {
                                    _collection = _allShader[i] as ShaderVariantCollection;
                                }
                            }
                        }
#if UNITY_5
                    Shader.WarmupAllShaders();
#elif UNITY_2017 || UNITY_2018
                    if (null != _collection)
                    {
                        _collection.WarmUp();
                        Debug.LogError("Log: ShaderVariantCollection WarmUp.");
                    }
#endif

                        MonoBehaviour.Destroy((matAll as FResourceRefKeeper).gameObject);

                        mInit = FrameDef.eResManagerState.Ready;

                        if (callback != null)
                        {
                            callback();
                        }

                    });


                    mInit = FrameDef.eResManagerState.Prepare;
                }
            }
            else
            {
                mInit = FrameDef.eResManagerState.Ready;
                if (callback != null)
                {
                    callback();
                }
            }

        }
        #region YMAL 解析关系表

        private void parseAssetBundleData(FAssetBundleDataNest nest)
        {
            if (null == nest)
            {
                return;
            }

            int _length = nest.m_list.Count;
            AssetBundleDataDic = new Dictionary<string, FAssetBundleData>(_length);

            for (int i = 0; i < _length; ++i)
            {
                AssetBundleDataDic[nest.m_list[i].Path] = nest.m_list[i];

            }
        }

        /// <summary>
        /// ManiFest内容转换为AssetInfo
        /// </summary>
        /// <param name="txt"></param>
        private void parseManiFest(FManiFestDataNest nest)
        {
            if (null == nest)
            {
                return;
            }

            int _length = nest.m_list.Count;

            AssetInfoDic = new Dictionary<Type, Dictionary<string, FAssetInfo>>(nest.m_typeList.Count);

            for (int i = 0; i < nest.m_typeList.Count; ++i)
            {
                Type _type = FResourceCommon.GetAssetType(nest.m_typeList[i]);
                AssetInfoDic[_type] = new Dictionary<string, FAssetInfo>(nest.m_typeCountList[i]);
            }


            for (int i = 0; i < _length; ++i)
            {
                FManiFestData _data = nest.m_list[i];

                FAssetInfo _info = new FAssetInfo(_data.m_AssetPath, UsedAssetBundle);
                string _fullName = _data.m_AssetType;
                _info.AssetType = FResourceCommon.GetAssetType(_fullName);

                _info.IsCommonRes = _data.m_Common;
                _info.IsSolid = _data.m_IsSolid;

                string _assetBundlePath = _data.m_AssetBundlePath;

                _info.AssetBundleData = AssetBundleDataDic[_assetBundlePath];
                _info.Size = _info.AssetBundleData.Size * 0.0009765625f;
                _info.AssetBundleData.IsSolid = _info.IsSolid;
                _info.AssetBundleData.IsCircleRef = _data.IsCircleRef;
                AssetInfoDic[_info.AssetType][_info.AssetPath] = _info;
            }
        }
        #endregion
        #endregion

        #region 事件
        private void CoroutinCountChangeEvent(params object[] args)
        {
            CoroutinCountChange((int)args[0]);
        }
        private void AddToRecycleBinEvent(params object[] args)
        {
            AddToRecycleBin(args[0] as FResourceUnit);
        }
        private void ProcessCompleteEvnent(params object[] args)
        {
            FResourceUnit _resUnit = args[1] as FResourceUnit;
            AddResourceUnit(_resUnit);
            #region 从等待列表中移除
            AssetLoad(args[0] as string);
            List<string> _assetPathList = _resUnit.AssetBundleData.AssetBundlePathList;
            for (int i = 0; i < _assetPathList.Count; ++i)
            {
                string _assetPath = _assetPathList[i];
                AssetLoad(_assetPath);
            }
            #endregion
        }
        internal void CoroutinCountChange(int count = 1)
        {

            CurrentCoroutineCount += count;

        }
        internal void AddCoroutinCount()
        {

            ++CurrentCoroutineCount;

        }
        internal void ReduceCoroutinCount()
        {

            --CurrentCoroutineCount;

        }
        #endregion

        #region Request

        internal FRequest FindRequest(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return null;
            }
            FRequest _request = null;

            List<FRequest> _list = null;
            for (int i = 0; i < TaskPriorityLength; ++i)
            {
                _list = ReadyRequestDic[i];

                for (int j = 0; j < _list.Count; ++j)
                {
                    if (_list[j].LoadPath == loadPath)
                    {
                        _request = _list[j];
                        break;
                    }
                }
            }

            return _request;
        }

        /// <summary>
        /// 生成请求
        /// </summary>
        /// <param name="loadPath">ab包路径</param>
        /// <param name="keyInfo"></param>
        /// <param name="handle"></param>
        /// <param name="LoadAsync"></param>
        /// <param name="isCache"></param>
        /// <param name="StayMemory"></param>
        /// <param name="Priority"></param>
        /// <returns></returns>
        internal FRequest CreateRequest(string loadPath, InstanceKeyInfo keyInfo, CallBack<object> handle, bool StayMemory = false, int Priority = 2)
        {

            FRequest _request = FindRequest(loadPath);
            if (null != _request)
            {
                _request.Priority = _request.Priority > Priority ? Priority : _request.Priority;
                if (null != handle && null != keyInfo)
                {
                    List<CallBackInfo> _list = new List<CallBackInfo> { new CallBackInfo( handle) };
                    _request.AddCallBack(keyInfo, _list);
                }
                return null;
            }
            else
            {
                FProcess _targetProcess = FindProcess(loadPath);
                if (null != _targetProcess && _targetProcess.State != FrameDef.AsyncState.Done)
                {
                    if (null != handle && null != keyInfo)
                    {
                        List<CallBackInfo> _list = new List<CallBackInfo> { new CallBackInfo(handle) };
                        _targetProcess.AddCallBack(keyInfo, _list);
                    }
                }
                else
                {
                    _request = new FRequest(keyInfo, UsedAssetBundle, GetFAssetBundleData(loadPath), StayMemory, Priority);
                    if (null != handle && null != keyInfo)
                    {
                        List<CallBackInfo> _list = new List<CallBackInfo> { new CallBackInfo(handle) };
                        _request.AddCallBack(keyInfo, _list);
                    }
                }
            }
            return _request;
        }
        #endregion

        #region Process
        internal bool ExistProcess(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return false;
            }
            return ProcessDic.ContainsKey(loadPath);
        }

        internal FProcess FindProcess(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return null;
            }
            FProcess _targetProcess = null;
            ProcessDic.TryGetValue(loadPath, out _targetProcess);

            return _targetProcess;
        }
        internal void RemoveProcess(FProcess process)
        {
            ProcessDic.Remove(process.LoadPath);
        }
        /// <summary>
        /// 创建加载进程
        /// </summary>
        /// <param name="request">请求数据</param>
        /// <param name="sync">是否异步</param>
        /// <returns></returns>
        internal FProcess CreateProcess(FRequest request, bool sync = true)
        {
            FProcess _targetProcess = FindProcess(request.LoadPath);
            bool StayMemory = request.StayMemory;

            if (null != _targetProcess && _targetProcess.State != FrameDef.AsyncState.Done)
            {
                _targetProcess.AddCallBack(request.CallBackDic);
            }
            else
            {
                //新建进程
                _targetProcess = new FProcess(request, sync);
                List<string> _AllDependencies = request.AssetBundleData.AssetBundlePathList;
                //处理依赖
                if (_AllDependencies.Count > 0)
                {
                    _targetProcess.CalculateAndLoadDependenceList((_loadPath) =>
                    {
                        if (sync)
                            LoadObjectAsync(_loadPath, (InstanceKeyInfo)null, null, FrameDef.TaskPriority.Highest, StayMemory,false);
                        else
                            LoadObjectSync(_loadPath, (InstanceKeyInfo)null, null, FrameDef.TaskPriority.Highest, StayMemory,false);
                    });
                }
                //可以准备加载资源了
                if (0 == _AllDependencies.Count)
                {
                    _targetProcess.State = FrameDef.AsyncState.Ready;
                }
            }
            return _targetProcess;
        }

        #endregion

        #region ResourceUnit
       /// <summary>
       /// 查找资源是否已经被加载存在内存中
       /// </summary>
       /// <param name="loadPath"></param>
       /// <returns></returns>
        internal FResourceUnit FindResourceUnit(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return null;
            }
            FResourceUnit _value = null;
            LoadedResourceUnitDic.TryGetValue(loadPath, out _value);
            return _value;
        }
        internal void AddResourceUnit(FResourceUnit _resUnit)
        {
            if (!LoadedResourceUnitDic.ContainsKey(_resUnit.LoadPath))
            {
                LoadMemorySize += _resUnit.Size;
                LoadedResourceUnitDic.Add(_resUnit.LoadPath, _resUnit);
            }
            else
                Debug.LogError("该资源已存在："+ _resUnit.LoadPath);
        }

        #endregion

        #region 回收
        /// <summary>
        /// 清理资源管理器
        /// </summary>
        public void ClearResouresManager()
        {
            ClearRecycleBin();
            ClearCacheAssets();
        }
        internal void AddToRecycleBin(FResourceUnit unit)
        {
            lock (locker)
            {
                if (null == unit)
                {
                    return;
                }

                int _size = (int)unit.Size;
                if (!RecycleBin.Contains(unit))
                {

                    RecycleBin.Add(unit);

                    CurrentZeroRefCacheSize += _size;
                }
            }

        }

        internal void RemoveFromeRecycleBin(string loadPath)
        {
            lock (locker)
            {
                FResourceUnit _unit = FindResourceUnit(loadPath);
                if (_unit != null)
                {
                    RemoveFromeRecycleBin(_unit);
                }
            }
        }

        internal void RemoveFromeRecycleBin(FResourceUnit unit)
        {
            lock (locker)
            {
                if (null == unit)
                {
                    return;
                }

                if (RecycleBin.Contains(unit))
                {
                    RecycleBin.Remove(unit);

                    CurrentZeroRefCacheSize -= (int)unit.Size;
                }

            }
        }
        internal void RemoveFromCacheList(FResourceRef keeper)
        {
            if (null != CacheList)
            {
                CacheList.Remove(keeper);
            }
        }
        internal void RemoveFromCacheList(FResourceUnit unit)
        {
            if (null != unit && null != CacheList && CacheList.Count > 0)
            {
                FResourceRef _ref = CacheList.Find(v => v.resUnit == unit);
                RemoveFromCacheList(_ref);
            }

        }
        public void ClearAllAssets()
        {

            using (var item = LoadedResourceUnitDic.GetEnumerator())
            {
                while (item.MoveNext())
                {
                    FResourceUnit value = item.Current.Value;
                    if (!value.StayMemory)
                    {
                        RemoveFromeRecycleBin(value);

                        LoadedResourceUnitDic.Remove(value.LoadPath);
                        value.Dispose();
                        LoadMemorySize -= value.Size;
                    }
                }
                item.Dispose();
            }



        }

        /// <summary>
        /// 清理缓存资源
        /// </summary>
        public void ClearCacheAssets()
        {

            for (int i = 0; i < CacheList.Count; ++i)
            {
                FResourceUnit _unit = CacheList[i].resUnit;
                RemoveFromCacheList(_unit);
                AddToRecycleBin(_unit);
                i--;

            }
        }

        /// <summary>
        /// 清理回收站
        /// </summary>
        public void ClearRecycleBin()
        {

            lock (locker)
            {

                if (0 == RecycleBin.Count)
                {
                    return;
                }
                for (int i = 0; i < RecycleBin.Count; ++i)
                {
                    FResourceUnit _unit = RecycleBin[i];

                    if (null != _unit)
                    {
                        if (_unit.ParentList.Count > 0)
                        {
                            bool _needBreak = false;
                            if (_unit.AssetBundleData.IsCircleRef)
                            {
                                for (int j = 0; j < _unit.ParentList.Count; ++j)
                                {
                                    if (_unit.ParentList[j].ReferenceCount != 0)
                                    {
                                        _needBreak = true;
                                    }
                                }
                            }
                            else
                            {
                                _needBreak = true;
                            }

                            if (_needBreak)
                            {
                                continue;
                            }
                        }

                        RemoveFromeRecycleBin(_unit);
                        i--;

                        foreach (KeyValuePair<string, FResourceUnit> pair in LoadedResourceUnitDic)
                        {
                            pair.Value.ResourceUnitDisposed(_unit);
                        }


                        if (0 == _unit.ReferenceCount)
                        {
                            LoadedResourceUnitDic.Remove(_unit.LoadPath);

                            LoadMemorySize -= _unit.Size;

                            _unit.Dispose();
                        }

                    }
                }



            }

        }
        #endregion

        #region 加载资源
        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="async">是否异步加载</param>
        /// <param name="assetPath">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <param name="handle">回调</param>
        /// <param name="priority">优先级</param>
        /// <param name="stayMemory">是否常驻内存</param>
        /// <param name="isMainRequest">是否是主资源</param>
        public void LoadObject(bool async, string assetPath, Type type, CallBack<object> handle, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal, bool stayMemory = false, bool isMainRequest = true)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }
            if (assetPath.Contains("$"))
            {
                assetPath = assetPath.Replace('$', '/');
            }
            FAssetInfo _assetInfo = GetInfo(assetPath, type);
            if (null == _assetInfo)
            {
                Debug.LogError("Manifest Can not find " + assetPath);
                handle?.Invoke(null);
                return;
            }
            if (type == typeof(Object))
            {
                type = _assetInfo.AssetType;
            }
            InstanceKeyInfo _keyInfo = new InstanceKeyInfo();
            _keyInfo.Init(assetPath, _assetInfo.Name, type);
            if (async)//异步
                LoadObjectAsync(_assetInfo.LoadPath, _keyInfo, handle, priority, stayMemory, isMainRequest);
            else
                LoadObjectSync(_assetInfo.LoadPath, _keyInfo, handle, priority, stayMemory, isMainRequest);

        }
        /// <summary>
        /// 同步加载处理进行中的异步资源
        /// </summary>
        private void SyncLoadCalculateAsync(string _key, InstanceKeyInfo keyInfo = null, CallBack<object> handle = null, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal, bool stayMemory = false, bool isMainRequest = false)
        {
            FProcess _process = FindProcess(_key);
            FRequest _request = FindRequest(_key);
            if (_request == null)
            {
                _request = CreateRequest(_key, keyInfo, handle, stayMemory, priority.GetHashCode());
                ReadyRequestDic[_request.Priority].Add(_request);
            }
            _request.Dirty = true;
            if (_process == null)
            {
                _process = CreateProcess(_request, false);
                _process.Dirty = true;
                _process.LoadAsync = false;
                if (null != _process)
                {
                    if (!ProcessDic.ContainsKey(_process.LoadPath))
                    {
                        ProcessDic.Add(_process.LoadPath, _process);
                    }
                }
            }
            _process.isMainProcess = _process.isMainProcess ? _process.isMainProcess : isMainRequest;
            if (_process.State <= FrameDef.AsyncState.SelfLoadEnd)
            {
                _process.State = FrameDef.AsyncState.Ready;
                _process.TryLoad();
            }
        }
        /// <summary>
        /// 资源已经加载直接读取资源
        /// </summary>
        /// <param name="mainLoadPath"></param>
        /// <param name="_unit"></param>
        /// <param name="keyInfo"></param>
        /// <returns></returns>
        private object LoadObjectFromDic(string abLoadPath, FResourceUnit _unit, InstanceKeyInfo keyInfo, bool isMainRequest = false)
        {
            object obj = null;
            _unit = FindResourceUnit(abLoadPath);
            if (_unit == null)
                return obj;
            RemoveFromeRecycleBin(_unit);
            //所有依赖的ab包
            for (int i = 0; i < _unit.AssetBundleData.AssetBundlePathList.Count; ++i)
            {
                string _key = _unit.AssetBundleData.AssetBundlePathList[i];

                FResourceUnit _childUnit = FindResourceUnit(_key);
                if (_childUnit != null)
                {
                    RemoveFromeRecycleBin(_childUnit);
                    _unit.AddResourceRefToDic(_key, _childUnit.BeUsed());
                }
                else
                {
                    SyncLoadCalculateAsync(_key);
                    Debug.LogError("异步资源未加载完成，换成手动加载并且标脏数据" + "（调用者：" + _unit.LoadPath + "），" + "子资源：" + _key);
                }
            }
            AssetLoad(abLoadPath);
            _unit.m_process.isMainProcess = _unit.m_process.isMainProcess ? _unit.m_process.isMainProcess : isMainRequest;
            if (keyInfo != null)
                obj = _unit.GetInstance(keyInfo);
            return obj;
        }
        /// <summary>
        /// 同步加载
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        private void LoadObjectSync(string abLoadPath, InstanceKeyInfo keyInfo, CallBack<object> handle, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal, bool stayMemory = false, bool isMainRequest = false)
        {
            object obj = null;
            FResourceUnit _unit = null;

            obj= LoadObjectFromDic(abLoadPath, _unit, keyInfo, isMainRequest);
            if (obj != null)
            {
                handle(obj);
                return ;
            }
            //----------------------------------------------------------------------------------------
            //建立主资源进程
            SyncLoadCalculateAsync(abLoadPath, keyInfo, handle, priority, stayMemory,isMainRequest);
        }
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="loadPath"></param>
        /// <param name="keyInfo"></param>
        /// <param name="handle"></param>
        /// <param name="priority"></param>
        /// <param name="stayMemory"></param>
        /// <param name="isMainRequest"></param>
        private void LoadObjectAsync(string loadPath, InstanceKeyInfo keyInfo, CallBack<object> handle, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal, bool stayMemory = false, bool isMainRequest = false)
        {
            AssetNeedLoad(loadPath);
            ///如果加载过的直接读取
            lock (locker)
            {
                FResourceUnit _unit = FindResourceUnit(loadPath);
                if (_unit != null)
                {
                    if (_unit.LoadComplete)
                    {
                        RemoveFromeRecycleBin(_unit);

                        for (int i = 0; i < _unit.AssetBundleData.AssetBundlePathList.Count; ++i)
                        {
                            string _key = _unit.AssetBundleData.AssetBundlePathList[i];
                            FResourceUnit _childUnit = FindResourceUnit(_key);
                            if (_childUnit != null)
                            {
                                RemoveFromeRecycleBin(_childUnit);
                                _unit.AddResourceRefToDic(_key, _childUnit.BeUsed());
                            }
                            else
                            {
                                Debug.LogError("异常情况：依赖资源已丢失，" + "（调用者：" + _unit.LoadPath + "），" + "子资源：" + _key);
                            }
                        }
                        AssetLoad(loadPath);

                        if (null != handle && null != keyInfo)
                        {
                            handle(_unit.GetInstance(keyInfo));
                        }
                        return;
                    }
                    else
                    {
                        AssetLoad(loadPath);
                        _unit.m_process.isMainProcess = _unit.m_process.isMainProcess ? _unit.m_process.isMainProcess : isMainRequest;
                        if (null != keyInfo)
                        {
                            _unit.m_process.AddCallBack(keyInfo, new List<CallBackInfo>(1) { new CallBackInfo(handle) });
                        }
                        return;
                    }

                }
            }
            //生成请求
            FRequest _req = CreateRequest(loadPath, keyInfo, handle, stayMemory, priority.GetHashCode());
            if (null != _req)
            {
                _req.isMainRequest = isMainRequest;
                //装入请求队列等待读取
                ReadyRequestDic[_req.Priority].Add(_req);
            }
        }

        #endregion

        #region 卸载资源
        public void UnloadAssets(List<string> pathList, bool flag, CallBack callBack)
        {
            if (null != pathList)
            {
                for (int i = 0; i < pathList.Count; ++i)
                {
                    UnloadAsset(pathList[i], flag, null);
                }
            }
            if (null != callBack)
            {
                callBack();
            }
        }
        private void UnloadAsset(string path, bool flag, CallBack callBack)
        {
            FAssetInfo _info = GetInfo(path, FResourceCommon.Object);
            FResourceUnit _unit = FindResourceUnit(_info.LoadPath);
            if (_unit!=null)
            {
                LoadedResourceUnitDic.Remove(_info.LoadPath);
                if (flag)
                {
                    RemoveFromeRecycleBin(_unit);
                    RemoveFromCacheList(_unit);
                    _unit.Dispose();
                }
                else
                {
                    _unit.UnloadAssetBundle(false);

                }
            }
        }

        /// <summary>
        /// 释放无用资源，效率低
        /// </summary>
        /// <param name="callBack"></param>
        public void UnloadUnusedAssets(CallBack callBack)
        {
            StartCoroutine(unloadUnusedAssets(callBack));

        }

        private IEnumerator unloadUnusedAssets(CallBack callBack)
        {
            AsyncOperation _as = Resources.UnloadUnusedAssets();//
            yield return _as;
            if (null != callBack)
            {
                callBack();
            }
        }

        #endregion

        #region 加载场景
        public void LoadScene(string name, CallBack handle, LoadSceneMode mode)
        {
            loadScene(name, handle, false, mode);

        }

        public void LoadSceneAsync(string name, CallBack handle, LoadSceneMode mode)
        {
            loadScene(name, handle, true, mode);
        }



        private void loadScene(string name, CallBack handle, bool async, LoadSceneMode mode)
        {

            List<CallBack> _callBackList = new List<CallBack>();
            _callBackList.Add(handle);
            FSceneProcess _process = new FSceneProcess(name, this, _callBackList, async, mode);

            SceneProcessList.Add(_process);
            _process.ProcessState = FrameDef.AsyncState.Ready;



        }

        #endregion

        #region 卸载场景


        public void UnloadScene(string name, CallBack handle)
        {
            StartCoroutine(unLoadScene(name, handle));
        }
        public void UnloadScene(Scene scene, CallBack handle)
        {
            StartCoroutine(unLoadScene(scene, handle));
        }
        public void UnloadScene(int sceneBuildIndex, CallBack handle)
        {
            StartCoroutine(unLoadScene(sceneBuildIndex, handle));
        }

        private IEnumerator unLoadScene(string name, CallBack handle)
        {

            AsyncOperation _asyncOperation = SceneManager.UnloadSceneAsync(name);
            yield return _asyncOperation;
            UnloadUnusedAssets(null);
            if (null != handle)
            {

                handle();
            }

        }
        private IEnumerator unLoadScene(Scene scene, CallBack handle)
        {

            AsyncOperation _asyncOperation = SceneManager.UnloadSceneAsync(scene);
            yield return _asyncOperation;
            UnloadUnusedAssets(null);
            if (null != handle)
            {
                handle();
            }

        }
        private IEnumerator unLoadScene(int sceneBuildIndex, CallBack handle)
        {

            AsyncOperation _asyncOperation = SceneManager.UnloadSceneAsync(sceneBuildIndex);
            yield return _asyncOperation;
            UnloadUnusedAssets(null);
            if (null != handle)
            {
                handle();
            }

        }
        #endregion

        #region 获取基础数据
        private bool isBaseRes(string assetPath)
        {

            if (assetPath == FrameDef.AllScripts ||
                assetPath == FrameDef.AllShaders)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 传入Type，更快查找
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public FAssetInfo GetInfo(string assetPath, Type type)
        {
            FAssetInfo _info = null;

            if (!UsedAssetBundle)
            {


                _info = new FAssetInfo(assetPath, UsedAssetBundle);
                _info.Size = 1;
                _info.AssetBundleData = new FAssetBundleData();
                _info.AssetBundleData.Path = assetPath;
                _info.AssetBundleData.AssetBundlePathList = null;

                //todo 同文件夹下同名不同后缀有风险
                _info.AssetType = FResourceCommon.Object;
                return _info;
            }

            if (isBaseRes(assetPath))
            {
                _info = new FAssetInfo(assetPath, UsedAssetBundle);
                _info.IsCommonRes = true;

                _info.AssetBundleData = new FAssetBundleData();
                _info.AssetBundleData.Common = true;
                _info.AssetBundleData.Path = assetPath.ToLower() + FResourceCommon.assetbundleFileSuffix;
                _info.AssetBundleData.AssetBundlePathList = null;

                _info.AssetType = typeof(UnityEngine.GameObject);
                return _info;
            }
            Dictionary<string, FAssetInfo> _dicValue = null;
            if (AssetInfoDic.TryGetValue(type, out _dicValue))
            {
                if (_dicValue.TryGetValue(assetPath, out _info))
                {
                    return _info;
                }
            }

            if (null == _info)
            {
                UnityEngine.Debug.LogError(assetPath + " 类型差异 " + type.ToString());
            }

            return _info;

        }
        /// <summary>
        /// 根据ab包路劲获取ab包信息
        /// </summary>
        /// <param name="loadPath"></param>
        /// <returns></returns>
        private FAssetBundleData GetFAssetBundleData(string loadPath)
        {
            FAssetBundleData _data = null;

            if (!UsedAssetBundle)
            {
                _data = new FAssetBundleData();
                _data.Path = loadPath;
                _data.AssetBundlePathList = new List<string>();

                return _data;
            }
            AssetBundleDataDic.TryGetValue(loadPath, out _data);
            return _data;
        }
        #endregion

        #region 获取shader
        public Shader GetShader(string name)
        {
            Shader _shader = null;

            if (FResourceCommon.IsEditor() || !UsedAssetBundle)
            {
                _shader = Shader.Find(name);

                if (_shader == null)
                {
                    UnityEngine.Debug.LogError("shader没找到,如果你是删除了res的打包最终版本,看效果请吧shader复制过来");
                }
            }
            else
            {

                if (!m_AllShaderDic.TryGetValue(name, out _shader))
                {
                    //UnityEngine.Debug.LogError("无法从allshader包里找到名为 " + name + " 的shader");
                    _shader = Shader.Find(name);

                }
            }
            return _shader;
        }
        #endregion

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();

            ClearAllAssets();
            ReadyRequestDic.Clear();
            ProcessDic.Clear();
            SceneProcessList.Clear();
            RecycleBin.Clear();
            LoadedResourceUnitDic.Clear();
            AssetInfoDic.Clear();
            AssetBundleDataDic.Clear();
            CacheList.Clear();
            Inst.ResourceEvent.RemoveEvent(FrameDef.ResourcesProcessType.ProcessComplete, ProcessCompleteEvnent);
            Inst.ResourceEvent.RemoveEvent(FrameDef.ResourcesProcessType.AddToRecycleBin, AddToRecycleBinEvent);
            Inst.ResourceEvent.RemoveEvent(FrameDef.ResourcesProcessType.CoroutinCountChange, CoroutinCountChangeEvent);
        }
        #endregion

        #region 编辑器工具显示
        /// <summary>
        /// 需要被加载的资源
        /// </summary>
        /// <param name="assetPath"></param>
        public void AssetNeedLoad(string assetPath)
        {

            if (FResourceCommon.IsEditor())
            {
                lock (locker)
                {
                    if (!WaitAssetList.Exists(v => v == assetPath))
                    {
                        WaitAssetList.Add(assetPath);
                    }
                }
            }
        }
        /// <summary>
        /// 资源已被加载
        /// </summary>
        /// <param name="assetPath"></param>
        public void AssetLoad(string assetPath)
        {

            if (FResourceCommon.IsEditor())
            {
                lock (locker)
                {
                    if (WaitAssetList.Exists(v => v == assetPath))
                    {
                        WaitAssetList.Remove(assetPath);
                    }
                }
            }
        }

        /// <summary>
        /// 调试输出,需要debugModel=true
        /// </summary>
        public string[] DebugLog(bool logOut)
        {
            List<ResUnitStateData> _ResUnitStateDataList = new List<FResourcesManager.ResUnitStateData>();

            List<FResourceUnit> _list = LoadedResourceUnitDic.Values.ToList();

            _list = _list.OrderBy(v => v.LoadPath).ToList();

            int _count = _list.Count;
            AssetsDebug = new string[_count];
            int index = 0;
            for (int i = 0; i < _count; ++i)
            {
                ResUnitStateData _data = new FResourcesManager.ResUnitStateData();
                index = i;
                FResourceUnit _unit = _list[i];
                string _loadPath = _unit.LoadPath;
                if (RecycleBin.Contains(_unit))
                {
                    AssetsDebug[index] = "[删除队列]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Delet;
                }
                else if (_unit.StayMemory)
                {
                    AssetsDebug[index] = "[常驻内存]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }
                else if (_unit.AssetBundle != null && _unit.ReferenceCount != 0)
                {
                    AssetsDebug[index] = "[正常引用]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }
                else if (_unit.AssetBundle == null && _unit.ReferenceCount > 0)
                {
                    AssetsDebug[index] = "[包已卸载]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }
                else
                {
                    AssetsDebug[index] = "[未知状态]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                }

                showState(_loadPath, AssetsDebug[index]);
            }

            if (!_list.Exists(v => v.LoadPath == AssetBundlePath))
            {
                AassetState = "不存在 " + AssetBundlePath;
            }

            WaitAssetList = WaitAssetList.OrderBy(v => v).ToList();

            return AssetsDebug;
        }

        private void showState(string loadPath, string state)
        {
            if (AssetBundlePath == loadPath)
            {
                AassetState = state;
            }

        }
        #endregion

        public void CheckAnimationClip(GameObject go, string assetPath)
        {
            if (!UsedAssetBundle && FResourceCommon.IsEditor())
            {
                var m_Animations = go.transform.GetComponents<Animation>();
                for (int i = 0; i < m_Animations.Length; i++)
                {
                    var m_Animation = m_Animations[i];
                    //var _temp = AnimationUtility.GetAnimationClips(m_Animation.gameObject);
                    int _clipCount = m_Animation.GetClipCount();
                    if (GetClipCount(m_Animation) != _clipCount)
                    {
                        Log.Error("检查动画组件是否missing:" + assetPath + "，GameObject名称：" + m_Animation.name);
                    }
                }
            }
        }

        private int GetClipCount(Animation ani)
        {
            int _count = 0;
            foreach (AnimationState item in ani)
            {
                _count++;
            }
            return _count;
        }

    }
}