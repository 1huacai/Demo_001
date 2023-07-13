using System;

namespace ResourceFrameWork
{


    /// <summary>
    /// 资源文件详细信息记录器
    /// </summary>
    public class FAssetInfo
    {
        private string m_assetPath;
        private string m_name;
        private string m_nameWithOutSuffix;
        private string m_suffix;
        private string m_pathWithOutSuffix;

        internal bool UsedAssetBundle;
        /// <summary>
        /// 文件路径相对Resources 
        /// </summary>
        public string AssetPath
        {
            get
            {
                return m_assetPath;
            }
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string Name
        {

            get
            {
                if (null == m_name)
                {
                    m_name = FResourceCommon.GetFileName(AssetPath, false);
                }
                return m_name;

            }

        }
        public string NameWithOutSuffix
        {

            get
            {
                if (null == m_nameWithOutSuffix)
                {
                    m_nameWithOutSuffix = FResourceCommon.GetFileName(AssetPath, true);
                }
                return m_nameWithOutSuffix;
            }

        }

        /// <summary>
        /// 后缀名
        /// </summary>
        public string Suffix
        {
            get
            {
                if (null == m_suffix)
                {
                    m_suffix = FResourceCommon.GetFileSuffix(AssetPath);
                }

                return m_suffix;
            }
        }

        /// <summary>
        /// 不包含后缀的原始路径,从Resources文件夹内开始
        /// </summary>
        public string PathWithOutSuffix
        {
            get
            {
                if (null == m_pathWithOutSuffix)
                {
                    m_pathWithOutSuffix = AssetPath.Substring(0, AssetPath.Length - Suffix.Length - 1);
                }
                return m_pathWithOutSuffix;
            }
        }

        /// <summary>
        /// 包路径
        /// </summary>
        public string BundlePath
        {
            get
            {
                return null != AssetBundleData ? AssetBundleData.Path : string.Empty;
            }
        }

        /// <summary>
        /// 资源类型，同名文件时用
        /// </summary>    
        public Type AssetType;

        /// <summary>
        /// 是否为公共资源
        /// </summary>
        public bool IsCommonRes = false;

        /// <summary>
        /// 不丢包
        /// </summary>
        public bool IsSolid = false;
        /// <summary>
        ///  AssetBundle包信息
        /// </summary>
        public FAssetBundleData AssetBundleData;
        /// <summary>
        /// 包体大小=bundleSize*4
        /// </summary>
        public float Size = 1f;


        /// <summary>
        /// 返回ab包的读取路径
        /// </summary>
        public string LoadPath
        {

            get
            {
                return FResourceCommon.GetLoadPath(UsedAssetBundle, AssetPath, BundlePath);
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usedAssetBundle"></param>
        public FAssetInfo(string assetPath, bool usedAssetBundle)
        {
            m_assetPath = assetPath;
            UsedAssetBundle = usedAssetBundle;


        }

        //public AssetInfo() : this(true)
        //{

        //}







    }

}