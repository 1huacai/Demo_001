//using System;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using UnityEditor;
//using UnityEngine;
//using Object = UnityEngine.Object;
//using System.Text;
//using System.Linq;
//using FrameWorkScriptableObject;



//public class CheckAssetBundleWindow : EditorWindow
//{
//    [MenuItem("Tools/检查AssetBundle资源")]
//    public static void init()
//    {

//        CheckAssetBundleWindow w = EditorWindow.GetWindow<CheckAssetBundleWindow>("检查AssetBundle资源");

//    }
//    [MenuItem("Assets/检查AssetBundle资源")]
//    public static void init1()
//    {

//        CheckAssetBundleWindow w = EditorWindow.GetWindow<CheckAssetBundleWindow>("检查AssetBundle资源");
//        obj = Selection.activeObject;
//        Check();
//    }
//    void Awake()
//    {

//    }

//    void Reset()
//    {

//    }

//    private static void Check()
//    {
//        _list.Clear();
//        if(null == obj)
//        {
//            return;
//        }
//        string path = AssetDatabase.GetAssetPath(obj);

//        FAssetBundleDataNest _assetBundleNest = Resources.Load("AssetBundleNest") as FAssetBundleDataNest;
//        if (null == _assetBundleNest)
//        {
//            Debug.LogError("未找到  AssetBundleNest，解析失败！！！");
//            return;
//        }
//        Dictionary<string, FAssetBundleData> _dic = new Dictionary<string, FAssetBundleData>(_assetBundleNest.m_list.Count);
//        for (int i = 0; i < _assetBundleNest.m_list.Count; ++i)
//        {
//            _dic.Add(_assetBundleNest.m_list[i].Path, _assetBundleNest.m_list[i]);
//        }

//        string _key = path.Replace("Assets/StreamingAssets/StreamingResources/", "");
//        FAssetBundleData _data = _dic[_key];
//        AssetBundle _assetBundle = AssetBundle.LoadFromFile(path, _data.Crc, _data.offset);
//        if (null != _assetBundle)
//        {
//            string[] _array = _assetBundle.GetAllAssetNames();
//            _array = _array.ToList().OrderBy(v => v).ToArray();
//            for (int i = 0; i < _array.Length; ++i)
//            {
//                string _content = (i + 1) + " :" + _array[i];
//                _list.Add(_content);

//            }

//            _assetBundle.Unload(true);
//        }
       
//    }




//    private static Object obj;
//    private Vector2 scroll;
//    static List<string>  _list = new List<string>();

//    void OnGUI()
//    {

//        obj = EditorGUILayout.ObjectField("AssetBundle", obj, typeof(Object), false);
     
//        if (GUILayout.Button("Check"))
//        {
//            Check();
//        }
//        scroll = EditorGUILayout.BeginScrollView(scroll, true, true, GUILayout.Height(500));
      
//        for (int i = 0; i < _list.Count; ++i)
//        {
//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField(_list[i]);
//            EditorGUILayout.EndHorizontal();
//        }
//        EditorGUILayout.EndScrollView();


//    }

//}





