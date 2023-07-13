using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrameWork.Scripts.ResourceLoad
{
    [Serializable]
    public class ManiFest : ScriptableObject
    {
        #region FAssetBundleData信息

        public string[] Path;
        public ListData[] AssetBundlePathList;//依赖的包路径,若没有依赖则为null

        public void InitAssetBundleData(string[] key, ListData[] value)
        {
            Path = key;
            AssetBundlePathList = value;
        }
        public List<string> GetDependencies(string assetPath)
        {
            for (int i = 0; i < Path.Length; i++)
            {
                if (Path[i] == assetPath)
                    return AssetBundlePathList[i].Data;
            }
            return null;
        }
        #endregion

        #region FAssetInfo信息
        [SerializeField]
        private string[] m_key;
        [SerializeField]
        private string[] m_value;

        public string GetABnameByAssetPath(string assetPath)
        {
            for (int i = 0; i < m_key.Length; i++)
            {
                if (m_key[i] == assetPath)
                    return m_value[i];
            }
            return null;
        }
        public void InitMainFestData(string[] key, string[] value)
        {
            m_key = key;
            m_value = value;
        }
        #endregion

        #region ShaderData

        public List<string> ShaderName;
        public List<string> ShaderAsset;

        public void InitShader(Dictionary<string, string> data)
        {
            ShaderName = new List<string>();
            ShaderAsset = new List<string>();
            foreach (var item in data)
            {
                ShaderName.Add(item.Key);
                ShaderAsset.Add(item.Value);
            }
        }
        #endregion


    }
    [Serializable]
    public class ListData
    {
        public List<string> Data;
    }
}
