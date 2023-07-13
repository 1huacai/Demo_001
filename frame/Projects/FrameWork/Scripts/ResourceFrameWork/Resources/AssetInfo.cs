using System.Collections.Generic;
using System;
using UnityEngine;
using CoreFrameWork.Scripts.Utils;

namespace ResourceFrameWork
{
    /// <summary>
    /// 资源文件详细信息记录器
    /// </summary>
    public class AssetInfo
    {

        /// <summary>
        /// 文件路径相对Resources 
        /// </summary>
        public string Path;

        /// <summary>
        /// 包体大小=bundleSize*4
        /// </summary>
        public int Size=10;


        /// <summary>
        /// 层数
        /// </summary>
        public int Level;
        /// <summary>
        /// GUID
        /// </summary>
        public string Guid;
        /// <summary>
        /// 资源类型，同名文件时用
        /// </summary>   
        public string ResType;

        /// <summary>
        /// 所有上级引用
        /// </summary>
        public List<string> AllUpDependencies = new List<string>();
        /// <summary>
        /// 所有下级依赖（不递归）
        /// </summary>
        public List<string> AllDownDependencies_without_reserve = new List<string>();

        /// <summary>
        /// 所有下级依赖 PathWithOutSuffix
        /// </summary>
        public string[] AllDependencies;
        public string md5 = "";
        /// <summary>
        /// 交叉引用计数
        /// </summary>
        public int CrossRefCount = 0;

        //独立打包
        public bool IsBuildEachPrefab = false;

        /// <summary>
        /// 是否为公共资源，上级引用计数大于1
        /// </summary>
        public bool IsCommonAsset = false;
        /// <summary>
        /// 是否为文件夹
        /// </summary>   
        public bool IsFolder;


        private string _directories = "-1";


        public bool isPrefab()
        {

            return Suffix.Equals("prefab", StringComparison.OrdinalIgnoreCase);

        }
        public bool isMaterial()
        {

            return Suffix.Equals("mat", StringComparison.OrdinalIgnoreCase);

        }
        public bool isShader()
        {

            return Suffix.Equals("shader", StringComparison.OrdinalIgnoreCase);

        }


        /// <summary>
        /// 返回从Assets开始长路径
        /// </summary>
        public string longPath
        {

            get
            {

                return "Assets/Resources/" + Path;

            }

        }
        /// <summary>
        /// 返回从Assets开始的AB包保存路径，包名为GUID
        /// </summary>
        public string SavePath
        {

            get
            {
                string tmp = "Assets/StreamingAssets/StreamingResources/" + Directories + Guid;
                return tmp;
            }


        }

        /// <summary>
        /// 返回从Assets开始的Ab包文件夹路径
        /// </summary>
        public string SaveDirectories
        {

            get
            {
                string tmp = "Assets/StreamingAssets/StreamingResources/" + Directories;
                return tmp;
            }


        }


        /// <summary>
        /// 返回计算机绝对路径，包名为GUID
        /// </summary>
        public string URLSavePath
        {

            get
            {
                string tmp = Application.dataPath + "/StreamingAssets/StreamingResources/" + Directories + Guid;
                return tmp;
            }


        }


        /// <summary>
        /// 返回ab包的读取路径
        /// </summary>
        public string LoadPath
        {

            get
            {
                string tmp = Directories + Guid;
                return tmp;
            }


        }

        /// <summary>
        /// 返回存放的文件夹路径
        /// </summary>
        public string Directories
        {

            get
            {
                if (_directories == "-1")
                {
                    string path = Path.Replace("\\", "/");
                    int index = path.LastIndexOf("/");
                    if (-1 == index)
                        return "";
                    _directories = path.Substring(0, index + 1);

                }

                return _directories;
            }


        }


        /// <summary>
        /// 不包含后缀的原始路径,从Resources文件夹内开始
        /// </summary>
        public string PathWithOutSuffix
        {
            get
            {
                //文件夹直接返回
                if (Suffix.Length == 0)
                    return Path;

                return Path.Substring(0, Path.Length - Suffix.Length - 1);
            }
        }


        /// <summary>
        /// 文件名
        /// </summary>
        public string Name
        {

            get
            {

                return PathUtils.GetFileName(Path, false);

            }

        }

        public string NameWithOutSuffix
        {

            get { return PathUtils.GetFileName(Path, true); }

        }

        /// <summary>
        /// 后缀名
        /// </summary>
        public string Suffix
        {

            get
            {

                return PathUtils.GetFileSuffix(Path);

            }

        }


    }

}