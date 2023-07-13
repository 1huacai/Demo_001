
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Text;

public class TextureEditor
{


    [MenuItem("Tools/图片处理/剔除Texture的MipMap")]
    public static void CancelGenerateMipMaps()
    {
        if (EditorUtility.DisplayDialog("提示", "剔除Texture的MipMap？", "确定", "取消"))
        {
            cancelGenerateMipMaps();
            EditorUtility.DisplayDialog("提示", "转换完毕！", "确定", "取消");
        }
    }

    private static void cancelGenerateMipMaps()
    {

        //string _tmp = "Assets/Resources/Effect/Textures/path20.png";
        //TextureImporter _importer1 = AssetImporter.GetAtPath(_tmp) as TextureImporter;
        //if (_importer1.mipmapEnabled)
        //{
        //    _importer1.mipmapEnabled = false;
        //    AssetDatabase.ImportAsset(_tmp);
        //}

        //return;

        string[] _array = AssetDatabase.FindAssets("t:Texture");

        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GUIDToAssetPath(_array[i]);
            TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;


            if (null != _importer)
            {
                if (_importer.textureType == TextureImporterType.Default || _importer.textureType == TextureImporterType.GUI)
                {
                    if (_importer.mipmapEnabled)
                    {
                        _importer.mipmapEnabled = false;
                        AssetDatabase.ImportAsset(_path);
                    }
                }
            }

        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    [MenuItem("Tools/图片处理/改变Texture的压缩格式（安卓）")]
    public static void ChangeTexureCompressType()
    {
        if (EditorUtility.DisplayDialog("提示", "改变Texture的压缩格式（安卓）？", "确定", "取消"))
        {
            changeTexureCompressType();
            EditorUtility.DisplayDialog("提示", "转换完毕！", "确定", "取消");

        }
    }


    public static void changeTexureCompressType()
    {
        if (EditorUtility.DisplayDialog("提示", "改变Texture的压缩格式（安卓）？", "确定", "取消"))
        {

            string[] _array = AssetDatabase.FindAssets("t:Texture");

            for (int i = 0; i < _array.Length; i++)
            {
                string _path = AssetDatabase.GUIDToAssetPath(_array[i]);
                TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;

                if (null != _importer)
                {
                    TextureImporterPlatformSettings _settings = _importer.GetPlatformTextureSettings("Android");

                    if (_settings.format == TextureImporterFormat.RGBA16 || _settings.format == TextureImporterFormat.ARGB32)
                    {
                        _settings.allowsAlphaSplitting = true;
                        _settings.compressionQuality = 50;
                        _settings.crunchedCompression = true;
                        _settings.format = TextureImporterFormat.ETC2_RGBA8;
                        _settings.overridden = true;
                        _settings.textureCompression = TextureImporterCompression.Compressed;
                        _importer.SetPlatformTextureSettings(_settings);

                        AssetDatabase.ImportAsset(_path);
                    }

                }

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }



    [MenuItem("Tools/图片处理/一键调优Texture（安卓）")]
    public static void OneKeyAuto()
    {
        if (EditorUtility.DisplayDialog("提示", "是否一键调优Texture？", "确定", "取消"))
        {
            string[] _array = AssetDatabase.FindAssets("t:Texture");

            for (int i = 0; i < _array.Length; i++)
            {
                string _path = AssetDatabase.GUIDToAssetPath(_array[i]);
                TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;

                if (null != _importer)
                {
                    int _flag = 0;

                    //去掉 mipmap
                    if (_importer.textureType == TextureImporterType.Default || _importer.textureType == TextureImporterType.GUI)
                    {
                        if (_importer.mipmapEnabled)
                        {
                            _flag++;
                            _importer.mipmapEnabled = false;
                        }
                    }



                    TextureImporterPlatformSettings _settings_Android = _importer.GetPlatformTextureSettings("Android");
                    if (_settings_Android.overridden)
                    {
                        _flag++;
                        _settings_Android.overridden = false;
                        _importer.SetPlatformTextureSettings(_settings_Android);
                    }


                    //TextureImporterPlatformSettings _settings_IOS = _importer.GetPlatformTextureSettings("IOS");
                    //if (_settings_IOS.overridden)
                    //{
                    //    _flag++;
                    //    _settings_IOS.overridden = false;
                    //    _importer.SetPlatformTextureSettings(_settings_IOS);
                    //}


                    if (_flag > 0)
                    {
                        AssetDatabase.ImportAsset(_path);
                    }
                }

            }



            EditorUtility.DisplayDialog("提示", "完毕！", "确定", "取消");

        }
    }

    [MenuItem("Tools/图片处理/RGBA16 -  ETC2_RGBA8")]
    public static void ConverRGBA16ToETC2_RGBA8()
    {

        string[] _array = AssetDatabase.FindAssets("t:Texture");

        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GUIDToAssetPath(_array[i]);
            ConverRGBAToETC2_RGBA8(_path);

        }
        AssetDatabase.Refresh();
    }
    [MenuItem("Tools/图片处理/RGBA16 -  ETC2_RGBA8( Selected )")]
    public static void ConverRGBA16ToETC2_RGBA8_1()
    {
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            string _path = AssetDatabase.GetAssetPath(Selection.objects[i]);
            ConverRGBAToETC2_RGBA8(_path);
        }
        AssetDatabase.Refresh();
    }


    private static void ConverRGBAToETC2_RGBA8(string path)
    {

        TextureImporter _importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (null != _importer)
        {
            TextureImporterPlatformSettings _settings_Android = _importer.GetPlatformTextureSettings("Android");
            if (_settings_Android.overridden)
            {
                if (_settings_Android.format == TextureImporterFormat.RGBA16 ||
                    _settings_Android.format == TextureImporterFormat.ARGB32 ||
                    _settings_Android.format == TextureImporterFormat.RGBA32)
                {
                    _settings_Android.format = TextureImporterFormat.ETC2_RGBA8;

                    if (_importer.npotScale == TextureImporterNPOTScale.None)
                    {
                        _importer.npotScale = TextureImporterNPOTScale.ToNearest;
                    }

                    _importer.SetPlatformTextureSettings(_settings_Android);

                    AssetDatabase.ImportAsset(path);
                }


            }
        }






    }

    [MenuItem("Tools/图片处理/选中指定文件夹下的 Texture")]
    public static void SelectTexture()
    {
        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i] is Texture)
            {
                _list.Add(_array[i]);

            }
        }
        Selection.objects = _list.ToArray();

    }

    [MenuItem("Tools/选中处理/Testtttttt ")]
    public static void Testtttttt()
    {
        StringBuilder _sb = new StringBuilder();
        string _path = string.Empty;
        // _path = AssetDatabase.GetAssetPath(Selection.objects[0]);
        // TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;

        //  Debug.LogError(_importer.spriteBorder);

        for (int i = 0; i < Selection.objects.Length; ++i)
        {
            _path = AssetDatabase.GetAssetPath(Selection.objects[i]);
            TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;
            _path = _path.Replace("Assets/Resources/", string.Empty);
            _path = AssetBundleTool.DeletSuffix(_path);

            Texture2D _tex = Resources.Load(_path) as Texture2D;

            if (null != _tex)
            {

                bool _rgba = _importer.alphaIsTransparency;
                int _num = _rgba ? 4 : 3;
                 int _KB =_tex.width*_tex.height*_num / 1024;

                if (_tex.width != _tex.height && _KB<128)
                {
                    _sb.Append(_path);
                    Debug.LogError(_path);

                }
                Resources.UnloadAsset(_tex);
                AssetDatabase.Refresh();
            }
        }
        Debug.LogError(_sb.ToString());
    }



    [MenuItem("Tools/选中处理/选中指定文件夹下的 AnimationClip")]
    public static void SelectAnimationClip()
    {
        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i] is AnimationClip)
            {
                _list.Add(_array[i]);

            }
        }
        Selection.objects = _list.ToArray();

    }


    //static List<Keyframe> TrimScaleKeyframes(AnimationClipCurveData curve)
    //{
    //    List<Keyframe> keyframes = __keyframes;
    //    float maxValue, minValue, averageValue;
    //    keyframes.Clear();
    //    List<KeyframeSample> samples = TakeSamples(curve, out maxValue, out minValue, out averageValue);
    //    int depth = curve.propertyName.Split('/').Length;
    //    var kcount = samples.Count;
    //    keyframes.Add(samples[0].keyframe);
    //    bool lastIsRemoved = false;
    //    Keyframe lastKeyframe = new Keyframe();
    //    var epsilon = m_scaleError;
    //    float error = 0;
    //    for (int k = 1; k < kcount - 1; ++k)
    //    {
    //        var kf = samples[k].keyframe;
    //        var diff = samples[k].pos.y - keyframes[keyframes.Count - 1].value;
    //        error += diff;
    //        if (Mathf.Abs(error) > epsilon)
    //        {
    //            if (lastIsRemoved)
    //            {
    //                keyframes.Add(lastKeyframe);
    //                lastIsRemoved = false;
    //            }
    //            keyframes.Add(kf);
    //            error = 0;
    //        }
    //        else
    //        {
    //            lastIsRemoved = true;
    //            lastKeyframe = kf;
    //        }
    //    }
    //    keyframes.Add(samples[kcount - 1].keyframe);
    //    if (keyframes.Count == 2)
    //    {
    //        if (Math.Abs(keyframes[0].value - keyframes[1].value) < Mathf.Abs(m_positionError))
    //        {
    //            keyframes[0] = KeyframeUtil.GetNew(keyframes[0].time, keyframes[0].value, TangentMode.Linear, TangentMode.Linear);
    //            keyframes[1] = KeyframeUtil.GetNew(keyframes[1].time, keyframes[1].value, TangentMode.Linear, TangentMode.Linear);
    //        }
    //    }
    //    return keyframes;
    //}


}







