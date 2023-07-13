using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

/// <summary>
/// 如果上级累计引用计数为1且，则与上级资源打在一起，否则打成单独的AB包，让上级资源记录与自己的引用关系
/// 如果下级资源只依赖于自己，则与自己打包在一起，否则只记录与下级资源的依赖关系
/// </summary>
public class EditorPackDef
{
    public static readonly string AllScripts = "AllScripts";
    public static readonly string AllInject = "AllInject";
    public static readonly string AllShaders = "AllShaders";
    public static readonly string ManiFest = "ManiFest";


    public static readonly string Inited = "Inited";
    public static readonly string Assets_0 = "Assets";
    public static readonly string Assets = "Assets/";
    public static readonly string AssetPathRoot = "Assets/Resources/"; 
    public static readonly string StreamingAssetsPathRoot = "Assets/StreamingAssets/";
    public static readonly string AssetTmpPathRoot = "Assets/tmp/";
    public static readonly string ZTextBackUpPath = "ZTextBackUp/";
    /// <summary>
    /// AB包后缀名
    /// </summary>
    public static readonly string AssetBundleSuffix = ".assetbundle";
    public static readonly string ABPathRoot_0 = "Assets/StreamingAssets/StreamingResources/";
    public static readonly string ABPathRoot = "StreamingAssets/StreamingResources/";
    public static readonly string StreamingResourcesPathRoot = "StreamingResources/";
    public static readonly string MD5Line = "==============================================================";

    public static readonly string Label_BuildAsFolderName = "BuildAsFolderName";
    public static readonly string Label_IsCommonAsset = "IsCommonAsset";
    public static readonly string Label_BuildSingle = "BuildSingle";
    public static readonly string Label_EditorOnly = "EditorOnly";
    public static readonly string Label_LoopSingle = "LoopSingle";

    public static readonly string ManiFestTextPath = "Resources/ManiFestText.txt";
    public static readonly string AssetBundleTextPath = "Resources/AssetBundleText.txt";

    public static readonly string ManiFestNestPath = "Resources/ManiFestNest.asset";
    public static readonly string AssetBundleNestPath = "Resources/AssetBundleNest.asset";

    public static readonly string AssetBundlePath = "Resources/AssetBundleData.asset";

    public static readonly string PackLogPath = "PackLog";

    /// <summary>
    /// "Assets/Resources/SGZ_ShaderVariants.shadervariants"
    /// </summary>
    public static readonly string ShaderVariantsPath = "Assets/Resources/ShaderCollector";



}



