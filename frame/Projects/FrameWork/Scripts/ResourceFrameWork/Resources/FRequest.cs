using System;
using System.Collections;
using System.Collections.Generic;

namespace ResourceFrameWork
{

    /// <summary>
    /// 资源请求类
    /// </summary>
    public class FRequest : IDisposable
    {
        /// <summary>
        /// ab包路径
        /// </summary>
        internal string LoadPath
        {
            get
            {
                return AssetBundleData.Path;
            }
        }
        internal InstanceKeyInfo m_keyInfo;
        internal bool UsedAssetBundle;

        internal int Priority =2;


        internal FAssetBundleData AssetBundleData;
        /// <summary>
        /// 关键字为资源路径+资源类型
        /// </summary>
        internal Dictionary<InstanceKeyInfo, List<CallBackInfo>> CallBackDic;

        internal bool isMainRequest;
        internal bool StayMemory = false;
        public bool Dirty = false;

        internal FRequest(InstanceKeyInfo keyInfo, bool usedAssetBundle, FAssetBundleData assetBundleData, bool stayMemory,  int priority = 2)
        {

            m_keyInfo = keyInfo;
            UsedAssetBundle = usedAssetBundle;
            AssetBundleData = assetBundleData;
            StayMemory = stayMemory;
            Priority = priority;

        }

        public void Dispose()
        {

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
            foreach( InstanceKeyInfo v in CallBackDic.Keys)
            {
                if(v.Path  == keyInfo.Path && v.Type == keyInfo.Type)
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
