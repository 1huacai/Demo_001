using System;
using System.Collections.Generic;
using UnityEngine;


namespace ResourceFrameWork
{

    [PreferBinarySerialization]
    public class FAssetBundleDataNest : ScriptableObject
    {
        public List<FAssetBundleData> m_list = new List<FAssetBundleData>();


    }



}
