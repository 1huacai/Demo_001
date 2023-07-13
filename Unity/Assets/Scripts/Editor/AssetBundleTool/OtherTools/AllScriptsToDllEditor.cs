using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class AllScriptsToDllEditor : EditorWindow
{

    [MenuItem("Assets/AllScriptsToDll")]
    private static void AllScriptsToDllEditorTool()
    {

        if (0 == Selection.objects.Length)
        {
            return;
        }
        UnityEngine.Object _ab = Selection.objects[0];
        string path = Application.streamingAssetsPath +"/StreamingResources/allscripts.assetbundle";
        AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
        if (null != assetBundle)
        {
            string _dir = Application.dataPath + "/tmp";
            _dir = _dir.Replace("Assets/","");
            if (!Directory.Exists(_dir))
            {
                Directory.CreateDirectory(_dir);
            }
            Object[] _array = assetBundle.LoadAllAssets();
            for (int i = 0; i < _array.Length; ++i)
            {
                TextAsset _ta = _array[i] as TextAsset;
                if (null != _ta)
                {
                    string _filePath = _dir +"/"+ _ta.name + ".dll";
                    if (File.Exists(_filePath))
                    {
                        File.Delete(_filePath);
                    }
                    File.WriteAllBytes(_filePath, _ta.bytes);
                }
            }

            assetBundle.Unload(true);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


}
