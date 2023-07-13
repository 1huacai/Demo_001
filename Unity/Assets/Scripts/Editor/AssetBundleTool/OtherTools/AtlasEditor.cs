
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

public class AtlasEditor
{


    [MenuItem("Tools/图集处理/设置图集打包标签")]
    public static void SetAtlasLabels()
    {
        if (EditorUtility.DisplayDialog("提示", "设置图集打包标签？", "确定", "取消"))
        {
            setAtlasLabels();
            EditorUtility.DisplayDialog("提示", "设置完毕！", "确定", "取消");
        }
    }


    public static void setAtlasLabels()
    {
        //将图集所有文件夹设置为 BuildAsFolderName
        //将所有非 _Mat _RGB _Alpha 结尾的文件设置为 EditorOnly
        //将所有带 Sprite 的文件夹设置为 EditorOnly

        Caching.ClearCache();
        if (!Directory.Exists(Application.dataPath + "/Resources/Ui"))
        {
            return;
        }

        List<string> _directors = Directory.GetDirectories(Application.dataPath + "/Resources/Ui", "*.*",
            SearchOption.AllDirectories).ToList();
        for (int i = 0; i < _directors.Count; ++i)
        {
            string _path = _directors[i];
            _path = _path.Replace("\\", "/").Replace(Application.dataPath, string.Empty).Insert(0, "Assets");
            Object _asset = AssetDatabase.LoadMainAssetAtPath(_path);
            if (_path.Contains("Sprite") || _path.Contains(".."))
            {
                AssetDatabase.SetLabels(_asset, new string[1] { "EditorOnly" });

            }
            else
            {
                AssetDatabase.SetLabels(_asset, new string[1] { "BuildAsFolderName" });


            }
        }

        List<string> _files = Directory.GetFiles(Application.dataPath + "/Resources/Ui", "*.*",
            SearchOption.AllDirectories).Where(s => !s.Contains("..") && !s.ToLower().EndsWith(".meta")).ToList();
        for (int i = 0; i < _files.Count; ++i)
        {
            string _path = _files[i];
            _path = _path.Replace("\\", "/").Replace(Application.dataPath, string.Empty).Insert(0, "Assets");
            Object _asset = AssetDatabase.LoadMainAssetAtPath(_path);
            if (_path.Contains("_RGB") || _path.Contains("_Alpha") || _path.Contains("_Mat") || _asset is Material)
            {
                if(_path.Contains("MapMini"))
                {
                    continue;
                }
                AssetDatabase.ClearLabels(_asset);

                if (_path.Contains("_RGB"))
                {
                    TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;
                    if (_importer.wrapMode != TextureWrapMode.Clamp)
                    {
                        _importer.wrapMode = TextureWrapMode.Clamp;
                        AssetDatabase.ImportAsset(_path);
                    }
                }

                if (_path.Contains("_Alpha"))
                {
                    TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;
                    if (_importer.wrapMode != TextureWrapMode.Clamp)
                    {
                        _importer.wrapMode = TextureWrapMode.Clamp;
                        AssetDatabase.ImportAsset(_path);
                    }

                    if (_importer.textureType != TextureImporterType.Default)
                    {
                        _importer.textureType = TextureImporterType.Default;
                        _importer.mipmapEnabled = false;
                        AssetDatabase.ImportAsset(_path);
                    }

                }
            }
            else
            {
                AssetDatabase.SetLabels(_asset, new string[1] { "EditorOnly" });
            }

        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    [MenuItem("Tools/图集处理/设置图集Panoramic_npc")]
    public static void setPanoramic_npcAtlas()
    {
        Caching.ClearCache();

        List<string> _files = Directory.GetFiles(Application.dataPath + "/Resources/Role/Panoramic_npc/assets", "*.*",
            SearchOption.AllDirectories).Where(s => !s.Contains("..") && (s.ToLower().EndsWith("_rgb.png") || s.ToLower().EndsWith("_alpha.png"))).ToList();
        for (int i = 0; i < _files.Count; ++i)
        {
            string _path = _files[i];
            _path = _path.Replace("\\", "/").Replace(Application.dataPath, string.Empty).Insert(0, "Assets");
            Object _asset = AssetDatabase.LoadMainAssetAtPath(_path);
            if (_path.Contains("_RGB") || _path.Contains("_Alpha"))
            {

                if (_path.Contains("_RGB"))
                {
                    TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;

                    if (_importer.alphaIsTransparency ||
                        _importer.mipmapEnabled ||
                        _importer.wrapMode != TextureWrapMode.Clamp)
                    {

                        _importer.alphaIsTransparency = false;
                        _importer.mipmapEnabled = false;
                        _importer.wrapMode = TextureWrapMode.Clamp;
                        AssetDatabase.ImportAsset(_path);
                    }
                }

                if (_path.Contains("_Alpha"))
                {
                    TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;
                    if (_importer.alphaIsTransparency ||
                            _importer.mipmapEnabled ||
                            _importer.textureType != TextureImporterType.Default ||
                        _importer.wrapMode != TextureWrapMode.Clamp)
                    {
                        _importer.alphaIsTransparency = false;
                        _importer.mipmapEnabled = false;
                        _importer.textureType = TextureImporterType.Default;
                        _importer.wrapMode = TextureWrapMode.Clamp;
                        AssetDatabase.ImportAsset(_path);
                    }

                    TextureImporterPlatformSettings _settings = _importer.GetPlatformTextureSettings("Android");
                    if (null != _settings && _settings.overridden)
                    {
                        _settings.overridden = false;
                        _importer.SetPlatformTextureSettings(_settings);
                        AssetDatabase.ImportAsset(_path);
                    }
                }
            }

        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    [MenuItem("Tools/图集处理/设置Ui图集格式 Fast")]
    public static void setUiAtlasFast()
    {
        SetUiAtlas(0);
    }
    [MenuItem("Tools/图集处理/设置Ui图集格式 Normal")]
    public static void setUiAtlasNormal()
    {
        SetUiAtlas(50);
    }

    [MenuItem("Tools/图集处理/设置Ui图集格式 Best (慎用，超级慢)")]
    public static void setUiAtlasBest()
    {
        SetUiAtlas(100);
    }


    public static void SetUiAtlas(int compressionQuality)
    {
        Caching.ClearCache();

        List<string> _files = Directory.GetFiles(Application.dataPath + "/Resources/Ui", "*.*",
            SearchOption.AllDirectories).Where(s => !s.Contains("..") && (s.ToLower().EndsWith("_rgb.png") || s.ToLower().EndsWith("_alpha.png"))).ToList();
        for (int i = 0; i < _files.Count; ++i)
        {
            string _path = _files[i];
            _path = _path.Replace("\\", "/").Replace(Application.dataPath, string.Empty).Insert(0, "Assets");
            if (_path.Contains("MapView/"))
            {
                continue;
            }

            Object _asset = AssetDatabase.LoadMainAssetAtPath(_path);

            int _count = 0;

            TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;
            if (_path.Contains("_RGB"))
            {
                if (_importer.textureType != TextureImporterType.Sprite)
                {
                    _count++;
                    _importer.textureType = TextureImporterType.Sprite;
                }

            }
            if (_path.Contains("_Alpha"))
            {
                if (_importer.textureType != TextureImporterType.Default)
                {
                    _count++;
                    _importer.textureType = TextureImporterType.Default;
                }

            }

            if (_importer.alphaIsTransparency)
            {
                _count++;
                _importer.alphaIsTransparency = false;
            }
            if (_importer.mipmapEnabled)
            {
                _count++;
                _importer.mipmapEnabled = false;
            }
            if (_importer.wrapMode != TextureWrapMode.Clamp)
            {
                _count++;
                _importer.wrapMode = TextureWrapMode.Clamp;
            }

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                TextureImporterPlatformSettings _androidSettings = _importer.GetPlatformTextureSettings("Android");
                if (null != _androidSettings)
                {
                    if (!_androidSettings.overridden)
                    {
                        _count++;
                        _androidSettings.overridden = true;
                    }
                    if (_androidSettings.format != TextureImporterFormat.ETC_RGB4)
                    {
                        _count++;
                        _androidSettings.format = TextureImporterFormat.ETC_RGB4;
                    }

                    if (_androidSettings.compressionQuality != compressionQuality)
                    {
                        _count++;
                        _androidSettings.compressionQuality = compressionQuality;
                    }
                    if (_count > 0)
                    {
                        _importer.SetPlatformTextureSettings(_androidSettings);
                    }
                }
            }

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                TextureImporterPlatformSettings _iOSSettings = _importer.GetPlatformTextureSettings("iPhone");
                if (null != _iOSSettings)
                {
                    if (!_iOSSettings.overridden)
                    {
                        _count++;
                        _iOSSettings.overridden = true;
                    }
                    if (_iOSSettings.format != TextureImporterFormat.PVRTC_RGB4)
                    {
                        _count++;
                        _iOSSettings.format = TextureImporterFormat.PVRTC_RGB4;
                    }
                    if (_iOSSettings.compressionQuality != compressionQuality)
                    {
                        _count++;
                        _iOSSettings.compressionQuality = compressionQuality;
                    }


                    if(_path.Contains("Common_New/Common_New_RGB"))
                    {
                        _iOSSettings.overridden = false;
                    }

                    if (_count > 0)
                    {
                        _importer.SetPlatformTextureSettings(_iOSSettings);
                    }


                }
            }

            if (_count > 0)
            {
                AssetDatabase.ImportAsset(_path);
            }


        }

        Caching.ClearCache();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Find Build Crash prefabs")]
    public static void FindCrashMissingPrefabs()
    {
        string[] allassetpaths = AssetDatabase.GetAllAssetPaths();
        EditorUtility.DisplayProgressBar("Bundle Crash Find", "Finding...", 0f);
        int len = allassetpaths.Length;
        int index = 0;
        foreach (var filePath in allassetpaths)
        {
            EditorUtility.DisplayProgressBar("Bundle Crash Find", filePath, (index + 0f) / (len + 0f));
            if (filePath.EndsWith(".prefab"))
            {
                GameObject fileObj = null;
                try
                {
                      fileObj = PrefabUtility.LoadPrefabContents(filePath);
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }

                if (fileObj)
                {
                    Component[] cps = fileObj.GetComponentsInChildren<Component>(true);
                    foreach (var cp in cps)
                    {
                        if (cp)
                        {
                            PrefabInstanceStatus _type = PrefabUtility.GetPrefabInstanceStatus(cp.gameObject);
                            if (_type == PrefabInstanceStatus.MissingAsset)
                            {
                                //string nodePath = PsdToUguiEx.CopyLuJin(null)+"/"+ fileObj.name;
                                Debug.LogError("Crash Bundle Missing Prefab:Path=" + filePath + " Name:" + fileObj.name + " ComponentName:" + cp);
                            }
                        }
                        else
                        {
                            Debug.LogError("Component Missing Prefab:Path=" + filePath + " Name:" + fileObj.name );
                        }
                    }
                }
                PrefabUtility.UnloadPrefabContents(fileObj);
            }
            index++;
        }
        EditorUtility.ClearProgressBar();
    }


    [MenuItem("Tools/检测文件路劲是否存在非英语字符")]
    public static void FindNoneEnglishCharactor()
    {
        string[] allassetpaths = AssetDatabase.GetAllAssetPaths();
        EditorUtility.DisplayProgressBar("Check Path", "Checking...", 0f);
        int len = allassetpaths.Length;
        int index = 0;
        Regex rex = new Regex(@"[a-z0-9A-Z]+");


        //allassetpaths = new string[] { "fa2452/4sdfds的.png"};
        foreach (var filePath in allassetpaths)
        {
            EditorUtility.DisplayProgressBar("Check Path", filePath, (index + 0f) / (len + 0f));
            string  _path= filePath;
            _path = Regex.Replace(_path, "[a-z0-9A-Z]+", "");//
            _path = _path.Replace("/","").Replace(".","").Replace("//","").Replace(" ","");
             
            Match ma = rex.Match(_path);
            if (!ma.Success && !string.IsNullOrEmpty(_path))
            {
                Debug.LogError(filePath);
            }
            index++;
        }
        EditorUtility.ClearProgressBar();
    }

}







