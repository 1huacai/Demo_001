//#define Resource_UseLog

using System;
using System.Reflection;
using UnityEngine;

namespace ResourceFrameWork
{

    /// <summary>
    /// 资源管理器的常用类
    /// </summary>
    public class FResourceCommon
    {

        /// <summary>
        /// 默认的包路径
        /// </summary>
        public static string assetbundleFilePath = Application.dataPath + "/StreamingAssets/";
        /// <summary>
        /// 包后缀
        /// </summary>
        public const string assetbundleFileSuffix = ".assetbundle";

        public const string sceneFileSuffix = ".unity3d";
        public static Type Object = typeof(UnityEngine.Object);





        public static Type GetAssetType(string type)
        {
            Type _type = null;
            switch (type)
            {
                case "UnityEngine.Object":
                    {
                        _type = typeof(UnityEngine.Object);
                    }
                    break;

                case "UnityEngine.GameObject":
                    {
                        _type = typeof(UnityEngine.GameObject);
                    }
                    break;

                case "UnityEngine.Shader":
                    {
                        _type = typeof(UnityEngine.Shader);
                    }
                    break;
                case "UnityEngine.Material":
                    {
                        _type = typeof(UnityEngine.Material);
                    }
                    break;
                case "UnityEngine.AnimationClip":
                    {
                        _type = typeof(UnityEngine.AnimationClip);
                    }
                    break;
                case "UnityEngine.AudioClip":
                    {
                        _type = typeof(UnityEngine.AudioClip);
                    }
                    break;
                case "UnityEngine.Texture2D":
                    {
                        _type = typeof(UnityEngine.Texture2D);
                    }
                    break;
                case "UnityEngine.Font":
                    {
                        _type = typeof(UnityEngine.Font);
                    }
                    break;
                case "UnityEditor.Animations.AnimatorController":
                    {
                        _type = typeof(UnityEngine.RuntimeAnimatorController);
                    }
                    break;
                case "UnityEngine.TextAsset":
                    {
                        _type = typeof(UnityEngine.TextAsset);
                    }
                    break;






                default:
                    {
                        _type = typeof(UnityEngine.Object);
                    }
                    break;


            }




            return _type;
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
                return resPathName;
            else
            {
                resPathName = resPathName.Substring(index + 1, resPathName.Length - index - 1);
                return resPathName;
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
                int index2 = path.LastIndexOf(".");
                string _result = string.Empty;
                if (-1 == index && -1 == index2)//无 / ,无 .
                {
                    return filePath;
                }

                if (-1 != index && -1 == index2)//有 / ,无 .
                {
                    _result = path.Substring(index + 1, path.Length - index - 1);
                    return _result;
                }
                if (-1 == index && -1 != index2)//无 / ,有 .
                {
                    _result = path.Substring(0, index2 - 1);
                    return _result;
                }
                if (-1 != index && -1 != index2)//有 / ,有 .
                {
                    _result = path.Substring(index + 1, index2 - index - 1);
                    return _result;
                }

                return string.Empty;
            }
            else
            {
                string path = filePath.Replace("\\", "/");
                int index = path.LastIndexOf("/");
                if (-1 == index)
                {

                    return filePath;
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

                return "";
            }

            return path.Substring(0, index + 1);
        }

        /// <summary>
        /// 获得文件后缀名
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileSuffix(string resPathName)
        {
            int index = resPathName.LastIndexOf(".");
            if (index == -1)
            {
                return string.Empty;
            }
            else
            {
                string _name = resPathName.Substring(index, resPathName.Length - index);
                return _name;
            }
        }
        public static string GetFileSuffixWithoutPoint(string resPathName)
        {
            int index = resPathName.LastIndexOf(".");
            if (index == -1)
            {
                return resPathName;
            }
            else
            {
                string _name = resPathName.Substring(index + 1, resPathName.Length - index - 1);
                return _name;
            }
        }

        public static string DeletSuffix(string resPathName)
        {
            int index = resPathName.LastIndexOf(".");
            if (index == -1)
                return resPathName;
            else
            {
                string _name = resPathName.Substring(0, index);
                return _name;
            }
        }
        public static Type GetAssetType(string typeName, Assembly assembly)
        {
            if (assembly == null)
                return null;

            return assembly.GetType(typeName);

        }



        public static Type GetType(string TypeName)
        {

            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, etc.
            var type = Type.GetType(TypeName);

            // If it worked, then we‘re done here
            if (type != null)
                return type;

            // Get the name of the assembly (Assumption is that we are using
            // fully-qualified type names)
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

            // Attempt to load the indicated Assembly
            var assembly = Assembly.LoadWithPartialName(assemblyName);
            return GetAssetType(TypeName, assembly);
        }

        /// <summary>
        ///路径必须带后缀名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FrameDef.AssetBundleType GetBundleTypeByPath(string path)
        {
            FrameDef.AssetBundleType _type = FrameDef.AssetBundleType.None;
            switch (GetFileSuffix(path).ToLower())
            {
                case ".cs":
                case ".dll":
                    {
                        _type = FrameDef.AssetBundleType.Script;
                    }
                    break;
                case ".shader":
                    {
                        _type = FrameDef.AssetBundleType.Shader;
                    }
                    break;
                case ".ttf":
                    {
                        _type = FrameDef.AssetBundleType.Font;
                    }
                    break;
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".tga":
                case ".psd":
                case ".tif":
                case ".exr":
                    {
                        _type = FrameDef.AssetBundleType.Texture;
                    }
                    break;
                case ".mat":
                    {
                        _type = FrameDef.AssetBundleType.Material;
                    }
                    break;

                case ".anim":
                    {
                        _type = FrameDef.AssetBundleType.Animation;
                    }
                    break;
                case ".controller":
                    {
                        _type = FrameDef.AssetBundleType.Controller;
                    }
                    break;

                case ".fbx":
                    {
                        _type = FrameDef.AssetBundleType.FBX;
                    }
                    break;
                case ".txt":
                case ".bytes":
                    {
                        _type = FrameDef.AssetBundleType.TextAsset;
                    }
                    break;
                case ".prefab":
                    {
                        _type = FrameDef.AssetBundleType.Prefab;
                    }
                    break;
                case ".unity":
                    {
                        _type = FrameDef.AssetBundleType.Scene;
                    }
                    break;
                case ".mp3":
                case ".ogg":
                    {
                        _type = FrameDef.AssetBundleType.Audio;
                    }
                    break;
                default:
                    {

                    }
                    break;
            }
            return _type;
        }

        public static Type GetAssetBundleType(string sufix)
        {
            Type _type = typeof(UnityEngine.Object);
            switch (sufix)
            {

                case "shader":
                    {
                        _type = typeof(Shader);
                    }
                    break;
                case "txt":
                case "xml":
                case "bytes":
                    {
                        _type = typeof(TextAsset);
                    }
                    break;
                case "ttf":
                    {
                        _type = typeof(Font);
                    }
                    break;
                case "prefab":
                case "fbx":
                    {
                        _type = typeof(GameObject);
                    }
                    break;
                case "png":
                case "jpg":
                case "jpeg":
                case "bmp":
                case "tga":
                case "psd":
                case "tif":
                case "exr":
                    {
                        _type = typeof(Texture2D);
                    }
                    break;
                case "mat":
                    {
                        _type = typeof(Material);
                    }
                    break;

                case "anim":
                    {
                        _type = typeof(AnimationClip);
                    }
                    break;
                case "controller":
                    {
                        _type = typeof(RuntimeAnimatorController);
                    }
                    break;
                case "unity":
                    {

                    }
                    break;
                case "mp3":
                case "ogg":
                case "aiff":
                case "wav":
                    {
                        _type = typeof(AudioClip);
                    }
                    break;
                default:
                    {
                        _type = typeof(UnityEngine.Object);
                    }
                    break;
            }
            return _type;
        }

        public static string GetLoadPath(bool usedAssetBundle, string assetPath, string bundlePath)
        {
            return usedAssetBundle ? bundlePath : assetPath;
        }

        public static bool IsEditor()
        {
            return Application.isEditor;
        }
    }
}
