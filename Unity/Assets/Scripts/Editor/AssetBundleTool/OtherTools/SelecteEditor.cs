
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Text;

public class SelecteEditor
{

    public static void SelectTarget(Type type)
    {
        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i].GetType() == type)
            {
                _list.Add(_array[i]);

            }
        }
        Selection.objects = _list.ToArray();

    }


    [MenuItem("Tools/选中处理/选中指定文件夹下的 Texture")]
    public static void SelectTexture() 
    {
        SelectTarget(typeof(Texture2D));
    }

    [MenuItem("Tools/选中处理/选中指定文件夹下的 AnimationClip")]
    public static void SelectAnimationClip()
    {
        SelectTarget(typeof(AnimationClip));
    }


    [MenuItem("Tools/选中处理/选中指定文件夹下的 AudioClip")]
    public static void SelectAudioClip()
    {
        SelectTarget(typeof(AudioClip));
    }







    [MenuItem("Tools/选中处理/选中指定文件夹下的 FBX")]
    public static UnityEngine.Object[] SelectFBX()
    {
        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();
        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GetAssetPath(_array[i]);
            if (_array[i].GetType() == typeof(GameObject) && _path.ToLower().EndsWith(".fbx"))
            {
                _list.Add(_array[i]);

            }
        }
        Selection.objects = _list.ToArray();
        return Selection.objects;
    }
    [MenuItem("Tools/选中处理/选中指定文件夹下的 Prefab")]
    public static UnityEngine.Object[] SelectPrefab()
    {

        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();

        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GetAssetPath(_array[i]);
            if (_array[i].GetType() == typeof(GameObject) && _path.ToLower().EndsWith(".prefab"))
            {
                _list.Add(_array[i]);

            }
        }
        Selection.objects = _list.ToArray();

        return Selection.objects;
    }


    [MenuItem("Tools/选中处理/选中指定文件夹下的 asset")]
    public static UnityEngine.Object[] SelectAsset()
    {

        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();

        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);

        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GetAssetPath(_array[i]);
            if (_path.ToLower().EndsWith(".asset"))
            {
                _list.Add(_array[i]);

            }
        }
        Selection.objects = _list.ToArray();

        return Selection.objects;
    }

    [MenuItem("Tools/选中处理/选中指定文件夹下的 Material")]
    public static void SelectMaterial()
    {
        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();
        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GetAssetPath(_array[i]);
            if (_array[i].GetType() == typeof(Material))
            {
                _list.Add(_array[i]);

            }
        }
        Selection.objects = _list.ToArray();

    }


    [MenuItem("Tools/选中处理/选中指定文件夹下的 Material 文件夹")]
    public static void SelectMaterialFolder()
    {

        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();
        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GetAssetPath(_array[i]);
            if (!_path.Contains(".mat"))
            {
                if (_path.EndsWith("Materials") || _path.EndsWith("Mat") || _path.EndsWith("mat"))
                {
                    _list.Add(_array[i]);

                }
            }
        }
        Selection.objects = _list.ToArray();

    }



    [MenuItem("Tools/选中处理/清理粒子系统")]
    public static void ClearParticleSystem()
    {


        UnityEngine.Object[] _array = SelectPrefab();

        for (int i = 0; i < _array.Length; i++)
        {
            ParticleSystem[] _ps = (_array[i] as GameObject).GetComponentsInChildren<ParticleSystem>();
            for (int j = 0; j < _ps.Length; ++j)
            {
                Renderer _render = _ps[j].GetComponent<Renderer>();
                if (null != _render && null != _render.sharedMaterial && _render.sharedMaterial.name == "Default-Particle")
                {
                    _render.sharedMaterial = null;
                    GameObject.DestroyImmediate(_ps[j], true);
                }
            }
        }


        //for (int i = 0; i < _array.Length; i++)
        //{
        //    ParticleSystem[] _ps = (_array[i] as GameObject).GetComponentsInChildren<ParticleSystem>();
        //    string _path = AssetDatabase.GetAssetPath(_array[i]);
        //     List<string> _list= AssetDatabase.GetDependencies(_path).ToList();

        //    for(int j = 0; j < _list.Count; ++j)
        //    {
        //        if(_list[j].ToLower().EndsWith(".fbx"))
        //        {

        //        }
        //    }
        //}


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("提示", "完成！", "确定", "取消");

    }

    [MenuItem("Tools/选中处理/清理FBX")]
    public static void ClearFBX()
    {


        UnityEngine.Object[] _array = SelectFBX();

        for (int i = 0; i < _array.Length; i++)
        {

            MeshFilter _filter = (_array[i] as GameObject).GetComponent<MeshFilter>();
            if (null != _filter)
            {
                GameObject.DestroyImmediate(_filter, true);
            }

            MeshRenderer _meshRender = (_array[i] as GameObject).GetComponent<MeshRenderer>();
            if (null != _meshRender)
            {
                GameObject.DestroyImmediate(_meshRender, true);
            }



            Animator _target = (_array[i] as GameObject).GetComponent<Animator>();
            if (null != _target)
            {
                GameObject.DestroyImmediate(_target, true);
            }

            Animation _animation = (_array[i] as GameObject).GetComponent<Animation>();
            if (null != _animation)
            {
                GameObject.DestroyImmediate(_animation, true);
            }

            //SkinnedMeshRenderer[] _renderArray = (_array[i] as GameObject).GetComponentsInChildren<SkinnedMeshRenderer>();
            //if (null != _renderArray)
            //{
            //    for (int j = 0; j < _renderArray.Length; j++)
            //    {
            //        for (int k = 0; k < _renderArray[j].sharedMaterials.Length; ++k)
            //        {
            //            if (null != _renderArray[j].sharedMaterials[k] && _renderArray[j].sharedMaterials[k].name == "Default-Material")
            //            {

            //                _renderArray[j].sharedMaterials[k] = null;


            //            }
            //        }

            //    }
            //}

        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("提示", "完成！", "确定", "取消");

    }



    [MenuItem("Tools/选中处理/选中带空格的资源")]
    public static void ClearSpaceAssetBundle()
    {

        UnityEngine.Object[] _array = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
        List<UnityEngine.Object> _list = new List<UnityEngine.Object>();
        for (int i = 0; i < _array.Length; i++)
        {
            string _path = AssetDatabase.GetAssetPath(_array[i]);
            if (_path.Contains(" "))
            {
                _list.Add(_array[i]);
                Debug.LogError(_path);
            }
        }
        Selection.objects = _list.ToArray();


    }


}







