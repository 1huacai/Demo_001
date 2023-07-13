using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;







public enum EditorBundleType
{
    None = 0,
    Script,         // .cs
    Shader,         // .shader or build-in shader with name
    Font,           // .ttf
    Texture,        // .tga, .png, .jpg, .tif, .psd, .exr
    Material,       // .mat
    Animation,      // .anim
    Controller,     // .controller
    Model,            // .fbx
    TextAsset,      // .txt, .bytes
    Prefab,         // .prefab
    /// <summary>
    /// 场景
    /// </summary>
    Scene,       // .unity
    Audio, //   .mp3,.ogg
    Asset, //.asset

}

public enum AssetBigType
{
    None = 0,
    /// <summary>
    /// 基础类型
    /// </summary>
    Base,
    /// <summary>
    /// 复合类型 
    /// </summary>
    Compound,



}



//包名 路径 包含的资源的路径 大小 包版本号 资源MD5码 
public class EditorAssetBundleData
{


    /// <summary>
    /// AssetBundle长路径，例如：Assets/StreamingAssets/Textures/test.assetbundle
    /// </summary>
    public string AssetPath;
    /// <summary>
    /// AssetBundle加载路径，例如：Textures/test.assetbundle
    /// </summary>
    public string LoadPath;

    /// <summary>
    /// 所依赖的包路径
    /// </summary>
    public List<string> AssetBundlePathList;

    public int Size;
    public int Version;
    public uint Crc;//默认为0
    public uint CompressCrc;//默认为0
    public uint offset
    {
        get
        {
            if (!AssetBundleEditor.s_UseOffsetEncryption|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                return 0;
            }

            if (0 == m_offset)
            {

                int _size = LoadPath.GetHashCode();
                if (_size > 0)
                {
                    m_offset = (uint)(_size % 10 + 4);
                }
                else
                {
                    m_offset = 0x0F;
                }
                m_offset += 32;//加32，预留AB头
            }
            return m_offset;
        }
    }
    public uint m_offset;
    /// <summary>
    /// 是否为公共资源包
    /// </summary>
    public int Common;
    public int Encrypted;

    public EditorAssetBundleData() { }

    public EditorAssetBundleData(string path, bool common = false)
    {
        AssetPath = path;
        Common = common ? 1 : 0;
        LoadPath = AssetPath.Replace(EditorPackDef.ABPathRoot_0, string.Empty);




    }

    public void Init(string assetPath, List<string> assetPathList, int size, int version, uint crc, uint compressCrc)
    {
        AssetPath = assetPath;
        AssetBundlePathList = assetPathList;

        Size = size;
        Version = version;
        Crc = crc;
        CompressCrc = compressCrc;

    }

    public void SetAssetPathList(List<string> assetPathList)
    {
        AssetBundlePathList = assetPathList;
    }
    public void AddAssetPath(string path)
    {


        if (null == AssetBundlePathList)
        {
            AssetBundlePathList = new List<string>();
        }
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        if (!AssetBundlePathList.Contains(path))
        {


            AssetBundlePathList.Add(path);
        }
    }

    public void SetSize(int size)
    {
        Size = size;
    }
    public void SetVersion(int version)
    {
        Version = version;
    }

}
