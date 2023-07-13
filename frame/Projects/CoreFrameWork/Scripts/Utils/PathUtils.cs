using System.IO;
using UnityEngine;

namespace CoreFrameWork.Scripts.Utils
{
    public static class PathUtils
    {
        /// <summary>
        /// 默认的包路径
        /// </summary>
        public static string assetbundleFilePath = Application.dataPath + "/StreamingAssets/";
        /// <summary>
        /// 包后缀
        /// </summary>
        public const string assetbundleFileSuffix = ".assetbundle";
        /// <summary>
        /// 场景路径
        /// </summary>
        public const string sceneFileSuffix = ".unity3d";
        /// <summary>
        /// SDK提供的外部资源文件路径，热更用
        /// </summary>
        public static string RESROOT;

       /// <summary>
       /// 获取资源最终路径
       /// </summary>
       /// <param name="loadPath"></param>
       /// <returns></returns>
        public static string GetFinalPath(string loadPath, bool isWebGL = false)
        {
            string _finalPath = GetURLPath("StreamingResources/" + loadPath, string.Empty, string.Empty, isWebGL);

            if (!File.Exists(_finalPath)&& !isWebGL)
            {
                _finalPath = GetStreamingAssetsPath("StreamingResources/" + loadPath, "");
            }
            return _finalPath;
        }
        /// <summary>
        /// 获得外部更新文件夹路径
        /// </summary>
        public static string GetURLPath(string p_filename, string PreFix, string Suffix, bool isWebGL = false)
        {
            string path = "";
            if (isWebGL)
            {
                return @"http://192.168.24.64/StreamingAssets/" + p_filename;
            }
            if (Application.platform == RuntimePlatform.OSXEditor)
                path = Application.persistentDataPath + "/" + p_filename;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
                path = RESROOT + p_filename;


            if (Application.platform == RuntimePlatform.WindowsEditor)
                path = Application.dataPath + "/StreamingAssets/" + p_filename;

            if (Application.platform == RuntimePlatform.WindowsPlayer)
                // path = Application.dataPath + "/StreamingAssets/" + p_filename;
                path = RESROOT + p_filename;
            if (Application.platform == RuntimePlatform.Android)
                path = RESROOT + p_filename;


            path = PreFix + path + Suffix;

            if (isWebGL)
            {
                path = @"file://" + path;
            }

            return path;
        }

        /// <summary>
        /// 根据平台获得对应的包内流媒体文件路径
        /// </summary>
        public static string GetStreamingAssetsPath(string p_filename, string suffix = "", bool isWebGL = false)
        {
            string path = string.Empty;

            string STREAMING_ASSET_PATH = "";


            if (Application.platform == RuntimePlatform.Android)
            {
                STREAMING_ASSET_PATH = Application.streamingAssetsPath;  // 其他平台
                //STREAMING_ASSET_PATH = Application.dataPath + "!assets";   // 安卓平台
            }
            else
            {
                STREAMING_ASSET_PATH = Application.streamingAssetsPath;  // 其他平台
            }


            path = STREAMING_ASSET_PATH + "/" + p_filename;
            if (isWebGL)
            {
                path = "file://" + path;
            }
            return path;
        }


        /// <summary>
        /// 获取资源文件名
        /// </summary>
        /// <param name="resPathName">传入的路径应为不带后缀格式</param>
        /// <returns></returns>
        public static string GetResourceName(string resPathName)
        {
            int index = resPathName.LastIndexOf("/");
            if (index == -1)
            {
                resPathName = resPathName.Split('.')[0];
                return resPathName;
            }
            else
            {
                return resPathName.Substring(index + 1, resPathName.Length - index - 1);
            }
        }

        /// <summary>
        /// 获得文件名
        /// </summary>
        /// <param name="filePath">实际的路径</param>
        /// <param name="removeSuffix">是否要返回后缀</param>
        /// <returns></returns>
        public static string GetFileName(string filePath, bool removeSuffix)
        {
            if (removeSuffix)
            {
                string path = filePath.Replace("\\", "/");
                int index = path.LastIndexOf("/");
                if (-1 == index)
                {
                    Log.Error("输入路径中没有/");
                    return "";
                }

                int index2 = path.LastIndexOf(".");
                if (-1 == index2)
                {
                    Log.Error("输入路径中没有.");
                    return "";
                }

                return path.Substring(index + 1, index2 - index - 1);
            }
            else
            {
                string path = filePath.Replace("\\", "/");
                int index = path.LastIndexOf("/");
                if (-1 == index)
                {
                    Log.Error("输入路径中没有/");
                    return "";
                }

                return path.Substring(index + 1, path.Length - index - 1);
            }
        }


        /// <summary>
        /// 获得文件路径中的文件夹路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFolder(string path)
        {
            path = path.Replace("\\", "/");
            int index = path.LastIndexOf("/");
            if (-1 == index)
            {
                Log.Warning("输入路径中没有文件夹");
                return "";
            }

            return path.Substring(0, index + 1);
        }

        /// <summary>
        /// 获得文件后缀名
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileSuffix(string filePath)
        {
            int index = filePath.LastIndexOf(".");
            if (-1 == index)
            {
                //  Log.Error("文件无后缀!");
                return "";
            }
            return filePath.Substring(index + 1, filePath.Length - index - 1);
        }
    }
}
