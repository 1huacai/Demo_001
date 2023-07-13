using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.IO;

public class SpriteAtlasTool : ScriptableObject
{

    /// <summary>
    /// 创建SpriteAtlas
    /// </summary>
    /// <param name="fileName">文件名</param>
    public static void CreateSpriteAtlas(string fileName)
    {
        string _atlasPath = string.Format(AssetPostprocessorEditor.SPRITE_ATLAS_FILE_NAME, fileName);
        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(_atlasPath);
        if(atlas != null)
        {
            return;
        }
        atlas = new SpriteAtlas();
        // 设置参数 可根据项目具体情况进行设置
        SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
        {
            blockOffset = 1,
            enableRotation = false,
            enableTightPacking = false,
            padding = 2,
        };
        atlas.SetPackingSettings(packSetting);

        SpriteAtlasTextureSettings textureSetting = new SpriteAtlasTextureSettings()
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear,
        };
        atlas.SetTextureSettings(textureSetting);

        TextureImporterPlatformSettings andoridPlatformSetting = new TextureImporterPlatformSettings()
        {
            name = "Android",
            overridden = true,
            allowsAlphaSplitting = false,
            format = TextureImporterFormat.ASTC_4x4,
            crunchedCompression = true,
            textureCompression = TextureImporterCompression.Compressed,
            compressionQuality = (int)TextureCompressionQuality.Normal,
        };
        TextureImporterPlatformSettings iosPlatformSetting = new TextureImporterPlatformSettings()
        {
            name = "iPhone",
            overridden = true,
            allowsAlphaSplitting = false,
            format = TextureImporterFormat.ASTC_4x4,
            crunchedCompression = true,
            textureCompression = TextureImporterCompression.Compressed,
            compressionQuality = (int)TextureCompressionQuality.Normal,
        };
        TextureImporterPlatformSettings pcPlatformSetting = new TextureImporterPlatformSettings()
        {
            name = "Standalone",
            overridden = true,
            allowsAlphaSplitting = false,
            format = TextureImporterFormat.RGBA32,
            crunchedCompression = true,
            textureCompression = TextureImporterCompression.Uncompressed,
            compressionQuality = (int)TextureCompressionQuality.Normal,
        };
        atlas.SetPlatformSettings(andoridPlatformSetting);
        atlas.SetPlatformSettings(iosPlatformSetting);
        atlas.SetPlatformSettings(pcPlatformSetting);

        

        AssetDatabase.CreateAsset(atlas, _atlasPath);

        //添加文件夹
        string _atlasSpritePath = string.Format(AssetPostprocessorEditor.SPRITE_ATLAS_FOLDER_NAME, fileName);
        Object obj = AssetDatabase.LoadAssetAtPath(_atlasSpritePath, typeof(Object));
        atlas.Add(new[] { obj });

        SpriteAtlasUtility.PackAtlases(new SpriteAtlas[1] { atlas }, EditorUserBuildSettings.activeBuildTarget);
    }

    #region SpriteAtlas
    /// <summary>
    /// 检查是否新增加了AtlasSprites文件夹
    /// </summary>
    /// <param name="path"></param>
    public static void CheckAddAtlasSpritesFolder(string path)
    {
        if(!path.Contains(AssetPostprocessorEditor.SPRITE_ATLAS_PATH))
        {
            return;
        }
        System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
        bool IsValidFolder = AssetDatabase.IsValidFolder(path);
        if (assetType == typeof(DefaultAsset) && path.EndsWith("") && IsValidFolder)
        {
            var folderName = GetFloderName(path + "/");
            if (!folderName.Contains(@"Atlas"))
            {
                EditorUtility.DisplayDialog("", "文件夹名字需要包含Atlas", "确定");
                return;
            }
            CreateSpriteAtlas(folderName);
        }else
        {
            if(assetType == typeof(Texture2D))
            {
                CheckSpriteTexture(path);
            }
           
        }
    }

    /// <summary>
    /// 检查图集文件夹里面的图片发生变化了
    /// </summary>
    /// <param name="path"></param>
    private static void CheckSpriteTexture(string path)
    {
        var folderName = GetFileFoldName(path);
        string _atlasPath = string.Format(AssetPostprocessorEditor.SPRITE_ATLAS_FILE_NAME, folderName);
        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(_atlasPath);
        if (atlas == null)
        {
            return;
        }
        SpriteAtlasUtility.PackAtlases(new SpriteAtlas[1] { atlas }, EditorUserBuildSettings.activeBuildTarget);
    }

    


    /// <summary>
    /// 检查是否新删除了AtlasSprites文件夹
    /// </summary>
    /// <param name="path"></param>
    public static void CheckRemoveAtlasSpritesFolder(string path)
    {
        if (path.StartsWith(AssetPostprocessorEditor.SPRITE_ATLAS_PATH) == false)
        {
            return;
        }
        if (path.Contains(AssetPostprocessorEditor.SPRITE_ATLAS_PATH)) //移除图集散图文件夹
        {
            if (!path.ToLower().EndsWith("png") && !path.ToLower().EndsWith("jpg"))
            {
                var folderName = GetFloderName(path + "/");
                string _atlasPath = string.Format(AssetPostprocessorEditor.SPRITE_ATLAS_FILE_NAME, folderName);
                if (File.Exists(_atlasPath))
                {
                    AssetDatabase.DeleteAsset(_atlasPath);
                }
            }else
            {
                CheckSpriteTexture(path);
            }
            
        }
        //else if (path.Contains(AssetsImporter.SPRITE_ATLAS_SOURCE_PATH))//删除图集文件时尝试重新创建
        //{
        //    var folderName = GetFloderName(path + "/");
        //    SpriteAtlasTool.CreateSpriteAtlas(folderName);
        //}
    }

    /// <summary>
    /// 检查是否移动了AtlasSprites文件夹
    /// </summary>
    /// <param name="path"></param>
    public static void CheckMoveAtlasSpritesFolder(string path)
    {
        if (path.StartsWith(AssetPostprocessorEditor.SPRITE_ATLAS_PATH) == false)
        {
            return;
        }
        string moveForm = path;
        if (moveForm.Contains(AssetPostprocessorEditor.SPRITE_ATLAS_PATH))//图集散图改名时删除旧的图集文件
        {
            if (!moveForm.ToLower().EndsWith("png") && !moveForm.ToLower().EndsWith("jpg"))
            {
                var folderName = GetFloderName(path + "/");
                string _atlasPath = string.Format(AssetPostprocessorEditor.SPRITE_ATLAS_FILE_NAME, folderName);
                if (File.Exists(_atlasPath))
                {
                    AssetDatabase.DeleteAsset(_atlasPath);
                }
            }
        }
    }

    /// <summary>
    /// 返回指定路径的目录信息
    /// </summary>
    /// <example>http://www.cftea.com/c/2017/02/6812.asp</example>
    /// <param name="filePath">资源文件路径带名字(或有后缀)</param>
    /// <returns></returns>
    public static string GetDirectoryName(string filePath)
    {
        return FormatToUnityPath(Path.GetDirectoryName(filePath));
    }

    /// <summary>
    /// 格式化反斜杠
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string FormatToUnityPath(string path)
    {
        return path.Replace("\\", "/");
    }


    /// <summary>
    /// 获取资源所在文件夹名字
    /// </summary>
    /// <param name="filePath">资源文件路径带名字(或有后缀)</param>
    /// <returns></returns>
    public static string GetFloderName(string filePath)
    {
        filePath = GetDirectoryName(filePath);
        string[] names = filePath.Split('/');
        if (filePath.EndsWith("/"))
            return names[names.Length - 2];
        return names[names.Length - 1];
    }

    /// <summary>
    /// 获取文件的文件夹名字
    /// </summary>
    /// <param name="filePath">比如 xx/xx/xx.png</param>
    public static string GetFileFoldName(string filePath)
    {
        var folderPath = Path.GetDirectoryName(filePath);//  xx/xx
        folderPath = FormatToUnityPath(folderPath);
        string foldName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);
        return foldName;
    }

    #endregion
}