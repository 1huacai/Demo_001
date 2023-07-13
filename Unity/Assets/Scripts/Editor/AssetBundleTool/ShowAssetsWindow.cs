using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Text;


public class ShowAssetsWindow : EditorWindow
{

    public static void init()
    {

        ShowAssetsWindow w = EditorWindow.GetWindow<ShowAssetsWindow>("展示目标资源");

    }

    void Awake()
    {

    }

    void Reset()
    {

    }

    public static void Show(List<string> list, Action callBack,
        List<string> md5NewList, List<string> md5ModityList, List<string> md5DeletList, List<string> md5RePackList)
    {
        init();
        m_list = list;
        m_callBack = callBack;
        m_md5NewList = md5NewList;
        m_md5ModityList = md5ModityList;
        m_md5DeletList = md5DeletList;
        m_md5RePackList = md5RePackList;
        for (int i = 0; i < m_md5NewList.Count; ++i)
        {
            string _path = m_md5NewList[i];
            if (!m_list.Contains(_path))
            {
                m_list.Add(_path);
            }
        }
        for (int i = 0; i < m_md5ModityList.Count; ++i)
        {
            string _path = m_md5ModityList[i];
            if (!m_list.Contains(_path))
            {
                m_list.Add(_path);
            }
        }
        for (int i = 0; i < m_md5DeletList.Count; ++i)
        {
            string _path = m_md5DeletList[i];
            if (!m_list.Contains(_path))
            {
                m_list.Add(_path);
            }
        }
    }


    private Vector2 scroll;
    private static Action m_callBack;
    private static List<string> m_list;
    private static List<string> m_md5NewList;
    private static List<string> m_md5ModityList;
    private static List<string> m_md5DeletList;
    private static List<string> m_md5RePackList;

    

    private bool IsNew(string path)
    {

        return m_md5NewList.Contains(path);

    }
    private bool IsModify(string path)
    {

        return m_md5ModityList.Contains(path);

    }
    private bool IsDelet(string path)
    {

        return m_md5DeletList.Contains(path);

    }
    private bool IsRePack(string path)
    {

        return m_md5RePackList.Contains(path);

    }
    
    void OnGUI()
    {


        scroll = EditorGUILayout.BeginScrollView(scroll, true, true, GUILayout.Height(500));

        for (int i = 0; i < m_list.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle _style = new GUIStyle();
            string _flag = string.Empty;
            string _path = m_list[i];
            if (IsNew(_path))
            {
                _style.normal.textColor = Color.green;
                _flag = "New";
            }
            else if (IsModify(_path))
            {
                _style.normal.textColor = Color.blue;
                _flag = "Modity";
            }
            else if (IsDelet(_path))
            {
                _style.normal.textColor = Color.red;
                _flag = "Delet";
            }
            else if (IsRePack(_path))
            {
                _style.normal.textColor = Color.yellow;
                _flag = "RePack";
            }

            else
            {
                _style.normal.textColor = Color.yellow;
                _flag = "RePack";
            }
            EditorGUILayout.LabelField((i + 1) + ". "+ _path);
            EditorGUILayout.LabelField(_flag, _style);

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();


        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {

            if (null != m_callBack)
            {
                m_callBack();
            }

            Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }

}





