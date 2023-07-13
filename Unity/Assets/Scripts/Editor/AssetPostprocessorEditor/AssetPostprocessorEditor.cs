using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine.Rendering;

public class AssetPostprocessorEditor : AssetPostprocessor
{
    private static string[] MUSIC_SUFFIX = new string[2] { ".mp3", ".ogg" };
    private static string[] TextureSuffix = new string[7] { ".png", ".jpg", ".tga", ".hdr", ".exr", ".cube", ".TGA" };
    private const string AUDIO_BG_PATH = "Assets/Resources/Sound/BG";
    private const string AUDIO_COMMON_PATH = "Assets/Resources/Sound/Common";
    private const string AUDIO_DIALOG_PATH = "Assets/Resources/Sound/Dialog";
    private const string TEXTURE = @"Assets/Resources/Textures";
    private const string SPRITE= @"Assets/Resources/Ui";

    public const string SPRITE_ATLAS_PATH = @"Assets/Resources/Ui/Atlas/";
    public const string SPRITE_ATLAS_SOURCE_PATH = "Assets/SpriteAtlasFiles/";
    public const string SPRITE_ATLAS_FILE_NAME = SPRITE_ATLAS_SOURCE_PATH + "{0}.spriteatlas";
    public const string SPRITE_ATLAS_FOLDER_NAME = SPRITE_ATLAS_PATH + "{0}";
    /// <summary>
    /// 模型导入默认设置的材质球
    /// </summary>
    private const string ART_IMPORT_DEFAULT_MAT_PATH = "Assets/Resources/Default/ImportDefaultMat.mat";
    private static Material ImportDefaultMat;
    #region 导入

    #region 音效
    private void OnPreprocessAudio()
    {
        if (System.Array.IndexOf(MUSIC_SUFFIX, Path.GetExtension(assetPath)) == -1)
        {
            Debug.LogError("你的音频格式不对：" + assetPath);
        }
        AudioImporter impor = (AudioImporter)assetImporter;
        bool isChange = false;
        AudioImporterSampleSettings settings = impor.defaultSampleSettings;
        if (settings.sampleRateSetting != AudioSampleRateSetting.OverrideSampleRate)
        {
            settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            isChange = true;
        }
        if (settings.sampleRateOverride != 22050)//人类语音和饿古典音乐的常见采用率
        {
            settings.sampleRateOverride = 22050;
            isChange = true;
        }

        impor.forceToMono = true;//clip.channels <= 1;//强制单声道，启用选项，unity将在打包之前把多声道音频混合成单声道
        if (assetPath.StartsWith(AUDIO_BG_PATH))
        {
            if (impor.preloadAudioData != false)
            {
                impor.preloadAudioData = false;//预加载，如果启用，则在加载场景时将预先加载音频剪辑；Streaming加载类型该选项不可用
                isChange = true;
            }
            if (settings.loadType != AudioClipLoadType.Streaming)
            {
                settings.loadType = AudioClipLoadType.Streaming;//动态解码声音。 此方法使用最少量的内存来缓冲从磁盘中逐步读取并在运行中解码的压缩数据。 解压缩发生在单独的流线程上，可以在Profiler窗口的音频窗格的"Streaming CPU"部分中监视其CPU使用情况。 即使没有加载任何音频数据，Streaming的剪辑也有大约200KB的消耗
                isChange = true;
            }
            if (impor.loadInBackground != true)
            {
                impor.loadInBackground = true;//后台加载，启用此选项，片段的加载将在单独的线程上延迟发生，而不会阻塞主线程；
                isChange = true;
            }
            if (settings.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                isChange = true;
            }
            if (settings.quality != 0.3f)
            {
                settings.quality = 0.3f;
                isChange = true;
            }
        }
        else if (assetPath.StartsWith(AUDIO_COMMON_PATH))
        {
            if (impor.preloadAudioData != true)
            {
                impor.preloadAudioData = true;//预加载，如果启用，则在加载场景时将预先加载音频剪辑；Streaming加载类型该选项不可用
                isChange = true;
            }
            if (settings.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;//在音频加载后马上解压缩。对较小的压缩声音使用此选项可以避免动态解压缩的性能开销，不过在加载时解压缩Vorbis编码的声音将使用大约十倍于内存的内存（对于ADPCM编码约为3.5倍），因此请勿对大文件使用此选项
                isChange = true;
            }
            if (impor.loadInBackground != false)
            {
                impor.loadInBackground = false;
                isChange = true;
            }
            if (settings.compressionFormat != AudioCompressionFormat.ADPCM)
            {
                settings.compressionFormat = AudioCompressionFormat.ADPCM;
                isChange = true;
            }
        }
        else if (assetPath.StartsWith(AUDIO_DIALOG_PATH))
        {
            if (impor.preloadAudioData != true)
            {
                impor.preloadAudioData = true;//预加载，如果启用，则在加载场景时将预先加载音频剪辑；Streaming加载类型该选项不可用
                isChange = true;
            }
            if (settings.loadType != AudioClipLoadType.CompressedInMemory)
            {
                settings.loadType = AudioClipLoadType.CompressedInMemory;//将声音压缩在内存中并在播放时解压缩。 此选项具有轻微的性能开销（特别是对于Ogg / Vorbis压缩文件），因此仅将其用于加载时解压缩将使用大量内存的较大的文件。 解压缩发生在混音器线程上，可以在探查器窗口的音频窗格中的"DSP CPU"部分进行监视
                isChange = true;
            }
            if (impor.loadInBackground != false)
            {
                impor.loadInBackground = false;
                isChange = true;
            }
            if (settings.compressionFormat != AudioCompressionFormat.ADPCM)
            {
                settings.compressionFormat = AudioCompressionFormat.ADPCM;
                isChange = true;
            }
        }

        impor.defaultSampleSettings = settings;
        if (isChange)
        {
            impor.SaveAndReimport();
        }

    }
    #endregion

    private void OnPreprocessAnimation()
    {
        //ModelImporter _importer = (ModelImporter)assetImporter;

    }
    private void OnPreprocessTexture()
    {
        TextureImporter _importer = (TextureImporter)assetImporter;
        if (_importer.textureType == TextureImporterType.Default)
        {
            SetUITextures();
        }
        if (_importer.assetPath.Contains(SPRITE_ATLAS_PATH))//sprite atlas
        {
            SetSpriteAtlas();
        }
    }
    #endregion

    #region 导出


    private void OnPostprocessAudio(AudioClip clip)
    {

    }
    #region 模型
    private void OnPostprocessModel(GameObject go)
    {
        if (assetPath.StartsWith("Assets/Resources") == false)
        {
            return;
        }
        ModelImporter model = this.assetImporter as ModelImporter;

        bool isChange = false;
        if (model.meshCompression != ModelImporterMeshCompression.Medium)
        {
            model.meshCompression = ModelImporterMeshCompression.Medium;
            isChange = true;
        }
        if (model.optimizeMeshVertices != true)
        {
            model.optimizeMeshVertices = true;
            isChange = true;
        }
        if (model.optimizeMeshPolygons != true)
        {
            model.optimizeMeshPolygons = true;
            isChange = true;
        }
        if (false)//要开read write
        {

            if (model.isReadable != true)
            {
                model.isReadable = true;
                isChange = true;
            }
        }

        if (model.materialImportMode != ModelImporterMaterialImportMode.None)
        {
            model.materialImportMode = ModelImporterMaterialImportMode.None;
            isChange = true;
        }

        if (model.materialLocation != ModelImporterMaterialLocation.External)
        {
            model.materialLocation = ModelImporterMaterialLocation.External;
            isChange = true;
        }

        if (model.importBlendShapes != false)
        {
            model.importBlendShapes = false;
            isChange = true;
        }
        if (model.importVisibility != false)
        {
            model.importVisibility = false;
            isChange = true;
        }
        if (model.importCameras != false)
        {
            model.importCameras = false;
            isChange = true;
        }
        if (model.importLights != false)
        {
            model.importLights = false;
            isChange = true;
        }

        if (true)//角色模型
        {
            if (assetPath.Contains("@"))//@代表有动画
            {
                if (model.importAnimation != true)
                {
                    model.importAnimation = true;
                    isChange = true;
                }


                //比如 把  f_a_001@w01_atk_02的avatar挂上f_a_001的avatar
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string[] files = fileName.Split(new char[1] { '@' }, System.StringSplitOptions.RemoveEmptyEntries);
                string mainFilePath = assetPath.Replace(fileName, files[0]);
                GameObject mainGO = AssetDatabase.LoadAssetAtPath(mainFilePath, typeof(GameObject)) as GameObject;
                if (mainGO == null)
                {
                    //EditorUtility.DisplayDialog("挂载Avatar", "找不到：" + mainFilePath, "确认");
                }
                else
                {
                    Avatar ava = mainGO.GetComponent<Animator>().avatar;
                    ModelImporterClipAnimation[] clips = model.clipAnimations;
                    if (model.sourceAvatar != ava)
                    {
                        model.sourceAvatar = ava;
                        isChange = true;
                    }

                }
            }
            else
            {
                if (model.importAnimation != false)
                {
                    model.importAnimation = false;
                    isChange = true;
                }
            }
        }

        if (model.importSettingsMissing == true)//只需要设置首次的属性
        {
            if (true)//场景模型
            {
                if (assetPath.Contains("@") == false)
                {
                    if (model.animationType != ModelImporterAnimationType.None)
                    {
                        model.animationType = ModelImporterAnimationType.None;
                        isChange = true;
                    }
                }

            }
            else
            {

            }

        }


        Renderer[] renders = go.GetComponentsInChildren<Renderer>(true);
        if (renders != null && renders.Length > 0)
        {
            foreach (Renderer ren in renders)
            {
                if (ren.sharedMaterials != null && ren.sharedMaterials.Length > 0)
                {
                    int len = ren.sharedMaterials.Length;
                    Material[] newMats = new Material[len];
                    for (int i = 0; i < len; i++)
                    {
                        Material sharedMaterial = ren.sharedMaterials[i];
                        if (sharedMaterial != null && sharedMaterial.shader.name.Equals("Standard"))
                        {
                            newMats[i] = ImportDefaultMat;
                        }
                    }
                    ren.sharedMaterials = newMats;//特意把材质球置空 
                }
                if (ren.allowOcclusionWhenDynamic != false)
                {
                    ren.allowOcclusionWhenDynamic = false;
                    isChange = true;
                }
                if (ren is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer smr = ren as SkinnedMeshRenderer;
                    if (smr.skinnedMotionVectors != false)
                    {
                        smr.skinnedMotionVectors = false;
                        isChange = true;
                    }
                }
                if (ren is MeshRenderer)
                {
                    MeshRenderer mr = ren as MeshRenderer;
                    if (mr.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion)
                    {
                        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                        isChange = true;
                    }
                }
            }
        }

        if (isChange)
        {
            model.SaveAndReimport();
        }
    }
    #endregion

    private void OnPostprocessMaterial(Material mat)
    {

    }
    private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
    {

    }
    private void OnPostprocessTexture(Texture2D texture)
    {

    }
    #region shader
    private static void OnPostprocessShader(string path)
    {
        Shader shader = AssetDatabase.LoadAssetAtPath(path, typeof(Shader)) as Shader;
        if (shader == null)
        {
            return;
        }

        ShaderImporter importer = AssetImporter.GetAtPath(path) as ShaderImporter;
        if (importer != null)
        {
            int propCount = shader.GetPropertyCount();
            List<string> texNames = new List<string>();
            List<Texture> textures = new List<Texture>();

            for (int j = 0; j < propCount; j++)
            {
                UnityEngine.Rendering.ShaderPropertyType type = shader.GetPropertyType(j);
                switch (type)
                {
                    case ShaderPropertyType.Texture:
                        string propName = shader.GetPropertyName(j);
                        if (importer.GetDefaultTexture(propName) != null &&
                            shader.GetPropertyTextureDefaultName(j) != importer.GetDefaultTexture(propName).name)
                        {
                            texNames.Add(propName);
                            textures.Add(null);
                        }

                        break;
                }
            }

            if (texNames.Count > 0)
            {
                importer.SetDefaultTextures(texNames.ToArray(), textures.ToArray());
                importer.SaveAndReimport();

                EditorUtility.DisplayDialog("警告", "Shader: " + path + " 有默认引用贴图。已经删掉引用了。", "Ok");
            }

            texNames.Clear();
            textures.Clear();
        }
    }
    #endregion

    private static Material GetDefaultMat()
    {
        if (ImportDefaultMat == null)
        {
            ImportDefaultMat = AssetDatabase.LoadAssetAtPath(ART_IMPORT_DEFAULT_MAT_PATH, typeof(Material)) as Material;
        }
        return ImportDefaultMat;
    }
    #region 材质球
    public static void OnPostprocessMateral(string path)
    {
        Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
        if (mat == null)
        {
            return;
        }

        // 场景 特效 使用的材质球
        if (true)
        {
            if (mat.enableInstancing == true)
            {
                mat.enableInstancing = false;//不使用GPU instancing
            }
        }

        if (path.Contains("Resources") == true && mat.shader.name.Equals("Standard"))
        {
            mat.shader = GetDefaultMat().shader;

        }


        //if (path.StartsWith(ART_EFFECT_PREFIX_PATH))//特效材质球
        //{
        //    string shaderName = mat.shader.name;
        //    if (path.Contains(ART_EFFECT_UI_MAT_NAME))//UI材质球
        //    {
        //        if (shaderName.ToLower().Contains("ui") == false)//说明UI材质球的shader使用了非UI的shader
        //        {
        //            Debug.LogError("UI材质球的shader使用了非UI的shader:" + path);
        //            //effectDeleteList.Add(path);
        //        }
        //    }
        //    else
        //    {
        //        if (shaderName.ToLower().Contains("ui") == true)//说明非UI类的使用了UI的shader
        //        {
        //            Debug.LogError("非UI类的使用了UI的shader:" + path);
        //            //effectDeleteList.Add(path);
        //        }
        //    }
        //}
    }
    #endregion
    #region 预制体
    void OnPostprocessPrefab(GameObject g)
    {
        string path = assetPath;
        if (path.StartsWith("Assets/Resources"))//检查是否有Standard
        {
            GameObject go = g;
            Renderer[] mrs = go.GetComponentsInChildren<Renderer>(true);
            if (mrs != null && mrs.Length > 0)
            {
                int len = mrs.Length;
                for (int i = 0; i < len; i++)
                {
                    Material mat = mrs[i].sharedMaterial;
                    if (mat != null && mat.shader.name.Equals("Standard"))
                    {
                        Debug.LogError("Standard型的shader:" + mrs[i].gameObject.name + ",go:" + go.name);
                        mrs[i].sharedMaterial = GetDefaultMat();
                        EditorUtility.DisplayDialog("警告", "你的预设包含了不合法的资源Standard型的shader\n" + path, "请改掉");
                        Selection.activeObject = mrs[i].gameObject;

                    }
                }

                BoxCollider boxc = go.GetComponent<BoxCollider>();
                if (boxc != null)
                {
                    Vector3 size = boxc.size;
                    if (size.x < 0 || size.y < 0 || size.z < 0)
                    {
                        Debug.LogError("错误：Collider的size不能使用负数，" + go.name);
                        //EditorUtility.DisplayDialog("警告", "Collider的size不能使用负数" + go.name, "请改掉");
                    }
                }
            }


            //特效
            if (true)
            {
                ParticleSystemRenderer[] psrs = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
                if (psrs != null && psrs.Length > 0)
                {
                    int len = psrs.Length;
                    for (int i = 0; i < len; i++)
                    {
                        ParticleSystemRenderer psr = psrs[i];
                        bool isReadWrite = false;
                        if (psr.renderMode == ParticleSystemRenderMode.Mesh)
                        {
                            isReadWrite = true;
                        }
                        string meshPath = AssetDatabase.GetAssetPath(psr.mesh);
                        ModelImporter model = ModelImporter.GetAtPath(meshPath) as ModelImporter;
                        if (model != null && model.isReadable != isReadWrite)
                        {
                            model.isReadable = isReadWrite;
                            model.SaveAndReimport();
                        }
                    }
                }
            }


            //role
            //if (path.StartsWith(ROLE_COMMON_PATH))
            //{
            //    Animator anim = go.GetComponent<Animator>();
            //    if(anim.cullingMode != AnimatorCullingMode.AlwaysAnimate)
            //    {
            //        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            //        PrefabUtility.SavePrefabAsset(go);
            //    }

            //}
        }
    }
    #endregion

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        int len = importedAssets.Length;
        int i = 0;
        bool isImportTexture = false;
        bool isImportMat = false;
        for (i = 0; i < len; i++)
        {
            string path = importedAssets[i];
            if (Path.GetExtension(path).Equals(".mat"))
            {
                isImportMat = true;
                OnPostprocessMateral(path);
            }
            if (Path.GetExtension(path).Equals(".shader"))
            {
                OnPostprocessShader(path);
            }
            SpriteAtlasTool.CheckAddAtlasSpritesFolder(path);
        }

        OnPostprocessAllAssetsSetLabel(importedAssets);
    }
    private static void OnPostprocessAllAssetsSetLabel(string[] importedAssets)
    {
        foreach (string str in importedAssets)
        {
            if (!str.Contains("/Resources"))
                continue;
            Object _target = AssetBundleTool.GetObjectByPath(str);
            if (AssetBundleTool.HasLabel(str, EditorPackDef.Label_LoopSingle))
            {
                continue;
            }
            #region 场景 Scenes
            if (str.Contains("/Resources/Art/Scenes"))
            {
                if (str.Contains(".prefab"))
                {
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                }
                continue;
            }
            #endregion
            #region 音效Audio
            if (str.Contains("/Resources/Audio"))
            {
                AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                continue;
            }
            if (str.Contains("/Resources/Sound"))
            {
                AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                continue;
            }
            #endregion
            #region 动画片段CinemaClips
            if (str.Contains("/Resources/Art/CinemaClips"))
            {
                if (str.Contains(".prefab") || _target is AnimationClip)
                {
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                }
                continue;
            }
            #endregion
            #region 配置表ConfigByte
            if (str.Contains("/Resources/ConfigByte"))
            {
                continue;
            }
            #endregion   
            #region 特效Effect
            if (str.Contains("/Resources/Art/Effect"))
            {
                if (str.Contains(".prefab"))
                {
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                }
                if ((_target is Texture || _target is Sprite) && (str.Contains("/Resources/Art/Effect/TexturesShow")))
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                continue;
            }
            #endregion
            #region 角色Role
            if (str.Contains("/Resources/Art/Role"))
            {
                if (str.Contains(".prefab"))
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                continue;
            }
            #endregion
            #region 单图 Textures 
            if (str.Contains("/Resources/Textures"))
            {
                AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                continue;
            }
            #endregion            
            #region 图集 UI 
            //if (str.Contains("/Resources/Ui"))
            //{
            //    if (str.Contains("..Sprite") || str.Contains(".."))
            //    {
            //        continue; ;
            //    }
            //    if (str.Contains("_RGB") || str.Contains("_Alpha") || str.Contains("_Mat") || _target is Material)
            //    {
            //        AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildAsFolderName);
            //        continue;
            //    }
            //    continue; ;
            //}
            if (str.Contains("/Resources/Ui/Atlas/"))
            {
                if (_target is DefaultAsset)
                {
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_IsCommonAsset);
                    continue;
                }
                continue; ;
            }
            #endregion
            #region 预制TSUprefabs
            if (str.Contains("/Resources/TSUprefabs"))
            {
                if (str.Contains(".prefab"))
                    AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                continue; ;
            }
            #endregion
            #region shader
            if (str.Contains("AllShaders.prefab"))
            {
                AssetBundleTool.AddLabel(_target, EditorPackDef.Label_BuildSingle);
                continue;
            }
            #endregion

            AssetBundleTool.ClearLabel(_target);
        }
    }
    #endregion

    #region 图片
    private static Dictionary<string, string> s_IllegalUITexture = new Dictionary<string, string>();

    private static Dictionary<string, string> s_TextureSuffixDeletes = new Dictionary<string, string>();

    /// <summary>
    /// 处理UI贴图
    /// </summary>
    private void SetUITextures()
    {
        TextureImporter impor = (TextureImporter)assetImporter;
        //处理ETC2 像素要求
        if (IsDivisibleOf4(impor) == false)
        {
            if (s_IllegalUITexture.ContainsKey(assetPath) == false)
            {
                (int width, int height) = GetTextureImporterSize(impor);
                s_IllegalUITexture.Add(assetPath, assetPath + ",width:" + width + ",height:" + height);
            }
            return;
        }
        //格式筛选 todo
        if (System.Array.IndexOf(TextureSuffix, Path.GetExtension(assetPath)) == -1)
        {
            if (s_TextureSuffixDeletes.ContainsKey(assetPath) == false)
            {
                s_TextureSuffixDeletes.Add(assetPath, assetPath);
            }
            return;
        }

        bool _isChange = false;

        if (impor.textureType != TextureImporterType.Default)
        {
            impor.textureType = TextureImporterType.Default;
            _isChange = true;
        }

        if (impor.isReadable != false)
        {
            impor.isReadable = false;
            _isChange = true;
        }

        if (impor.mipmapEnabled != false)
        {
            impor.mipmapEnabled = false;
            _isChange = true;
        }

        if (impor.sRGBTexture != true)
        {
            impor.sRGBTexture = true;
            _isChange = true;
        }

        if (impor.alphaIsTransparency != true)
        {
            impor.alphaIsTransparency = true;
            _isChange = true;
        }

        if (impor.textureCompression != TextureImporterCompression.CompressedHQ)
        {
            impor.textureCompression = TextureImporterCompression.CompressedHQ;
            _isChange = true;
        }

        if (impor.npotScale != TextureImporterNPOTScale.None)
        {
            impor.npotScale = TextureImporterNPOTScale.None;
            _isChange = true;
        }

        if (impor.wrapMode != TextureWrapMode.Clamp)
        {
            impor.wrapMode = TextureWrapMode.Clamp;
            _isChange = true;
        }

        bool isSetTextrePlatformSetting = SetTextrePlatformSetting(impor, true);
        if (isSetTextrePlatformSetting == true)
        {
            _isChange = true;
        }


        if (_isChange)
        {
            impor.SaveAndReimport();
        }
    }
    /// <summary>
    /// 处理图集
    /// </summary>
    private void SetSpriteAtlas()
    {
        TextureImporter impor = (TextureImporter)assetImporter;
        //(int width, int height) = GetTextureImporterSize(impor);
        //if (width >= 350 || height >= 350)
        //{
        //    //if (illegalUITexture.ContainsKey(assetPath) == false)
        //    //{
        //    //    illegalUITexture.Add(assetPath, assetPath + ",width:" + width + ",height:" + height);
        //    //}
        //    Debug.LogError("图集里面的图片尺寸不对，过大了，" + assetPath + ", width:" + width + ",height:" + height);
        //}

        //impor.importSettingsMissing  判断是否第一次导出
        bool isChange = false;
        if (impor.textureType != TextureImporterType.Sprite)
        {

            impor.textureType = TextureImporterType.Sprite;
            isChange = true;
        }

        if (impor.spriteImportMode != SpriteImportMode.Single)
        {
            impor.spriteImportMode = SpriteImportMode.Single;
            isChange = true;
        }

        if (impor.isReadable != false)
        {
            impor.isReadable = false;
            isChange = true;
        }

        if (impor.mipmapEnabled != false)
        {
            impor.mipmapEnabled = false;
            isChange = true;
        }

        if (impor.sRGBTexture != true)
        {
            impor.sRGBTexture = true;
            isChange = true;
        }

        if (impor.alphaIsTransparency != true)
        {
            impor.alphaIsTransparency = true;
            isChange = true;
        }

        if (impor.wrapMode != TextureWrapMode.Clamp)
        {
            impor.wrapMode = TextureWrapMode.Clamp;
            isChange = true;
        }

        bool isSetTextrePlatformSetting = SetTextrePlatformSetting(impor, true);
        if (isSetTextrePlatformSetting == true)
        {
            isChange = true;
        }


        if (isChange)
        {
            impor.SaveAndReimport();
        }
    }

    #region 工具方法
    /// <summary>
    /// 被4整除
    /// </summary>
    /// <param name="importer"></param>
    /// <returns></returns>
    bool IsDivisibleOf4(TextureImporter importer)
    {
        (int width, int height) = GetTextureImporterSize(importer);
        return (width % 4 == 0 && height % 4 == 0);
    }
    public static (int, int) GetTextureImporterSize(TextureImporter importer)
    {
        if (importer != null)
        {
            object[] args = new object[2];
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(importer, args);
            return ((int)args[0], (int)args[1]);
        }
        return (0, 0);
    }
    /// <summary>
    /// 获取是否是法线贴图
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool GetIsNormalMap(string path, TextureImporter ti)
    {
        if (ti.textureType == TextureImporterType.NormalMap)
        {
            return true;
        }
        int index = assetPath.IndexOf("_n");
        if (index == -1)
        {
            return false;
        }
        string str = assetPath.Substring(index + 2, 1);
        if (str.Equals(".") || str.Equals("_"))
        {
            return true;
        }

        return false;
    }

    private TextureImporterFormat GetSingleChannelFormat(TextureImporter impor)
    {
        TextureImporterSettings settings = new TextureImporterSettings();
        impor.ReadTextureSettings(settings);
        if (settings.singleChannelComponent == TextureImporterSingleChannelComponent.Alpha)
        {
            return TextureImporterFormat.Alpha8;
        }
        return TextureImporterFormat.R8;
    }
    /// <summary>
    /// 设置平台贴图
    /// </summary>
    /// <param name="impor"></param>
    /// <param name="isUI"></param>
    private bool SetTextrePlatformSetting(TextureImporter impor, bool isUI)
    {

        bool isChangeAndroid = false;
        bool isChangeiPhone = false;
        bool isChangePC = false;

        //-----android
        TextureImporterPlatformSettings settings = impor.GetPlatformTextureSettings("Android");
        if (settings.overridden != true)
        {
            settings.overridden = true;
            isChangeAndroid = true;
        }

        if (settings.allowsAlphaSplitting != false)
        {
            settings.allowsAlphaSplitting = false;
            isChangeAndroid = true;
        }

        if (settings.compressionQuality != (int)TextureCompressionQuality.Normal)
        {
            settings.compressionQuality = (int)TextureCompressionQuality.Normal;
            isChangeAndroid = true;
        }

        TextureImporterFormat andoridFormat = TextureImporterFormat.ASTC_6x6;
        if (GetIsNormalMap(assetPath, impor) || isUI == true)
        {
            andoridFormat = TextureImporterFormat.ASTC_4x4;
        }
        if (impor.textureType == TextureImporterType.SingleChannel)
        {
            andoridFormat = GetSingleChannelFormat(impor);
        }

        if (settings.format != andoridFormat)
        {
            settings.format = andoridFormat;
            isChangeAndroid = true;
        }
        if (isChangeAndroid)
        {
            impor.SetPlatformTextureSettings(settings);
        }

        //-------ios
        settings = impor.GetPlatformTextureSettings("iPhone");

        if (settings.overridden != true)
        {
            settings.overridden = true;
            isChangeiPhone = true;
        }

        if (settings.allowsAlphaSplitting != false)
        {
            settings.allowsAlphaSplitting = false;
            isChangeiPhone = true;
        }

        if (settings.compressionQuality != (int)TextureCompressionQuality.Normal)
        {
            settings.compressionQuality = (int)TextureCompressionQuality.Normal;
            isChangeiPhone = true;
        }


        TextureImporterFormat iosFormat = TextureImporterFormat.ASTC_6x6;
        if (GetIsNormalMap(assetPath, impor) || isUI == true)
        {
            iosFormat = TextureImporterFormat.ASTC_4x4;
        }
        if (impor.textureType == TextureImporterType.SingleChannel)
        {
            iosFormat = GetSingleChannelFormat(impor);
        }
        if (settings.format != iosFormat)
        {
            settings.format = iosFormat;
            isChangeiPhone = true;
        }

        if (isChangeiPhone)
        {
            impor.SetPlatformTextureSettings(settings);
        }

        //-------pc
        settings = impor.GetPlatformTextureSettings("Standalone");

        if (settings.overridden != true)
        {
            settings.overridden = true;
            isChangePC = true;
        }

        if (settings.allowsAlphaSplitting != false)
        {
            settings.allowsAlphaSplitting = false;
            isChangePC = true;
        }

        if (settings.compressionQuality != (int)TextureCompressionQuality.Normal)
        {
            settings.compressionQuality = (int)TextureCompressionQuality.Normal;
            isChangePC = true;
        }

        if (settings.textureCompression != TextureImporterCompression.Uncompressed)
        {
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            isChangePC = true;
        }

        TextureImporterFormat pcFormat = (impor.DoesSourceTextureHaveAlpha() | (impor.alphaSource == TextureImporterAlphaSource.FromGrayScale))
            ? TextureImporterFormat.RGBA32 : TextureImporterFormat.RGB24;

        if (GetIsNormalMap(assetPath, impor))
        {
            pcFormat = TextureImporterFormat.RGBA32;
        }
        if (impor.textureType == TextureImporterType.SingleChannel)
        {
            pcFormat = GetSingleChannelFormat(impor);
        }
        if (settings.format != pcFormat)
        {
            settings.format = pcFormat;
            isChangePC = true;
        }
        if (isChangePC)
        {
            impor.SetPlatformTextureSettings(settings);
        }
        return isChangePC == true && isChangeAndroid == true && isChangeiPhone == true;
    }
    #endregion
    #endregion
}
