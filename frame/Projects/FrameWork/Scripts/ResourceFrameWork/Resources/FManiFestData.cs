using System;

namespace ResourceFrameWork
{

    ////资源路径	资源类型	是否为公共资源(1 true, 0 false)	不丢包(1 true, 0 false)	资源所属AssetBundle包名
     [Serializable]
    public class FManiFestData
    {
        public string m_AssetPath;
        public string m_AssetType;
        public bool m_Common;
        public bool m_IsSolid;
        public bool IsCircleRef;
        public string m_AssetBundlePath;
 
 

    }

}
