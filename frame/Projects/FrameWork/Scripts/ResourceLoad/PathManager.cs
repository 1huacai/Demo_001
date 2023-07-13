using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ResourceLoad
{
    public class PathManager
    {
        private static readonly StringBuilder str_builder = new StringBuilder(256);

        #region 路径定义

        /// <summary>
        /// 本地assetdatabase加载的路径
        /// </summary>
        public static string RES_LOCAL_ASSETDATABASE_RELATIVE_PATH
        {
            get
            {
                if (ResourcesManager.settingConfig != null)
                {
                    return ResourcesManager.settingConfig.RES_LOCAL_ASSETDATABASE_RELATIVE_PATH;
                }

                return "";
            }
        }

        public static string RES_STREAM_ROOT_PATH
        {
            get
            {
                return Application.streamingAssetsPath;
            }
        }

        /// <summary>
        /// 沙盒资源根路径
        /// </summary>
        /// 
        private static string persistentPath;
        public static string RES_PERSISTENT_ROOT_PATH
        {
            get
            {
                if (!string.IsNullOrEmpty(persistentPath))
                {
                    return persistentPath;
                }

                if (ResourcesManager.settingConfig != null)
                {
                    string rootPath = null;
                    rootPath = Application.persistentDataPath;

                    if (string.IsNullOrEmpty(ResourcesManager.settingConfig.RES_PERSISTENT_RELATIVE_PATH))
                    {
                        persistentPath = rootPath;
                    }
                    else
                    {
                        persistentPath = rootPath + "/" + ResourcesManager.settingConfig.RES_PERSISTENT_RELATIVE_PATH;
                    }

                    return persistentPath;
                }

                return "";
            }
        }

        /// <summary>
        /// SDK热更新目录
        /// </summary>
        public static string RES_SDK_UPDATE_ROOT_PATH
        {
            get;
            set;
        }

        #endregion

        public static string GetRuntimePlatform()
        {
            string str = "";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    {
                        str = "Android";
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    {
                        str = "IOS";
                    }
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    {
                        str = "Windows";
                    }
                    break;
            }

            return str;
        }


        private static string ConstPath = Application.dataPath.Replace("Assets", "");
        //就是为了解决Assetdatabase需要传入资源后缀名才能加载的问题
        public static string GetExtension(string assetPath, HRes res)
        {
            if (res == null)
            {
                return "";
            }
            List<string> extensions = res.GetExtesions();
            if (extensions.Count == 1)
            {
                //某些资源只有一种扩展，就不用走下面的匹配
                return extensions[0];
            }
            //找到资源所在目录下的所有文件，然后依次比较名字，是否相同，然后得到扩展名（扩展名注意还要再extensions中才行，解决那种同名，但是不同后缀的问题）
            for (int i = 0; i < extensions.Count; i++)
            {
                str_builder.Clear();
                str_builder.Append(ConstPath);
                str_builder.Append(assetPath);
                str_builder.Append(extensions[i]);
                string tmpPath = str_builder.ToString();
                if (File.Exists(tmpPath))
                {
                    return extensions[i];
                }
            }
            return "";
        }
        /// <summary>
        /// 获取完整热更路径（热更可能不存在）
        /// </summary>
        /// <param name="assetPath">资源相对路径</param>
        /// <returns>完整热更路径</returns>
        public static string GetUpdateResPath(string assetPath)
        {
            var config = ResourcesManager.settingConfig;
            str_builder.Clear();
            //先判断热更新目录
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                    {
                        str_builder.Append(RES_SDK_UPDATE_ROOT_PATH);
                    }
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    {
                        str_builder.Append(RES_PERSISTENT_ROOT_PATH);
                    }
                    break;
            }

            str_builder.Append('/');
            str_builder.Append(assetPath);
            return str_builder.ToString();
        }

        /// <summary>
        /// URL大小写问题，测试的接口有：AssetBundle.LoadFromFileAsync 和 VideoPlayer.url 在Android上使用时候要区分大小的，PC上不会区分。
        /// </summary>
        /// <param name="assetPath">带后缀资源路径</param>
        /// <param name="res"></param>
        /// <returns></returns>
        public static string URL(string assetPath, HRes res = null)
        {
            string tmpPath;
            if (ResourcesManager.settingConfig.webGL)
            {
                return @"http://192.168.24.64/StreamingAssets/StreamingResources/"+assetPath;
            }
#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.BeginSample("PathManager.URL");
#endif
            var loadMode = ResourcesManager.Instance.LoadMode;
            if (loadMode == ResourceLoadMode.eAssetResource)
            {
                str_builder.Clear();
                str_builder.Append(RES_LOCAL_ASSETDATABASE_RELATIVE_PATH);
                str_builder.Append('/');
                str_builder.Append(assetPath);
                tmpPath = str_builder.ToString();
                string extention = GetExtension(tmpPath, res);
                str_builder.Clear();
                str_builder.Append(tmpPath);
                str_builder.Append(extention);
            }
            else if (loadMode == ResourceLoadMode.eAssetbundle)
            {
                tmpPath = ResourcesManager.Instance.GetUpdateResPath(assetPath);
                if (!string.IsNullOrEmpty(tmpPath))
                {
                    return tmpPath;
                }
                else
                {
                    var config = ResourcesManager.settingConfig;
                    //更新目录没有，那么走流式目录
                    str_builder.Clear();
                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            {
                                str_builder.Append(RES_STREAM_ROOT_PATH);
                                tmpPath = config.RES_StreamingAssets_PATH;
                            }
                            break;
                        case RuntimePlatform.IPhonePlayer:
                        case RuntimePlatform.OSXPlayer:
                        case RuntimePlatform.WindowsPlayer:
                            {
                                str_builder.Append(RES_STREAM_ROOT_PATH);
                                tmpPath = config.RES_StreamingAssets_PATH;
                            }
                            break;
                        case RuntimePlatform.OSXEditor:
                        case RuntimePlatform.WindowsEditor:
                            {
                                str_builder.Append(RES_STREAM_ROOT_PATH);
                                tmpPath = config.RES_StreamingAssets_PATH;

                            }
                            break;
                    }

                    if (!string.IsNullOrEmpty(tmpPath))
                    {
                        str_builder.Append('/');
                        str_builder.Append(tmpPath);
                    }

                    str_builder.Append('/');
                    str_builder.Append(assetPath);
                }
            }
            tmpPath = str_builder.ToString();
#if ENABLE_PROFILER
            UnityEngine.Profiling.Profiler.EndSample();
#endif
            return tmpPath;
        }
    }
}

