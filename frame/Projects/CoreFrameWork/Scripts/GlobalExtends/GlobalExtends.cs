using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Aor扩展方法封装集合
/// </summary>
public static class GlobalExtends
{

    public static void Dispose(this GameObject obj)
    {
        if (Application.isEditor)
        {
            GameObject.DestroyImmediate(obj);
        }
        else
        {
            GameObject.Destroy(obj);
        }
    }

    public static void Dispose(this Transform trans)
    {
        if (Application.isEditor)
        {
            GameObject.DestroyImmediate(trans.gameObject);
        }
        else
        {
            GameObject.Destroy(trans.gameObject);
        }
    }

    /// <summary>
    /// 获取(广义)字符串字节数(数字/符号/英文字符占1个字节,中文占2个字节)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int GetUWidthLength(this string str)
    {
        int byteLen = 0;
        char[] strs = str.ToCharArray();
        int i, len = strs.Length;
        for (i = 0; i < len; i++)
        {
            if (Convert.ToInt32(strs[i]) > 255)
            {
                byteLen += 2;
            }
            else
            {
                byteLen += 1;
            }
        }
        return byteLen;
    }

    /// <summary>
    /// 截取指定字节长度的字符串
    /// </summary>
    /// <param name="str">原字符串</param>
    /// <param name="startIndex">起始位置</param>
    /// <param name="bytesLen">截取字节长度</param>
    /// <returns></returns>
    public static string CutByteString(this string str, int startIndex, int bytesLen)
    {
        string result = string.Empty;// 最终返回的结果
        if (string.IsNullOrEmpty(str)) { return result; }

        int byteLen = System.Text.Encoding.Default.GetByteCount(str);// 单字节字符长度
        int charLen = str.Length;// 把字符平等对待时的字符串长度
        char[] chars = str.ToCharArray();

        if (startIndex == 0)
        {

            int byteCount = 0;// 记录读取进度
            int pos = 0;// 记录截取位置

            for (int i = 0; i < charLen; i++)
            {
                if (Convert.ToInt32(chars[i]) > 255) // 按中文字符计算加2
                {
                    byteCount += 2;
                }
                else // 按英文字符计算加1
                {
                    byteCount += 1;
                }
                if (byteCount > bytesLen)// 超出时只记下上一个有效位置
                {
                    pos = i;
                    break;
                }
                else if (byteCount == bytesLen)// 记下当前位置
                {
                    pos = i + 1;
                    break;
                }
            }
            if (pos >= 0) { result = str.Substring(0, pos); }


        }
        else if (startIndex >= byteLen)
        {
            return result;
        }
        else //startIndex < byteLen
        {

            int AllLen = startIndex + bytesLen;
            int byteCountStart = 0;// 记录读取进度
            int byteCountEnd = 0;// 记录读取进度
            int startpos = 0;// 记录截取位置                
            int endpos = 0;// 记录截取位置

            for (int i = 0; i < charLen; i++)
            {
                if (Convert.ToInt32(chars[i]) > 255) // 按中文字符计算加2
                {
                    byteCountStart += 2;
                }
                else // 按英文字符计算加1
                {
                    byteCountStart += 1;
                }
                if (byteCountStart > startIndex)// 超出时只记下上一个有效位置
                {
                    startpos = i;
                    AllLen = startIndex + bytesLen - 1;
                    break;
                }
                else if (byteCountStart == startIndex)// 记下当前位置
                {
                    startpos = i + 1;
                    break;
                }
            }

            if (startIndex + bytesLen <= byteLen)//截取字符在总长以内
            {
                for (int i = 0; i < charLen; i++)
                {
                    if (Convert.ToInt32(chars[i]) > 255) // 按中文字符计算加2
                    {
                        byteCountEnd += 2;
                    }
                    else // 按英文字符计算加1
                    {
                        byteCountEnd += 1;
                    }
                    if (byteCountEnd > AllLen)// 超出时只记下上一个有效位置
                    {
                        endpos = i;
                        break;
                    }
                    else if (byteCountEnd == AllLen)// 记下当前位置
                    {
                        endpos = i + 1;
                        break;
                    }
                }
                endpos = endpos - startpos;
            }
            else if (startIndex + bytesLen > byteLen)//截取字符超出总长
            {
                endpos = charLen - startpos;
            }
            if (endpos >= 0)
            {
                result = str.Substring(startpos, endpos);
            }
        }
        return result;
    }

    /// <summary>
    /// 可控的误差比较
    /// </summary>
    /// <param name="f1">浮点源1</param>
    /// <param name="f2">浮点源2</param>
    /// <param name="epsilon">公差范围</param>
    /// <returns>是否在误差内</returns>
    public static bool FloatEquel(this float f1, float f2, float epsilon = float.Epsilon)
    {
        return Mathf.Abs(f1 - f2) < epsilon;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tran">tran</param>
    /// <param name="layer">设置的layer</param>
    /// <param name="layerDic">字典,存设置前的老layer</param>
    public static void SetLayerAndSaveDic(this Transform tran, int layer, Dictionary<int, int> layerDic)
    {


        if (layerDic != null && !layerDic.ContainsKey(tran.GetHashCode()))
            layerDic.Add(tran.GetHashCode(), tran.gameObject.layer);

        tran.gameObject.layer = layer;

        if (tran.childCount > 0)
        {
            foreach (Transform each in tran)
            {
                SetLayerAndSaveDic(each, layer, layerDic);
            }
        }

    }

    /// <summary>
    /// 获取对象在Hierarchy中的节点路径
    /// </summary>
    public static string getHierarchyPath(this GameObject inObj)
    {
        return _getHierarchPathLoop(inObj.transform);
    }
    /// <summary>
    /// 获取对象在Hierarchy中的节点路径
    /// </summary>
    public static string getHierarchyPath(this Transform tran)
    {
        return _getHierarchPathLoop(tran);
    }

    private static string _getHierarchPathLoop(Transform t, string path = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = t.gameObject.name;
        }
        else
        {
            path = t.gameObject.name + "/" + path;
        }

        if (t.parent != null)
        {
            return _getHierarchPathLoop(t.parent, path);
        }
        else
        {
            return path;
        }
    }


    /// <summary>
    /// 通过字典递归设置Layer
    /// </summary>
    /// <param name="layerDic">缓存Layer数据的字典</param>
    public static void SetLayerByDic(this Transform tran, Dictionary<int, int> layerDic)
    {

        int code = tran.GetHashCode();
        if (layerDic != null && layerDic.ContainsKey(code))
        {
            tran.gameObject.layer = layerDic[code];
        }


        if (tran.childCount > 0)
        {
            foreach (Transform each in tran)
            {
                SetLayerByDic(each, layerDic);
            }
        }

    }

    /// <summary>
    /// 递归设置Layer
    /// </summary>
    public static void SetLayer(this Transform tran, int layer, int ignoreLayer = int.MinValue)
    {
        if (tran.gameObject.layer != ignoreLayer)
        {
            tran.gameObject.layer = layer;
        }

        if (tran.childCount > 0)
        {
            foreach (Transform each in tran)
            {
                SetLayer(each, layer, ignoreLayer);
            }
        }

    }

    /// <summary>
    /// 获取当前Transform对象上挂载的接口
    /// </summary>
    public static T GetInterface<T>(this Transform tran) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            return null;
        }
        //return inObj.GetComponents<Component>().OfType<T>().FirstOrDefault();
        Component[] cp = tran.gameObject.GetComponents<Component>();
        int i, length = cp.Length;
        for (i = 0; i < length; i++)
        {
            if (cp[i] is T)
            {
                T t = cp[i] as T;
                return t;
            }
        }
        return null;
    }
    /// <summary>
    /// 获取当前Transform对象上挂载的接口集合
    /// </summary>
    public static T[] GetInterfaces<T>(this Transform tran) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            return null;
        }
        Component[] cp = tran.gameObject.GetComponents<Component>();
        int i, length = cp.Length;

        if (length == 0)
        {
            return null;
        }

        ArrayList ts = new ArrayList();

        for (i = 0; i < length; i++)
        {
            if (cp[i] is T)
            {
                ts.Add(cp[i]);
            }
        }

        length = ts.Count;
        if (length > 0)
        {
            T[] o = new T[length];
            for (i = 0; i < length; i++)
            {
                o[i] = ts[i] as T;
            }
            return o;
        }

        return null;
    }

    /// <summary>
    /// 获取当前GameObject对象上挂载的接口
    /// </summary>
    public static T GetInterface<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            return null;
        }
        //return inObj.GetComponents<Component>().OfType<T>().FirstOrDefault();
        Component[] cp = inObj.GetComponents<Component>();
        int i, length = cp.Length;
        for (i = 0; i < length; i++)
        {
            if (cp[i] is T)
            {
                T t = cp[i] as T;
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取当前GameObject对象上挂载的接口集合
    /// </summary>
    private static Component[] cp;
    private static ArrayList ts= new ArrayList();

    public static T[] GetInterfaces<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            return null;
        }
        cp = null;
        cp = inObj.GetComponents<Component>();
        int i, length = cp.Length;

        if (length == 0)
        {
            return null;
        }

          ts.Clear();

        for (i = 0; i < length; i++)
        {
            if (cp[i] is T)
            {
                ts.Add(cp[i]);
            }
        }

        length = ts.Count;
        if (length > 0)
        {
            T[] o = new T[length];
            for (i = 0; i < length; i++)
            {
                o[i] = ts[i] as T;
            }
            return o;
        }

        return null;
    }

    /// <summary>
    /// 获取当前GameObject对象上挂载的接口集合
    /// </summary>
    public static T[] GetInterfacesInChildren<T>(this GameObject inObj) where T : class
    {
        if (!typeof(T).IsInterface)
        {
            return null;
        }
        Component[] cp = inObj.GetComponentsInChildren<Component>();
        int i, length = cp.Length;

        if (length == 0)
        {
            return null;
        }

        ArrayList ts = new ArrayList();

        for (i = 0; i < length; i++)
        {
            if (cp[i] is T)
            {
                ts.Add(cp[i]);
            }
        }

        length = ts.Count;
        if (length > 0)
        {
            T[] o = new T[length];
            for (i = 0; i < length; i++)
            {
                o[i] = ts[i] as T;
            }
            return o;
        }

        return null;
    }

    /// <summary>
    /// 克隆一个List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<T> clone<T>(this List<T> list) where T : class
    {
        if (list == null) return null;
        List<T> t = new List<T>();
        int i, length = list.Count;
        for (i = 0; i < length; i++)
        {
            T item = list[i] as T;
            t.Add(item);
        }
        return t;
    }

    /// <summary>
    /// 查找并返回Component(当前对象找不到,则查找子对象)
    /// </summary>
    public static T FindComponent<T>(this GameObject obj) where T : Component
    {
        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
        }
        return ins;
    }
    /// <summary>
    /// 查找并返回Component(当前对象找不到,则查找子对象)
    /// </summary>
    public static T FindComponent<T>(this Transform obj) where T : Component
    {
        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
        }
        return ins;
    }
    /// <summary>
    /// 查找并返回Component(当前对象找不到,则查找子和父级对象)
    /// </summary>
    public static T FindComponentIncParent<T>(this GameObject obj) where T : Component
    {

        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
            if (ins == null)
            {
                ins = obj.GetComponentInParent<T>();
            }
        }
        return ins;
    }
    /// <summary>
    /// 查找并返回Component(当前对象找不到,则查找子和父级对象)
    /// </summary>
    public static T FindComponentIncParent<T>(this Transform obj) where T : Component
    {
        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
            if (ins == null)
            {
                ins = obj.GetComponentInParent<T>();
            }
        }
        return ins;
    }

    /// <summary>
    /// 查找或者创建Component(当前Component在当前对象和子集对象都找不到,则在当前对象上创建Component)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T FindOrCreateComponent<T>(this GameObject obj) where T : Component
    {
        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
            if (ins == null)
            {
                ins = obj.AddComponent<T>();
            }
        }
        return ins;
    }

    /// <summary>
    /// 查找或者创建Component(当前Component在当前对象和子集对象都找不到,则在当前对象上创建Component)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T FindOrCreateComponent<T>(this Transform obj) where T : Component
    {
        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
            if (ins == null)
            {
                ins = obj.gameObject.AddComponent<T>();
            }
        }
        return ins;
    }

    /// <summary>
    /// 查找或者创建Component(当前Component在当前对象和子集/父级对象都找不到,则在当前对象上创建Component)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T FindOrCreateComponentIncParent<T>(this GameObject obj) where T : Component
    {
        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
            if (ins == null)
            {
                ins = obj.GetComponentInParent<T>();
                if (ins == null)
                {
                    ins = obj.AddComponent<T>();
                }
            }
        }
        return ins;
    }

    /// <summary>
    /// 查找或者创建Component(当前Component在当前对象和子集/父级对象都找不到,则在当前对象上创建Component)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T FindOrCreateComponentIncParent<T>(this Transform obj) where T : Component
    {
        T ins = obj.GetComponent<T>();
        if (ins == null)
        {
            ins = obj.GetComponentInChildren<T>();
            if (ins == null)
            {
                ins = obj.GetComponentInParent<T>();
                if (ins == null)
                {
                    ins = obj.gameObject.AddComponent<T>();
                }
            }
        }
        return ins;
    }

    /// <summary>
    /// 按照节点顺序返回所有子节点的Component<T>;
    /// （不包含自身）
    /// </summary>
    public static List<T> FindAllComponentsInChildrenInOrder<T>(this GameObject go) where T : Component
    {
        return go.transform.FindAllComponentsInChildrenInOrder<T>();
    }

    /// <summary>
    /// 按照节点顺序返回所有子节点的Component<T>;
    /// （不包含自身）
    /// </summary>
    public static List<T> FindAllComponentsInChildrenInOrder<T>(this Transform trans) where T : Component
    {
        List<T> list = new List<T>();

        FACICIOLoop<T>(trans, ref list);

        if (list.Count > 0)
        {
            return list;
        }
        else
        {
            return null;
        }

    }
    /// <summary>
    /// 从节点的Root开始按照节点顺序返回所有子节点的Component<T>;
    /// （包含自身）
    /// </summary>
    public static List<T> FindAllComponentsInOrder<T>(this GameObject go) where T : Component
    {
        return go.transform.FindAllComponentsInOrder<T>();
    }
    /// <summary>
    /// 从节点的Root开始按照节点顺序返回所有子节点的Component<T>;
    /// （包含自身）
    /// </summary>
    public static List<T> FindAllComponentsInOrder<T>(this Transform trans) where T : Component
    {
        List<T> list = new List<T>();

        T cpt = trans.root.GetComponent<T>();
        if (cpt != null)
        {
            list.Add(cpt);
        }

        FACICIOLoop<T>(trans.root, ref list);

        if (list.Count > 0)
        {

            return list;
        }
        else
        {
            return null;
        }
    }


    private static void FACICIOLoop<T>(Transform t, ref List<T> list) where T : Component
    {
        int i, len = t.childCount;
        for (i = 0; i < len; i++)
        {
            Transform ct = t.GetChild(i);
            T cpt = ct.GetComponent<T>();
            if (cpt != null)
            {
                list.Add(cpt);
            }
            if (ct.childCount > 0)
            {
                FACICIOLoop<T>(ct, ref list);
            }
        }
    }
}
