using System;
using System.Collections.Generic;


namespace ResourceFrameWork
{
    /// <summary>
    /// ab包信息 包名 路径 包含的资源的路径 大小 包版本号 资源MD5码 
    /// </summary>
    [Serializable]
    public class FAssetBundleData
    {
        public string Path;

        /// <summary>
        /// 依赖的包路径,若没有依赖则为null
        /// </summary>
        public List<string> AssetBundlePathList;

        /// <summary>
        /// 字节
        /// </summary>
        public int Size;
        public int Version;
        public uint Crc;//默认为0
        public uint CompressCrc;//默认为0
        public uint offset;//默认为0
        /// <summary>
        /// 是否为公共包
        /// </summary>
        public bool Common = false;
        /// <summary>
        /// 标记为不丢包
        /// </summary>
        public bool IsSolid = false;
        /// <summary>
        /// 是否存在循环引用
        /// </summary>
        public bool IsCircleRef = false;
        /// <summary>
        /// 是否被加密了
        /// </summary>
        public bool Encrypted;

        public FAssetBundleData()
        {

        }

        public void AddDependentAssetBundlePath(string assetBundlePath)
        {
            if(null == AssetBundlePathList)
            {
                AssetBundlePathList = new List<string>();
            }
            if(!AssetBundlePathList.Contains(assetBundlePath))
            {
                AssetBundlePathList.Add(assetBundlePath);
            }
        }

    }

}
