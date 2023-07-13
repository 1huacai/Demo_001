using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Object = UnityEngine.Object;
using CoreFrameWork.Com;

namespace CoreFrameWork.Utils
{
    public static class FUtils
    {
        /// <summary>
        /// 字符串转换枚举
        /// </summary>
        public static T EnumParse<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }

        public static uint EnumsParse(Type t, string name)
        {


            if (string.IsNullOrEmpty(name))
                return 0;
            string[] names = name.Split('|');
            uint rt = 0;
            for (int i = 0; i < names.Length; i++)
            {
                rt |= Convert.ToUInt32(Enum.Parse(t, names[i]));
            }
            return rt;
        }


        #region prop format, prop can is 100 or 100%
        public static double PropFormat(string v, float nowV, float baseV, float percentage = 0.01f)
        {
            bool reduce = v.Contains("--");
            bool add = v.Contains("++");
            if (reduce)
                v = v.Replace("--", "");
            else if (add)
                v = v.Replace("++", "");

            double value = 0;
            if (v.IndexOf("%") == -1)
            {
                value = v.ToFloat();
            }
            else if (v.IndexOf("%%") != -1)
            {
                value = Math.Round(v.Replace("%%", "").ToFloat() * percentage * baseV);
            }
            else
            {
                value = Math.Round(v.Replace("%", "").ToFloat() * percentage * nowV);
            }
            if (reduce)
                value = nowV - value;
            if (add)
                value = nowV + value;

            return value;
        }

        public static float PropFormat(string prop)
        {
            float value = 0;
            if (prop.IndexOf("%") == -1)
            {
                value = prop.ToFloat();
            }
            else
            {
                value = prop.Replace("%", "").ToFloat();
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public static string PropAdd(string a1, string a2)
        {
            //if (a1.Length > 2 && a1.Substring (a1.Length - 2, 1) == "%")
            if (a1.Length >= 2 && a1.Substring(a1.Length - 1, 1) == "%")
            {
                return (a1.Replace("%", "").ToFloat() + a2.Replace("%", "").ToFloat()).ToString() + "%";
            }
            return (a1.ToFloat() + a2.ToFloat()).ToString();
        }
        public static string PropMultiply(string a1, string a2)
        {
            if (a1.Length >= 2 && a1.Substring(a1.Length - 1, 1) == "%")
            {
                return (a1.Replace("%", "").ToFloat() * a2.Replace("%", "").ToFloat()).ToString() + "%";
            }
            return (a1.ToFloat() * a2.ToFloat()).ToString();
        }
        public static string PropMultiply(string a1, double v)
        {
            if (a1.Length >= 2 && a1.Substring(a1.Length - 1, 1) == "%")
            {
                return (a1.Replace("%", "").ToFloat() * v).ToString() + "%";
            }
            return (a1.ToFloat() * v).ToString();
        }
        #endregion

        #region 随机打乱list
        /// <summary>
        /// 随即打乱列表
        /// </summary>
        public static void listRamdom<T>(List<T> arr)
        {
            System.Random ran = new System.Random();
            int k = 0;
            T strtmp;
            for (int i = 0; i < arr.Count; i++)
            {
                k = ran.Next(0, 20);
                if (k != i)
                {
                    strtmp = arr[i];
                    arr[i] = arr[k];
                    arr[k] = strtmp;
                }
            }
        }
        /// <summary>
        /// 随即打乱数组
        /// </summary>
        public static void arrayRamdom<T>(T[] arr)
        {
            System.Random ran = new System.Random();
            int k = 0;
            T strtmp;
            for (int i = 0; i < arr.Length; i++)
            {
                k = ran.Next(0, 20);
                if (k != i)
                {
                    strtmp = arr[i];
                    arr[i] = arr[k];
                    arr[k] = strtmp;
                }
            }
        }
        #endregion


        #region other
        public static float testExecTime(CallBack call)
        {
            System.Diagnostics.Stopwatch testTime = new System.Diagnostics.Stopwatch();
            testTime.Start();
            call();
            return testTime.ElapsedMilliseconds;
        }
        public static float testExecTime(CallBack call, int count)
        {
            System.Diagnostics.Stopwatch testTime = new System.Diagnostics.Stopwatch();
            testTime.Start();
            for (int i = 0; i < count; i++)
                testExecTime(call);
            return testTime.ElapsedMilliseconds;
        }
        #endregion

        #region 获取md5码
        public static string GetMD5(byte[] bytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            return hashString.PadLeft(32, '0');
        }


        public static string GetMD5(string str)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            return GetMD5(bytes);
        }
        #endregion
        #region string to int,float,byte....
        public static long StrToLong(string str)
        {
            return str == null || str.Trim() == "" ? 0 : long.Parse(str.Replace("_", "-"));
        }

        public static uint StrTouint(string str)
        {
            uint s;
            try
            {
                s = str == null || str.Trim() == "" ? 0 : uint.Parse(str.Replace("_", "-"));
            }
            catch
            {
                s = uint.MaxValue;
            }
            return s;
        }
        public static int StrToInt(string str)
        {
            int s;
            try
            {
                s = str == null || str.Trim() == "" ? 0 : int.Parse(str.Replace("_", "-"));
            }
            catch
            {
                s = int.MaxValue;
            }
            return s;
        }
        public static byte StrToByte(string str)
        {
            return str == null || str.Trim() == "" ? (byte)0 : byte.Parse(str);
        }
        public static short StrToShort(string str)
        {
            return str == null || str.Trim() == "" ? (short)0 : short.Parse(str.Replace("_", "-"));
        }
        public static float StrToFloat(string str)
        {
            return str == null || str.Trim() == "" ? 0f : float.Parse(str.Replace("_", "-"));
        }
        public static double StrToDouble(string str)
        {
            return str == null || str.Trim() == "" ? 0f : double.Parse(str.Replace("_", "-"));
        }

        public static Color StrToColor(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Color.black;
            else
            {
                float r, g, b, a;

                string[] strs = str.Split(',');

                r = StrToFloat(strs[0]);
                if (r > 1)
                    r = r / 255;
                g = strs.Length > 1 ? StrToFloat(strs[1]) : 0;
                if (g > 1)
                    g = g / 255;
                b = strs.Length > 2 ? StrToFloat(strs[2]) : 0;
                if (b > 1)
                    b = b / 255;
                a = strs.Length > 3 ? StrToFloat(strs[3]) : 0;
                if (a > 1)
                    a = a / 255;
                return new Color(r, g, b, a);
            }
        }

        public static Vector3 StrToVector3(string str)
        {
            if (str == "" || str == "0")
                return Vector3.zero;
            string[] strs = str.Split(',');
            return new Vector3(StrToFloat(strs[0]), strs.Length > 1 ? StrToFloat(strs[1]) : 0, strs.Length > 2 ? StrToFloat(strs[2]) : 0);
        }

        public static Vector2 StrToVector2(string str)
        {
            if (str == "" || str == "0")
                return Vector2.zero;
            string[] strs = str.Split(',');
            return new Vector2(StrToFloat(strs[0]), strs.Length > 1 ? StrToFloat(strs[1]) : 0);
        }
        public static string Vector3ToStr(Vector3 vec)
        {
            return vec.x + "," + vec.y + "," + vec.z;
        }
        public static string Vector2ToStr(Vector2 vec)
        {
            return vec.x + "," + vec.y;
        }

        #endregion
        /// <summary>
        /// 四舍五入
        /// </summary>
        /// <param name="_value"></param>
        /// <returns></returns>
        public static int CeilAndFloorToInt(float _value)
        {
            int _valueInt = Mathf.FloorToInt(_value);
            float _tempValue = _value - (float)_valueInt;

            int _hereValue = _tempValue >= 0.5f ? (++_valueInt) : _valueInt;
            return _hereValue;

        }

        /// <summary>
        /// 时间戳转换DateTime对象
        /// </summary>
        public static DateTime UnixTimeToDateTime(long unixTime)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return dtStart.Add(new TimeSpan(unixTime * 10000000));
        }

        public static void ClearChildren(Transform tran)
        {
            if (null != tran)
            {
                for (int i = 0; i < tran.childCount; ++i)
                {
                    Transform _temp = tran.GetChild(i);
                    if (null != _temp)
                    {
                        MonoBehaviour.Destroy(_temp.gameObject);
                    }
                }
            }
        }

        public static void ClearChildrenImmediate(Transform tran)
        {
            if (null != tran)
            {
                for (int i = 0; i < tran.childCount; ++i)
                {
                    i--;
                    Transform _temp = tran.GetChild(0);
                    _temp.gameObject.SetActive(false);
                    //这句unity报警告
                    //_temp.parent = null;
                    MonoBehaviour.DestroyImmediate(_temp.gameObject);
                }
            }
        }

        /// <summary>
        /// 删除目录中所有文件和子目录
        /// </summary>
        /// <param name="srcPath"></param>
        public static void DelectDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static Component addScript(GameObject go, string script)
        {
            return go.AddComponent(Runtime.GetType(script));
        }


        public static List<T> AddTargetToList<T>(List<T> targetList, List<T> list) where T : Object
        {
            if (null == targetList || null == list)
            {
                Debug.LogError("param can not be null.");
                return null;
            }
            for (int i = 0; i < list.Count; ++i)
            {
                if (!targetList.Contains(list[i]))
                {
                    targetList.Add(list[i]);
                }
            }

            return targetList;
        }
        public static Tkey[] DicKeysToArray<Tkey, Tvalue>(Dictionary<Tkey, Tvalue> dic)
        {

            if (null == dic)
            {
                return null;
            }
            int _count = dic.Keys.Count;
            if (_count > 0)
            {
                Tkey[] _array = new Tkey[_count];
                dic.Keys.CopyTo(_array, 0);
                return _array;
            }
            else
            {
                return null;
            }
        }

        public static List<Tkey> DicKeysToList<Tkey, Tvalue>(Dictionary<Tkey, Tvalue> dic)
        {
            List<Tkey> _list = new List<Tkey>();
            Tkey[] _array = DicKeysToArray(dic);
            if (null != _array)
            {
                for (int i = 0; i < _array.Length; ++i)
                {
                    _list.Add(_array[i]);
                }
            }
            return _list;
        }

        /// <summary>
        /// 字典values 转为数组
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="Tvalue"></typeparam>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static Tvalue[] DicValuesToArray<Tkey, Tvalue>(Dictionary<Tkey, Tvalue> dic)
        {
            if (null == dic)
            {
                return null;
            }
            int _count = dic.Values.Count;
            if (_count > 0)
            {
                Tvalue[] _array = new Tvalue[_count];
                dic.Values.CopyTo(_array, 0);
                return _array;
            }
            else
            {
                return null;
            }
        }

        public static List<Tvalue> DicValuesToList<Tkey, Tvalue>(Dictionary<Tkey, Tvalue> dic)
        {
            List<Tvalue> _list = new List<Tvalue>();
            Tvalue[] _array = DicValuesToArray(dic);
            if (null != _array)
            {
                for (int i = 0; i < _array.Length; ++i)
                {
                    _list.Add(_array[i]);
                }
            }
            return _list;
        }



        public static void SetLayer(Transform target, int layer)
        {
            if (null != target)
            {
                if (target.gameObject.layer != layer)
                {
                    target.gameObject.layer = layer;
                }
                int _length = target.childCount;

                for (int i = 0; i < _length; ++i)
                {
                    Transform _temp = target.GetChild(i);
                    if (_temp.gameObject.layer != layer)
                    {
                        _temp.gameObject.layer = layer;
                    }
                    SetLayer(_temp, layer);
                }
            }

        }

        public static int GetLayer(Transform target)
        {

            if (null != target)
            {
                return target.gameObject.layer;
            }
            else
            {
                return LayerMask.NameToLayer("Default");
            }
        }

        public static byte[] Decrypty(byte[] source, int offset, byte code)
        {
            if (null == source || 0 == source.Length)
            {
                return new byte[0];
            }
            int filelen = source.Length;
            byte[] buffer = new byte[filelen - offset];
            for (int i = 0; i < filelen; ++i)
            {
                if (i < offset)
                {
                    continue;
                }
                if (i > offset && i < offset + 32)
                {
                    buffer[i - offset] = (byte)(source[i] ^ code);
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        buffer[i - offset] = (byte)(source[i] ^ code);
                    }
                    else
                    {
                        buffer[i - offset] = source[i];
                    }
                }
            }

            return buffer;
        }
    }




}