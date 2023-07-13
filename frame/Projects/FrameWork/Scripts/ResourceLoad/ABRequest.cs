using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ResourceLoad
{
    public class ABRequest
    {
        public class ABData
        {
            public AssetBundle mAB;
            public AssetBundleCreateRequest mRequest;
            public UnityWebRequest webRequest;
        }

        List<HAssetBundle> mABLoadList = new List<HAssetBundle>();
        static Dictionary<string, ABData> mABDataMap = new Dictionary<string, ABData>();
        private AssetType mAssetType; //目标资源类型
        private bool mIsSync; //是否是同步请求

        public bool IsComplete
        {
            get;
            private set;
        }

        public ABRequest()
        {
            IsComplete = false;
        }

        public void Load(HAssetBundle ab, bool isSync, AssetType assetType)
        {
#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.BeginSample("ABRequest.Load");
#endif
            mAssetType = assetType;
            mIsSync = isSync;

            if (ab == null)
            {
                Debug.LogError("ABRequest HAssetbundle is Null");
                return;
            }

            //引用计数
            mABLoadList.Clear();
            mABLoadList.Add(ab);

            if(ab.DepList != null)
            {
                for (int i = 0; i < ab.DepList.Count; i++)
                {
                    HAssetBundle depAB = ResourcesManager.Instance.Get<HAssetBundle>(ab.DepList[i], "", AssetType.eAB, false, true);
                    mABLoadList.Add(depAB);
                }
            }
           

            int len = mABLoadList.Count;
#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.BeginSample("ABRequest.CoLoad.111111." + len);
#endif

            if (!IsComplete)
            {
                ResourcesManager.Instance.StartCoroutine(CoLoad(mABLoadList));
            }
            //else
            //{
            //    IsComplete = true;
            //    //Debug.Log("AB Is Complete Direct Return!!!!");
            //}
#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.EndSample();
#endif

#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        public IEnumerator CoLoad(List<HAssetBundle> abList)
        {
            if (!IsComplete)
            {
                int len = mABLoadList.Count;

                //开启所有加载
                for (int j = 0; j < len; j++)
                {
                    if (ResourcesManager.Instance.mResMap.TryGetValue(mABLoadList[j].ResName, out HRes res2))
                    {
                        if (!res2.ABRequest.IsComplete)
                        {
                            LoadAB(mABLoadList[j]);
                        }
                        //else
                        //{
                        //    Debug.Log("dep AB Is Complete Direct Skip!!!!");
                        //}
                    }

                }
                //等待加载完成
                for (int k = 0; k < len; k++)
                {
                    ABData data;
                    if (ResourcesManager.settingConfig.webGL)
                    {

                        if (mABDataMap.TryGetValue(mABLoadList[k].ABName, out data))
                        {
                            if (data.webRequest != null)
                            {
                                if(!data.webRequest.isDone)
                                {
                                    yield return data.webRequest.SendWebRequest();
                                    while (!data.webRequest.isDone)
                                    {
                                        yield return null;
                                    }
                                    if (data.webRequest.isHttpError || data.webRequest.isNetworkError)
                                    {
                                        CoreFrameWork.Log.Error(data.webRequest.error);
                                    }
                                    AssetBundle ab = (data.webRequest.downloadHandler as DownloadHandlerAssetBundle).assetBundle;//转为AB包
                                    data.mAB = ab;
                                    mABLoadList[k].AB = data.mAB;
                                    mABLoadList[k].ABRequest.IsComplete = true;
                                }

                            }
                        }
                        else
                        {
                            Debug.LogError("mABDataMap is not find abname : " + mABLoadList[k].ABName);
                        }
                    }
                    else
                    {

                        if (mABDataMap.TryGetValue(mABLoadList[k].ABName, out data))
                        {
                            if (data.mRequest != null)
                            {
                                while (!data.mRequest.isDone)
                                {
                                    yield return null;
                                }

                                data.mAB = data.mRequest.assetBundle;
                                mABLoadList[k].AB = data.mAB;
                                mABLoadList[k].ABRequest.IsComplete = true;
                            }
                        }
                        else
                        {
                            Debug.LogError("mABDataMap is not find abname : " + mABLoadList[k].ABName);
                        }
                    }
                }

                IsComplete = true;
            }
        }

        private void LoadAB(HAssetBundle ab)
        {
            ABData abData = null;
            if (!mABDataMap.TryGetValue(ab.ABName, out abData))
            {
                abData = new ABData();
                mABDataMap[ab.ABName] = abData;
            }

            if (abData.mAB == null)
            {
                string url = PathManager.URL(ab.ABName);

                if (ResourcesManager.settingConfig.webGL)
                {
                    if (abData.webRequest == null)
                        abData.webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);//请求资源
                    return;
                }

                if (mIsSync)
                {
#if ENABLE_PROFILER
                    UnityEngine.Profiling.Profiler.BeginSample("ABRequest.CoLoadAB.11111");
#endif
                    if (abData.mRequest == null)
                    {
                        abData.mAB = AssetBundle.LoadFromFile(url, 0, ResourcesManager.settingConfig.ASSETBUNDLE_ENCRYPT);
                    }
                    else
                    {
                        abData.mAB = abData.mRequest.assetBundle;
                    }
                    ab.AB = abData.mAB;
                    ab.ABRequest.IsComplete = true;
#if ENABLE_PROFILER
                    UnityEngine.Profiling.Profiler.EndSample();
#endif
                }
                else
                {
#if ENABLE_PROFILER
                    UnityEngine.Profiling.Profiler.BeginSample("ABRequest.CoLoadAB.22222222");
#endif
                    if (abData.mRequest == null)
                    {
                        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(url, 0, ResourcesManager.settingConfig.ASSETBUNDLE_ENCRYPT);
                        abData.mRequest = request;
                    }
#if ENABLE_PROFILER
                    UnityEngine.Profiling.Profiler.EndSample();
#endif
                }
            }
        }

        public static void Remove(string abName)
        {
            if (mABDataMap.ContainsKey(abName))
            {
                mABDataMap.Remove(abName);
            }
        }
    }
}