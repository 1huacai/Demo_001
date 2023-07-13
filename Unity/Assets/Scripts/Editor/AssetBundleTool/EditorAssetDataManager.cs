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
public class EditorAssetDataManager
{
    public static EditorAssetDataManager Inst
    {
        get
        {
            if (null == _inst)
            {
                _inst = new EditorAssetDataManager();
            }
            return _inst;
        }
    }


    private static EditorAssetDataManager _inst;
    public Dictionary<string, EditorAssetData> AllAssetDataDic;
    /// <summary>
    /// 会被命名的有效资源
    /// </summary>
    public Dictionary<string, EditorAssetData> TargetAssetDataDic;
    public List<EditorAssetData> TargetAssetDataList {
        get
        {
            return TargetAssetDataDic.Values.ToList();
        }
    }
    /// <summary>
    /// 被鼠标选中的资源
    /// </summary>
    public List<EditorAssetData> SelectedAssetDataList;
    /// <summary>
    ///  会被命名的资源的主资源
    /// </summary>
    public List<EditorAssetData> BundledAssetDataList;

    public EditorAssetDataManager()
    {
        TargetAssetDataDic = new Dictionary<string, global::EditorAssetData>();
        SelectedAssetDataList = new List<EditorAssetData>();
        BundledAssetDataList = new List<EditorAssetData>();
    }
    public void Init()
    {

    }

    public void Add(EditorAssetData data)
    {
        if (null == data)
        {
            return;
        }
        EditorAssetData _data = null;
        if (!TargetAssetDataDic.TryGetValue(data.AssetPath, out _data))
        {
            TargetAssetDataDic.Add(data.AssetPath, data);
        }

        if(!BundledAssetDataList.Exists(v=>v.AssetPath== data.AssetPath))
        {
            BundledAssetDataList.Add(data);
        }
    }

    public void Remove(EditorAssetData data)
    {
        if (null == data)
        {
            return;
        }
        EditorAssetData _data = null;
        if (TargetAssetDataDic.TryGetValue(data.AssetPath, out _data))
        {
            TargetAssetDataDic.Remove(data.AssetPath);
        }

        if (BundledAssetDataList.Exists(v => v.AssetPath == data.AssetPath))
        {
            BundledAssetDataList.Remove(data);
        }
    }

    public void Clear()
    {
        TargetAssetDataDic.Clear();
        BundledAssetDataList.Clear();
        SelectedAssetDataList.Clear();
    }

    public void Dispose()
    {
        TargetAssetDataDic = null;
        BundledAssetDataList = null;
        SelectedAssetDataList = null;
        _inst = null;
    }
}



