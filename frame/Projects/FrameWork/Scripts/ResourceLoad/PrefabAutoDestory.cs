using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ResourceLoad
{
    public class PrefabAutoDestroy : MonoBehaviour
    {
        public HRes mRes;
        public ResRef mResRef;
        internal string mAssetPathInit;

        void OnDestroy()
        {
            if (mResRef != null)
            {
                mResRef.Release();
            }
        }

        void Copy(HRes res)
        {
            mRes = res;
            mResRef = new ResRef(res);
            mAssetPathInit = res.AssetPath;
        }

        //外部实例化带有PrefabAutoDestroy的对象，需要调用这个借口增加引用
        public void AddRef(HRes res)
        {
            Copy(res);
            if (mRes != null)
            {
                mRes.AddRef();
            }
        }
    }

}