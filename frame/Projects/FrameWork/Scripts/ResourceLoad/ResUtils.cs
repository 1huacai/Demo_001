using ResourceLoad;
using System.IO;
using UnityEngine;

namespace Framework
{
    public static class ResUtils
    {
        public static string mSDKUpdatePath = null;
        /// <summary>
        /// 热更路径
        /// </summary>
        public static string SDKUpdatePath
        {
            get
            {
                if (string.IsNullOrEmpty(mSDKUpdatePath))
                {
                    mSDKUpdatePath = PathManager.RES_PERSISTENT_ROOT_PATH;
                }
                if (!mSDKUpdatePath.EndsWith("/"))
                {
                    mSDKUpdatePath += "/";
                }
                return mSDKUpdatePath;
            }
            set
            {
                mSDKUpdatePath = value;
            }
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        /// <param name="p_filename">文件路径带名和后缀</param>
        /// <param name="toStreamingAssetsPath">当沙盒目录找不到的情况下是否连接到包内目录</param>
        /// <param name="folderName">一般来说都是相对资源路径下的，只有Video是特殊的外部文件夹</param>
        /// <returns></returns>
        public static string ResPath(string p_filename, bool toStreamingAssetsPath = true, string folderName = "StreamingResources/")
        {
            if (p_filename.StartsWith("/"))
            {
                p_filename = p_filename.Substring(1);
            }
            p_filename = folderName + p_filename;
            var path = ResUtils.SDKUpdatePath + p_filename;
            if(toStreamingAssetsPath == true)
            {
                if (!File.Exists(path))
                {
                    path = GetStreamingAssetsPath(p_filename);
                }
            }
           
            return path;
        }

        /// <summary>
        /// 根据平台获得对应的包内流媒体文件路径
        /// </summary>
        private static string GetStreamingAssetsPath(string p_filename)
        {
            string path = string.Empty;
            string STREAMING_ASSET_PATH = "";
            if (Application.platform == RuntimePlatform.Android)
            {
                STREAMING_ASSET_PATH = Application.dataPath + "!assets";   // 安卓平台
            }
            else
            {
                STREAMING_ASSET_PATH = Application.streamingAssetsPath;  // 其他平台
            }
            path = STREAMING_ASSET_PATH + "/" + p_filename;
            return path;
        }

        /// <summary>
        /// 加载Resources下的预制体
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="root">父节点</param>
        /// <returns>实例化后的预制体</returns>
        public static GameObject LoadPrefabInstance(string path, Transform root = null)
        {
            GameObject resGO = Resources.Load<GameObject>(path);
            GameObject go = GameObject.Instantiate(resGO);
            if (root != null)
            {
                go.transform.SetParent(root);
            }
            //go.ResetPRS();
            resGO = null;
            return go;
        }
    }
}