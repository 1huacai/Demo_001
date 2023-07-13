using CoreFrameWork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FrameWork
{
    public static class ConfigUtils
    {

        ///// <summary>
        ///// Config对象深拷贝
        ///// </summary>
        ///// <param name="cfg"></param>
        ///// <returns></returns>
        //public static T DeepCopy<T>(this T cfg) where T : Config
        //{
        //    if (cfg == null) return null;
        //    Type t = cfg.GetType();
        //    PropertyInfo[] fis = t.GetProperties();
        //    T c = (T)Activator.CreateInstance(t);
        //    for (int i = 0; i < fis.Length; i++)
        //    {
        //        fis[i].SetValue(c, fis[i].GetValue(cfg, null), null);
        //    }
        //    return c;
        //}
        ///// <summary>
        ///// 自定义反射方法，通过解析字符串配置项反射修改字段数据
        ///// </summary>
        ///// <param name="cfg">反射类</param>
        ///// <param name="propName">字段名</param>
        ///// <param name="strValue">字段数据</param>
        //public static void SetConfigStrValue(this Config cfg, string propName, string strValue, VaryingExtension ex = null)
        //{
        //    if (string.IsNullOrEmpty(strValue)) return;
        //    FieldInfo fi = cfg.GetType().GetField(propName);
        //    if (fi == null)
        //    {
        //        Log.Error(string.Format("{0}类中不存在{1}属性，修改失败！", cfg, propName));
        //        return;
        //    }
        //    fi.SetValue(cfg, ConfigManager.ParseConfigField(cfg, strValue, fi.FieldType, ex));
        //}
        /// <summary>
        /// 自定义反射方法，反射修改字段数据
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetConfigValue(this Config cfg, string propName, object value)
        {
            var fi = cfg.GetType().GetField(propName);
            if (fi == null)
            {
                Log.Error(string.Format("{0}类中不存在{1}字段，修改失败！", cfg, propName));
                return;
            }
            fi.SetValue(cfg, value);
        }

        /// <summary>
        /// 通过反射获取字段数据
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetConfigValue(this Config cfg, string propName)
        {
            var fi = cfg.GetType().GetField(propName);
            if (fi == null)
            {
                Log.Error(string.Format("{0}类中不存在{1}字段，获取失败！", cfg, propName));
                return null;
            }
            return fi.GetValue(cfg);
        }

        public enum OperatorCode
        {
            Set,
            Add,
            Sub,
            Mul,
            Div,
            Mod
        }

        /// <summary>
        /// Config字段同类型的算术运算，若能运算，返回运算结果，否则返回A值
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operatorCode"></param>
        /// <returns></returns>
        public static object ConfigOperator(object a, object b, OperatorCode operatorCode)
        {
            if (a == null || b == null || a.GetType() != b.GetType())
            {
                return a;
            }
            Type t = a.GetType();
            if (t.IsArray)
            {
                return _arrayObjOperator((Array)a, (Array)b, operatorCode);
            }
            if (a is VarDoubleA)
            {
                double[] result = (double[])_arrayObjOperator(((VarDoubleA)a).Value, ((VarDoubleA)b).Value, operatorCode);
                return new VarDoubleA(result, new int[result.Length]);
            }
            if (a is VarDoubleAA)
            {
                double[][] result = (double[][])_arrayObjOperator(((VarDoubleAA)a).Value, ((VarDoubleAA)b).Value, operatorCode);
                return new VarDoubleAA(result, new int[result.Length][]);
            }
            if (t.IsGenericType)
            {
                IList listA = (IList)a;
                IList listB = (IList)b;

                Type ttl = t.GetGenericArguments()[0];
                Type tl = typeof(List<>).MakeGenericType(ttl);
                IList list = (IList)Activator.CreateInstance(tl);

                int min = Math.Min(listA.Count, listB.Count);
                int max = Math.Max(listA.Count, listB.Count);
                for (int i = 0; i < min; i++)
                {
                    list.Add(_baseObjOperator(listA[i], listB[i], operatorCode));
                }
                if (min < max)
                {
                    IList targetList = listA.Count >= listB.Count ? listA : listB;
                    for (int i = min; i < max; i++)
                    {
                        list.Add(targetList[i]);
                    }
                }
                return list;
            }
            return _baseObjOperator(a, b, operatorCode);
        }

        //public static void WriteSimpleBuffer ( Type t, object value, ByteBuffer buffer )
        //{
        //    if (t.IsArray)
        //    {
        //        if (value == null)
        //        {
        //            buffer.writeShort (0);
        //        }
        //        else
        //        {
        //            IList list = (IList)value;
        //            Type subType = t.GetElementType ();
        //            int len = list.Count;
        //            buffer.writeShort (len);
        //            for (int i = 0; i < len; i++)
        //            {
        //                WriteSimpleBuffer (subType, list[i], buffer);
        //            }
        //        }
        //    }
        //    else if (t.IsEnum)
        //    {

        //        //buffer.writeInt (Convert.ToInt32 (Enum.ToObject (t, value)));
        //        buffer.writeLong (Convert.ToInt64 (Enum.ToObject (t, value)));
        //    }
        //    else if (ConfigManager.IsConfigClass(t))
        //    {
        //        if (value == null)
        //        {
        //            buffer.writeUTF(null);
        //            buffer.writeLong(0);
        //        }
        //        else
        //        {
        //            buffer.writeUTF(t.FullName);
        //            buffer.writeLong(((Config)value).id);
        //        }
        //    }
        //    else
        //    {
        //        switch (t.Name)
        //        {
        //            case "String":
        //                if (value == null)
        //                    buffer.writeUTF (null);
        //                else
        //                    buffer.writeUTF (value.ToString ());
        //                break;
        //            case "Int32":
        //            case "UInt32":
        //                if (value == null)
        //                    buffer.writeInt (0);
        //                else
        //                    buffer.writeInt (int.Parse (value.ToString ()));
        //                break;
        //            case "Int64":
        //            case "UInt64":
        //                if (value == null)
        //                    buffer.writeLong (0);
        //                else
        //                    buffer.writeLong (long.Parse (value.ToString ()));
        //                break;
        //            case "Byte":
        //            case "SByte":
        //                if (value == null)
        //                    buffer.writeByte (0);
        //                else
        //                    buffer.writeByte (byte.Parse (value.ToString ()));
        //                break;
        //            case "Int16":
        //            case "UInt16":
        //                if (value == null)
        //                    buffer.writeShort (0);
        //                else
        //                    buffer.writeShort (short.Parse (value.ToString ()));
        //                break;
        //            case "Double":
        //                if (value == null)
        //                    buffer.writeDouble(0);
        //                else
        //                    buffer.writeDouble(double.Parse(value.ToString()));
        //                break;
        //            case "Single":
        //                if (value == null)
        //                    buffer.writeFloat (0);
        //                else
        //                    buffer.writeFloat (float.Parse (value.ToString ()));
        //                break;
        //            case "Char":
        //                if (value == null)
        //                    buffer.writeChar (0);
        //                else
        //                    buffer.writeChar (char.Parse (value.ToString ()));
        //                break;
        //            case "Boolean":
        //                if (value == null)
        //                    buffer.writeBoolean (false);
        //                else
        //                    buffer.writeBoolean(value.ToString().ToLower() == "true" || value.ToString() == "1");
        //                break;
        //        }
        //    }
        //}
        //public static object ReadSimpleBuffer(Config instance, Type t, ByteBuffer buffer, bool isAsset, VaryingExtension ex)
        //{
        //    if (t.IsArray)
        //    {
        //        int len = buffer.readUnsignedShort ();
        //        Type subType = t.GetElementType ();
        //        Array ins = Array.CreateInstance (subType, len);
        //        for (int i = 0; i < len; i++)
        //        {
        //            ins.SetValue(ReadSimpleBuffer(instance, subType, buffer, isAsset, ex), i);
        //        }
        //        return ins;
        //    }
        //    else if (t.IsEnum)
        //    {
        //        //return Enum.ToObject (t, buffer.readInt ());
        //        return Enum.ToObject (t, buffer.readLong ());
        //    }
        //    if (ConfigManager.IsConfigClass(t))
        //    {
        //        string tName = buffer.readUTF();
        //        if (!string.IsNullOrEmpty(tName))
        //        {
        //            return null;
        //        }
        //        Config c = ConfigManager.Instance.GetConfigFromDic(Type.GetType(tName), buffer.readLong(), ex);
        //        if (c != null)
        //        {
        //            if (instance is IConfigRef && ((IConfigRef)instance).ConfigRef != null)
        //            {
        //                ((IConfigRef)instance).ConfigRef.Add(c);
        //                if (c is IConfigRef && ((IConfigRef)c).ConfigRef != null)
        //                {
        //                    ((IConfigRef)instance).ConfigRef.AddRange(((IConfigRef)c).ConfigRef);
        //                }
        //            }
        //            int length = c.Assets.Count;
        //            string[] array = new string[length];
        //            c.Assets.CopyTo(array, 0);
        //            for (int i = 0; i < length; ++i)
        //            {
        //                instance.Assets.Add(array[i]);
        //            }
        //        }
        //        return c;
        //    }
        //    switch (t.Name)
        //    {
        //        case "String":
        //            string sbuff = buffer.readUTF();
        //            if (isAsset && !string.IsNullOrEmpty(sbuff))
        //            {
        //                instance.Assets.Add(sbuff);
        //            }
        //            return sbuff;
        //        case "Int32":
        //        case "UInt32":
        //            return buffer.readInt();
        //        case "Int64":
        //        case "UInt64":
        //            return buffer.readLong();
        //        case "Byte":
        //            return buffer.readByte();
        //        case "SByte":
        //            return buffer.readUnsignedByte();
        //        case "Int16":
        //            return buffer.readShort();
        //        case "UInt16":
        //            return buffer.readUnsignedShort();
        //        case "Double":
        //            return buffer.readDouble();
        //        case "Single":
        //            return buffer.readFloat();
        //        case "Char":
        //            return buffer.readChar();
        //        case "Boolean":
        //            return buffer.readBoolean();
        //    }
        //    return null;
        //}
        #region 算术运算相关实现
        private static object _arrayObjOperator(Array a, Array b, OperatorCode operatorCode)
        {
            Type t = a.GetType().GetElementType();
            Array arr = Array.CreateInstance(t, Math.Max(a.Length, b.Length));

            int min = Math.Min(a.Length, b.Length);
            int max = Math.Max(a.Length, b.Length);
            for (int i = 0; i < min; i++)
            {
                if (!t.IsArray)
                {
                    arr.SetValue(_baseObjOperator(a.GetValue(i), b.GetValue(i), operatorCode), i);
                }
                else
                {
                    arr.SetValue(_arrayObjOperator((Array)a.GetValue(i), (Array)b.GetValue(i), operatorCode), i);
                }
            }
            if (min < max)
            {
                Array targetArr = a.Length >= b.Length ? a : b;
                for (int i = min; i < max; i++)
                {
                    arr.SetValue(targetArr.GetValue(i), i);
                }
            }
            return arr;
        }


        private static object _baseObjOperator(object a, object b, OperatorCode operatorCode)
        {
            if (IsConfigClass(a.GetType()) && IsConfigClass(b.GetType()))
            {
                return operatorCode == OperatorCode.Set ? b : a;
            }
            if (a.GetType() != b.GetType())
            {
                return a;
            }
            if (operatorCode == OperatorCode.Set)
            {
                return b;
            }
            if (a is VarDouble)
            {
                double result = _objOperator((a as VarDouble).Value, (b as VarDouble).Value, operatorCode,
                                   (s, s1) => s + s1,
                                   (s, s1) => s - s1,
                                   (s, s1) => s * s1,
                                   (s, s1) => s / s1,
                                   (s, s1) => s % s1);
                return new VarDouble(result, 0);
            }

            switch (a.GetType().Name)
            {
                case "String":
                    return _objOperator((string)a, (string)b, operatorCode,
                        (s, s1) => s + s1,
                        (s, s1) => s,
                        (s, s1) => s,
                        (s, s1) => s,
                        (s, s1) => s);
                case "Int32":
                    return _objOperator((int)a, (int)b, operatorCode,
                        (s, s1) => s + s1,
                        (s, s1) => s - s1,
                        (s, s1) => s * s1,
                        (s, s1) => s / s1,
                        (s, s1) => s % s1);
                case "UInt32":
                    return _objOperator((uint)a, (uint)b, operatorCode,
                        (s, s1) => s + s1,
                        (s, s1) => s - s1,
                        (s, s1) => s * s1,
                        (s, s1) => s / s1,
                        (s, s1) => s % s1);
                case "UInt64":
                    return _objOperator((ulong)a, (ulong)b, operatorCode,
                        (s, s1) => s + s1,
                        (s, s1) => s - s1,
                        (s, s1) => s * s1,
                        (s, s1) => s / s1,
                        (s, s1) => s % s1);
                case "Double":
                    return _objOperator((double)a, (double)b, operatorCode,
                        (s, s1) => s + s1,
                        (s, s1) => s - s1,
                        (s, s1) => s * s1,
                        (s, s1) => s / s1,
                        (s, s1) => s % s1);

                case "Int64":
                    return _objOperator((long)a, (long)b, operatorCode,
                        (s, s1) => s + s1,
                        (s, s1) => s - s1,
                        (s, s1) => s * s1,
                        (s, s1) => s / s1,
                        (s, s1) => s % s1);
                case "Single":
                    return _objOperator((float)a, (float)b, operatorCode,
                        (s, s1) => s + s1,
                        (s, s1) => s - s1,
                        (s, s1) => s * s1,
                        (s, s1) => s / s1,
                        (s, s1) => s % s1);
                case "Byte":
                case "SByte":
                case "Int16":
                case "Char":
                case "UInt16":
                case "Boolean":
                    return a;
            }
            return a;
        }

        private static T _objOperator<T>(T a, T b, OperatorCode operatorCode, Func<T, T, T> opAdd, Func<T, T, T> opSub, Func<T, T, T> opMul, Func<T, T, T> opDiv, Func<T, T, T> opMod)
        {
            switch (operatorCode)
            {
                case OperatorCode.Add:
                    return opAdd != null ? opAdd(a, b) : a;
                case OperatorCode.Sub:
                    return opSub != null ? opSub(a, b) : a;
                case OperatorCode.Mul:
                    return opMul != null ? opMul(a, b) : a;
                case OperatorCode.Div:
                    return opDiv != null ? opDiv(a, b) : a;
                case OperatorCode.Mod:
                    return opMod != null ? opMod(a, b) : a;
            }
            return a;
        }
        #endregion

        public static object ParseConfigField(Config instance, string data, Type t, VaryingExtension ex)
        {
            //处理非数组类型数据
            if (!t.IsArray)
            {
                //处理list泛型
                if (t.IsGenericType)
                {
                    Type ttl = t.GetGenericArguments()[0];
                    Type tl = typeof(List<>).MakeGenericType(ttl);
                    if (data.Equals(""))
                        return Activator.CreateInstance(tl);
                    IList listConfig = (IList)Activator.CreateInstance(tl);
                    string[] strData = data.Split('+');
                    object o;
                    for (int i = 0; i < strData.Length; i++)
                    {
                        o = _parseBase(strData[i], ttl, ex);
                        if (o == null)
                            return null;
                        listConfig.Add(o);
                    }
                    return listConfig;
                }
                //修改基本类型数据
                return _parseBase(data, t, ex);
            }
            //字符串数据转换为指定类型数组数据
            if (t.Name.Contains("[][][]"))
            {
                Type tt3 = t.GetElementType().GetElementType().GetElementType();
                if (data.Equals(""))
                    return Array.CreateInstance(t.GetElementType(), 0);

                string[] line = data.Split('&');
                string[][][] str3 = new string[line.Length][][];
                for (int i = 0; i < line.Length; i++)
                {
                    var v2 = line[i].Split('|');
                    str3[i] = new string[v2.Length][];
                    for (int a = 0; a < v2.Length; a++)
                    {
                        str3[i][a] = v2[a].Split('+');
                    }
                }
                Array arrConfig3 = Array.CreateInstance(t.GetElementType(), line.Length);
                object odata;
                for (int a = 0; a < line.Length; a++)
                {
                    Array arrConfig2 = Array.CreateInstance(t.GetElementType().GetElementType(), line[a].Split('|').Length);
                    for (int i = 0; i < str3[a].Length; i++)
                    {
                        Array tempElem = Array.CreateInstance(tt3, str3[a][i].Length);
                        for (int j = 0; j < str3[a][i].Length; j++)
                        {
                            odata = _parseBase(str3[a][i][j], tt3, ex);
                            if (odata == null)
                                return null;
                            tempElem.SetValue(odata, j);
                        }
                        arrConfig2.SetValue(tempElem, i);
                    }
                    arrConfig3.SetValue(arrConfig2, a);
                }
                return arrConfig3;
            }
            else if (t.Name.Contains("[][]"))
            {
                Type tt2 = t.GetElementType().GetElementType();
                if (data.Equals(""))
                    return Array.CreateInstance(t.GetElementType(), 0);
                string[] line = data.Split('|');
                string[][] str2 = new string[line.Length][];
                for (int i = 0; i < line.Length; i++)
                    str2[i] = line[i].Split('+');

                Array arrConfig2 = Array.CreateInstance(t.GetElementType(), line.Length);
                object odata;
                for (int i = 0; i < line.Length; i++)
                {
                    Array tempElem = Array.CreateInstance(tt2, str2[i].Length);
                    for (int j = 0; j < str2[i].Length; j++)
                    {
                        odata = _parseBase(str2[i][j], tt2, ex);
                        if (odata == null)
                            return null;
                        tempElem.SetValue(odata, j);
                    }
                    arrConfig2.SetValue(tempElem, i);
                }
                return arrConfig2;
            }
            if (t.Name.Contains("[]"))
            {
                Type tt1 = t.GetElementType();
                if (data.Equals(""))
                    return Array.CreateInstance(tt1, 0);
                string[] str1 = data.Split('+');
                int n = str1.Length;
                Array arrConfig1 = Array.CreateInstance(tt1, n);
                object odata;
                for (int i = 0; i < n; i++)
                {
                    odata = _parseBase(str1[i], tt1, ex);
                    if (odata == null)
                        return null;
                    arrConfig1.SetValue(odata, i);
                }
                return arrConfig1;
            }
            return null;
        }

        private static object _parseBase(string data, Type type, VaryingExtension ex)
        {
            //字符串数据转换为指定类型枚举数据
            if (type.IsEnum)
                return data.Equals("") ? Enum.GetNames(type).GetValue(0) : Enum.Parse(type, data);
            //处理自定义类型数据
            if (IsConfigClass(type))
            {
                long id;
                if (!long.TryParse(data, out id))
                {
                    Log.Error("引用的Config类型" + type.Name + "的id（" + data + "）格式有误，返回空值");
                    return null;
                }
                Config c = ConfigManager.Instance.GetConfigFromDic(type, id);
                if (c == null)
                {
                    Log.Error("引用的Config类型" + type.Name + "的id（" + data + "）不存在，返回空值");
                }
                c.ApplyVaryingExtension(ex);
                return c;
            }
            if (data.Equals(""))
                return type.Name.Equals("String") ? "" : type.Assembly.CreateInstance(type.FullName);
            try
            {
                switch (type.Name)
                {
                    case "String":
                        return data;
                    case "Byte":
                        return byte.Parse(data);
                    case "SByte":
                        return sbyte.Parse(data);
                    case "Int16":
                        data = data.Replace('_', '-');
                        return short.Parse(data);
                    case "UInt16":
                        data = data.Replace('_', '-');
                        return ushort.Parse(data);
                    case "Int32":
                        data = data.Replace('_', '-');
                        return int.Parse(data);
                    case "UInt32":
                        data = data.Replace('_', '-');
                        return uint.Parse(data);
                    case "Int64":
                        data = data.Replace('_', '-');
                        return long.Parse(data);
                    case "UInt64":
                        data = data.Replace('_', '-');
                        return ulong.Parse(data);
                    case "Char":
                        return char.Parse(data);
                    case "Boolean":
                        return data.ToLower() == "true" || data == "1";
                    case "Single":
                        data = data.Replace('_', '-');
                        return float.Parse(data);
                    case "Double":
                        data = data.Replace('_', '-');
                        return double.Parse(data);
                    case "VarDouble":
                        var vardouble = VarDouble.Pare(data);
                        vardouble.ApplyVaryingExtension(ex);
                        return vardouble;
                    case "VarDoubleA":
                        var vardoubleA = VarDoubleA.Pare(data);
                        vardoubleA.ApplyVaryingExtension(ex);
                        return vardoubleA;
                    case "VarDoubleAA":
                        var vardoubleAA = VarDoubleAA.Pare(data);
                        vardoubleAA.ApplyVaryingExtension(ex);
                        return vardoubleAA;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static bool IsConfigClass(Type type)
        {
            if (type.IsInterface)
                return false;
            while (type != typeof(object))
            {
                if (type == typeof(Config))
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
