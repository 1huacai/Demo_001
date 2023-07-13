using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResourceFrameWork
{

    public class HandleBase
    {
        internal bool UsedAssetBundle;
        public FAssetInfo Info;

        /// <summary>
        /// 加载路径
        /// </summary>
        public string LoadPath
        {
            get
            {
                return FResourceCommon.GetLoadPath(UsedAssetBundle, Info.AssetPath, Info.BundlePath);
            }
        }
    }

}
