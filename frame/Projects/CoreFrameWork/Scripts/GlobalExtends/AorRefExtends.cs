using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 常用反射Extends;
/// </summary>
public static class AorRefExtends
{

    // -- Field get
    #region Field Get

    //
    static Dictionary<Type, Dictionary<string, PropertyInfo>> AllSerializeFieldPropertiesCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

    public static Dictionary<string, PropertyInfo> GetAllSerializeFieldProperties(Type t)
    {
        Dictionary<string, PropertyInfo> infos = null;
        if (!AllSerializeFieldPropertiesCache.ContainsKey(t))
        {
            infos = new Dictionary<string, PropertyInfo>();
            AllSerializeFieldPropertiesCache.Add(t, infos);
        }
        else
        {
            return AllSerializeFieldPropertiesCache[t];
        }

        GetAllSerializeFieldProperties(t, ref infos);

        return infos;
    }


    private static void GetAllSerializeFieldProperties(Type t, ref Dictionary<string, PropertyInfo> infos)
    {
        PropertyInfo[] props = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);


        foreach (PropertyInfo each in props)
        {
            if (each.CanWrite)
            {
                if (!infos.ContainsKey(each.Name))
                {
                    infos.Add(each.Name, each);
                }

            }
            else
            {
                // CustomAttributeData.GetCustomAttributes(each).Where(d => d.Constructor.DeclaringType == typeof(SerializeField)).Select(d => new AttributeFactory(d)).ToList();


                //序列化标识获得
                // Attribute[] attrs = Attribute.GetCustomAttributes(each);
                object[] attrs = each.GetCustomAttributes(typeof(SerializeField), true);

                if (attrs.Length > 0)
                {
                    if (!infos.ContainsKey(each.Name))
                    {
                        infos.Add(each.Name, each);
                    }
                }


            }

        }


        if (t.BaseType != null)
        {
            GetAllSerializeFieldProperties(t.BaseType, ref infos);
        }


    }
    public static PropertyInfo GetSerializeFieldPropertie(Type t, string name)
    {
        if (!AllSerializeFieldPropertiesCache.ContainsKey(t))
        {
            AllSerializeFieldPropertiesCache.Add(t, new Dictionary<string, PropertyInfo>());
        }

        if (AllSerializeFieldPropertiesCache[t].ContainsKey(name))
        {
            return AllSerializeFieldPropertiesCache[t][name];
        }
        else
        {
            PropertyInfo prop = t.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
            {
                if (t.BaseType != null)
                {
                    return GetSerializeFieldPropertie(t.BaseType, name);
                }
            }
            else
            {
                if (prop.CanWrite)
                {

                    AllSerializeFieldPropertiesCache[t].Add(name, prop);
                    return prop;
                }
            }


        }

        return null;


    }


    public static FieldInfo GetSerializeFieldField(Type t, string name)
    {
        //无ｄｉｃ建立ｄｉｃ
        if (!AllSerializeFieldFieldsCache.ContainsKey(t))
        {
            AllSerializeFieldFieldsCache.Add(t, new Dictionary<string, FieldInfo>());
        }

        if (AllSerializeFieldFieldsCache[t].ContainsKey(name))
        {
            return AllSerializeFieldFieldsCache[t][name];
        }
        else
        {

            FieldInfo field = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (field == null)
            {


                if (t.BaseType != null)
                {
                    return GetSerializeFieldField(t.BaseType, name);
                }
            }
            else
            {
                if (field.IsPublic)
                {
                    AllSerializeFieldFieldsCache[t].Add(name, field);
                    return field;

                }
                else
                {

                    //序列化标识获得
                    // Attribute[] attrs = Attribute.GetCustomAttributes(each);
                    object[] attrs = field.GetCustomAttributes(typeof(SerializeField), true);

                    if (attrs.Length > 0)
                    {
                        AllSerializeFieldFieldsCache[t].Add(name, field);
                        return field;
                    }


                }
            }


        }


        return null;


    }

    static Dictionary<Type, Dictionary<string, FieldInfo>> AllSerializeFieldFieldsCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();

    public static Dictionary<string, FieldInfo> GetAllSerializeFieldFields(Type t)
    {
        Dictionary<string, FieldInfo> infos = null;
        if (!AllSerializeFieldFieldsCache.ContainsKey(t))
        {
            infos = new Dictionary<string, FieldInfo>();
            AllSerializeFieldFieldsCache.Add(t, infos);
        }
        else
        {
            return AllSerializeFieldFieldsCache[t];
        }

        GetAllSerializeFieldFields(t, ref infos);

        return infos;
    }

    private static void GetAllSerializeFieldFields(Type t, ref Dictionary<string, FieldInfo> infos)
    {

        FieldInfo[] fields = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);



        foreach (FieldInfo each in fields)
        {
            if (each.IsPublic)
            {

                if (!infos.ContainsKey(each.Name))
                {
                    infos.Add(each.Name, each);
                }

            }
            else
            {

                //序列化标识获得
                // Attribute[] attrs = Attribute.GetCustomAttributes(each);
                object[] attrs = each.GetCustomAttributes(typeof(SerializeField), true);

                if (attrs.Length > 0)
                {
                    if (!infos.ContainsKey(each.Name))
                    {
                        infos.Add(each.Name, each);
                    }
                }


            }

        }


        if (t.BaseType != null)
        {
            GetAllSerializeFieldFields(t.BaseType, ref infos);
        }


    }





    public static object ref_GetField_Inst_NonPublic(this object obj, string fieldName)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(obj);
        }
        else
        {
            return null;
        }
    }

    public static object ref_GetField_Inst_Public(this object obj, string fieldName)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(obj);
        }
        else
        {
            return null;
        }
    }

    //未验证
    public static object ref_GetField_Static_NonPublic(this object obj, string fieldName)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(null);
        }
        else
        {
            return null;
        }
    }

    //未验证
    public static object ref_GetField_Static_Public(this object obj, string fieldName)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(null);
        }
        else
        {
            return null;
        }
    }

    #endregion

    // -- Field set
    #region Field Set
    public static bool ref_SetField_Inst_NonPublic(this object obj, string fieldName, object value)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(obj, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ref_SetField_Inst_Public(this object obj, string fieldName, object value)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(obj, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    //未验证
    public static bool ref_SetField_Static_NonPublic(this object obj, string fieldName, object value)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(null, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    //未验证
    public static bool ref_SetField_Static_Public(this object obj, string fieldName, object value)
    {
        Type t = obj.GetType();
        FieldInfo fieldInfo = t.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(null, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    // -- InvokeMethod
    #region InvokeMethod

    public static object ref_InvokeMethod_Inst_NonPublic(this object obj, string MethodName, object[] parameters)
    {
        Type t = obj.GetType();
        MethodInfo methodInfo = t.GetMethod(MethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
        if (methodInfo != null)
        {
            return methodInfo.Invoke(obj, parameters);
        }
        else
        {
            return null;
        }
    }

    public static object ref_InvokeMethod_Inst_Public(this object obj, string MethodName, object[] parameters)
    {
        Type t = obj.GetType();
        MethodInfo methodInfo = t.GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
        if (methodInfo != null)
        {
            return methodInfo.Invoke(obj, parameters);
        }
        else
        {
            return null;
        }
    }

    public static object ref_InvokeMethod_Static_NonPublic(this object obj, string MethodName, object[] parameters)
    {
        Type t = obj.GetType();
        MethodInfo methodInfo = t.GetMethod(MethodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
        if (methodInfo != null)
        {
            return methodInfo.Invoke(null, parameters);
        }
        else
        {
            return null;
        }
    }

    public static object ref_InvokeMethod_Static_Public(this object obj, string MethodName, object[] parameters)
    {
        Type t = obj.GetType();
        MethodInfo methodInfo = t.GetMethod(MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);
        if (methodInfo != null)
        {
            return methodInfo.Invoke(null, parameters);
        }
        else
        {
            return null;
        }
    }

    #endregion
}
