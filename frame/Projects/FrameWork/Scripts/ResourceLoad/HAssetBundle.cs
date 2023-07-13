using FrameWork.Scripts.ResourceLoad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ResourceLoad
{
    [Serializable]
    public class HAssetBundle : HRes
    {
        private static AssetBundle mAssetBundle;
        [SerializeField]
        private static ManiFest m_AssetInfoData;
        /// <summary>
        /// ab包依赖信息
        /// </summary>
        private static Dictionary<string, string[]> mDependenciesMap = new Dictionary<string, string[]>();
        private static Dictionary<string, Dictionary<string, string>> mVariantMap = new Dictionary<string, Dictionary<string, string>>();
        public static Dictionary<string, Dictionary<string, string>> VariantMap
        {
            get
            {
                return mVariantMap;
            }
        }
        public static string GetABname(string assetPath)
        {
            return m_AssetInfoData.GetABnameByAssetPath(assetPath);
        }
        public static string[] GetDependencies(string ABName)
        {
            return m_AssetInfoData.GetDependencies(ABName).ToArray();
        }

        public static void ReleaseAssetBundleManifest()
        {
            mDependenciesMap.Clear();
            if (mAssetBundle != null)
            {
                mAssetBundle.Unload(true);
                m_AssetInfoData = null;
                if(VariantMap != null)
                    VariantMap.Clear();
            }
        }

        public List<string> DepList
        {
            get;
            set;
        }

        public AssetBundle AB
        {
            get;
            set;
        }

        public string ABName
        {
            get;
            set;
        }
        #region UNITY_EDITOR
        public static Dictionary<string, Dictionary<string, int>> mWhoRefMeMapAll = new Dictionary<string, Dictionary<string, int>>();
        public List<string> mWhoRefMeList = new List<string>();
        #endregion

        public HAssetBundle()
        {
        }

        public static void InitManifest(AssetBundle assetBundle = null)
        {
            if (m_AssetInfoData == null)
            {
                var path = PathManager.URL("manifest.assetbundle");
                if (assetBundle == null)
                    mAssetBundle = AssetBundle.LoadFromFile(path);
                else
                    mAssetBundle = assetBundle;
                m_AssetInfoData = mAssetBundle.LoadAsset("AssetBundleData") as ManiFest;
            }
        }
        public override void Init(string assetPath, string assetName, string resName, AssetType assetType, bool isAll,bool isDep)
        {
            base.Init(assetPath, assetName, resName, assetType, isAll, isDep);
            ABName = assetPath;
#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.BeginSample("HAssetBundle.Init");
#endif
            if (isDep == false && DepList == null)
            {
                if(m_AssetInfoData != null)
                {
#if ENABLE_PROFILER
                    UnityEngine.Profiling.Profiler.BeginSample("HAssetBundle.Init.11");
#endif
                    DepList = new List<string>();
                    string[] depList = GetDependencies(ABName);
                    if (depList != null && depList.Length > 0)
                    {
                        DepList.AddRange(depList);
                    }
#if ENABLE_PROFILER
                    UnityEngine.Profiling.Profiler.EndSample();
#endif
                    #region UNITY_EDITOR
                    for (int i = 0; i < DepList.Count; i++)
                    {
                        AddWhoRefMe(DepList[i], ABName);
                    }


                    if (mWhoRefMeMapAll.ContainsKey(assetPath))
                    {
                        AddWhoRefMe(mWhoRefMeMapAll[assetPath]);
                    }
                    #endregion
                }
                else
                {
                    Debug.LogError("请先初始化AssetbundleManifest，再加载Assetbundle!!!!");
                }
            }
#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        protected override IEnumerator CoLoadByAB(bool isSync, bool isAll, bool isPreLoad, Action<System.Object, ResRef> callback)
        {
            ABRequest.Load(this, isSync, AssetType);
            while (!ABRequest.IsComplete)
            {
                yield return null;
            }

            OnCompleted(AB, isPreLoad, callback);
        }

        public override void AddRef()
        {
            OnAddRef();
            AddDepRef();
        }

        private void OnAddRef()
        {
            RefCount++;
        }

        public void AddDepRef()
        {
            for (int i = 0; i < DepList.Count; i++)
            {      
                if (ResourcesManager.Instance.mResMap.ContainsKey(DepList[i]))
                {
                    HAssetBundle res = ResourcesManager.Instance.mResMap[DepList[i]] as HAssetBundle;
                    res.OnAddRef();
                }
            }
        }

        public override void Release(bool isImmediately = false)
        {
            //减少自身
            OnRelease(isImmediately);
            if(DepList != null)
            {
                //减少依赖
                for (int i = 0; i < DepList.Count; i++)
                {
                    if (ResourcesManager.Instance.mResMap.ContainsKey(DepList[i]))
                    {
                        HAssetBundle res = ResourcesManager.Instance.mResMap[DepList[i]] as HAssetBundle;
                        res.OnRelease(isImmediately);
                    }
                }
            }
          
        }

        private void OnRelease(bool isImmediately)
        {
            //这里判断大于0，是因为再ResourcesManager.ReleaseAll()的时候会释放掉所有HAB和其余HRes，当释放HRes的时候他会释放依赖的HAB，
            //但是此时可能已经再ReleaseAll的时候释放掉了对应的HAB，会出现RefCount已经等于0，此时就不要重复释放了。
            if (RefCount > 0)
            {
                RefCount--;
                if (RefCount <= 0)
                {
                    if(isImmediately)
                    {
                        ReleaseReal();
                    }
                    else
                    {
                        //放入回收站
                        ResourcesManager.Instance.AddRecycleBin(this);
                    }
                }
            }
        }

        public override void ReleaseReal()
        {
            //清理ab request缓存表
            ABRequest.Remove(ABName);
            //缓存列表中移除
            if (ResourcesManager.Instance.mResMap.ContainsKey(ResName))
            {
                ResourcesManager.Instance.mResMap.Remove(ResName);
            }
            //释放这个AB的所有资源,从这个AB中加载的资源Asset都将变为"null"
            if (AB != null)
            {
                AB.Unload(true);
                AB = null;
            }

            if (ResourcesManager.settingConfig.DEBUG_MODE)
                ResourcesManager.Instance.RemoveDebugInfo(ABName);
        }
        #region UNITY_EDITOR
        public static void AddWhoRefMe(string me, string refMe)
        {
            if (ResourcesManager.settingConfig.DEBUG_MODE)
            {
                if (!mWhoRefMeMapAll.ContainsKey(me))
                {
                    mWhoRefMeMapAll[me] = new Dictionary<string, int>();
                }

                if (!mWhoRefMeMapAll[me].ContainsKey(refMe))
                {
                    mWhoRefMeMapAll[me][refMe] = 1;
                }

                if (ResourcesManager.Instance.mResMap.ContainsKey(me))
                {
                    HAssetBundle hAB = ResourcesManager.Instance.mResMap[me] as HAssetBundle;
                    hAB.AddWhoRefMe(mWhoRefMeMapAll[me]);
                }
            }
        }

        public void AddWhoRefMe(Dictionary<string, int> refMeMap)
        {
            if (ResourcesManager.settingConfig.DEBUG_MODE)
            {
                mWhoRefMeList = refMeMap.Keys.ToList();
            }
        }
        #endregion
    }
}
