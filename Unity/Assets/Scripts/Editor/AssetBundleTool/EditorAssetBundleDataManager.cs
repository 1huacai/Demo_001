using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;


/// </summary>
public class EditorAssetBundleDataManager
{
    public static EditorAssetBundleDataManager Inst
    {
        get
        {
            if (null == _inst)
            {
                _inst = new EditorAssetBundleDataManager();
            }
            return _inst;
        }
    }


    private static EditorAssetBundleDataManager _inst;
    /// <summary>
    /// 会被命名的有效资源
    /// </summary>
    public Dictionary<string, EditorAssetBundleData> AssetBundleDataDic;
    public List<EditorAssetBundleData> AssetBundleDataList
    {
        get
        {
            return new List<EditorAssetBundleData>(AssetBundleDataDic.Values);
        }
    }


    public EditorAssetBundleDataManager()
    {
        AssetBundleDataDic = new Dictionary<string, global::EditorAssetBundleData>();

    }

    public void Init()
    {

    }

    public void Add(EditorAssetData data, string assetbundleName)
    {
        if (null == data)
        {
            return;
        }

        if (!string.IsNullOrEmpty(data.FinalAssetBundleName))
        {
            EditorAssetBundleData _bundleData = null;
            string _key = string.IsNullOrEmpty(assetbundleName) ?
                data.FinalAssetBundleName.ToLower() : assetbundleName.ToLower();

            if (!AssetBundleDataDic.TryGetValue(_key, out _bundleData))
            {

                string _path = _key.Replace(EditorPackDef.AssetPathRoot, string.Empty).Insert(0, EditorPackDef.ABPathRoot_0);

                _bundleData = new EditorAssetBundleData(_path, data.IsCommonAsset);

                AssetBundleDataDic.Add(_key, _bundleData);
            }

            if (0 == _bundleData.Common && data.IsCommonAsset)
            {
                _bundleData.Common = 1;
            }

            for (int i = 0; i < data.DownDependenceDataList.Count; ++i)
            {
                if (data.FinalAssetBundleName != data.DownDependenceDataList[i].FinalAssetBundleName)
                {
                    _bundleData.AddAssetPath(data.DownDependenceDataList[i].FinalAssetBundleName.ToLower());
                }
            }

            data.AssetBundleData = _bundleData;
        }

    }

    public void Remove(EditorAssetBundleData data)
    {
        if (null == data)
        {
            return;
        }
        EditorAssetBundleData _data = null;
        if (AssetBundleDataDic.TryGetValue(data.AssetPath, out _data))
        {
            AssetBundleDataDic.Remove(data.AssetPath);
        }


    }

    public bool ContainsKey(string key)
    {
        return AssetBundleDataDic.ContainsKey(key);
    }
    public EditorAssetBundleData GetValue(string key)
    {
        return AssetBundleDataDic[key];
    }

    public void Clear()
    {
        AssetBundleDataDic.Clear();

    }

    public void Dispose()
    {
        AssetBundleDataDic = null;

        _inst = null;
    }
}



