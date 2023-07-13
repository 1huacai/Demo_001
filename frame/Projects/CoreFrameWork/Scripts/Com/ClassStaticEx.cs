//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
namespace CoreFrameWork.Com
{
    public static class ClassStaticEx
    {

        #region extends string class

        public static int ToInt(this string str)
        {
            int rt;
            int.TryParse(str, out rt);
            return rt;
        }
        public static uint ToUInt(this string str)
        {
            uint rt;
            uint.TryParse(str, out rt);
            return rt;
        }
        public static byte ToByte(this string str)
        {
            byte rt;
            byte.TryParse(str, out rt);
            return rt;
        }
        public static short ToShort(this string str)
        {
            short rt;
            short.TryParse(str, out rt);
            return rt;
        }
        public static float ToFloat(this string str)
        {
            float rt;
            float.TryParse(str, out rt);
            return rt;
        }
        public static double ToDouble(this string str)
        {
            double rt;
            double.TryParse(str, out rt);
            return rt;
        }
        public static long ToLong(this string str)
        {
            long rt;
            long.TryParse(str, out rt);
            return rt;
        }
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        #endregion

        #region extends Array class
        public static T[] Concat<T>(this T[] v, T[] arr)
        {
            int oldLen = v.Length;
            Array.Resize(ref v, v.Length + arr.Length);
            for (int i = oldLen; i < v.Length; i++)
            {
                v[i] = arr[i - oldLen];
            }
            return v;
        }
        public static T[] ShallowClone<T>(this T[] v)
        {
            if (v == null)
                return v;
            T[] rt = new T[v.Length];
            v.CopyTo(rt, 0);
            return rt;
        }
        public static T[][] ShallowClone<T>(this T[][] v)
        {
            if (v == null)
                return v;
            T[][] rt = new T[v.Length][];
            v.CopyTo(rt, 0);
            return rt;
        }

        public static void sort<T>(this T[] array, Comparison<T> comparison)
        {
            array.sort(0, array.Length - 1, comparison);
        }
        /// <summary>
        /// 直接反编译c#的复制过来的
        /// </summary>
        public static void sort<T>(this T[] array, int low0, int high0, Comparison<T> comparison)
        {
            int num;
            int num2;
            int num3;
            T local;
            if (low0 < high0)
            {
                goto Label_0008;
            }
            return;
            Label_0008:
            num = low0;
            num2 = high0;
            num3 = num + ((num2 - num) / 2);
            local = array[num3];
            Label_001C:
            goto Label_0025;
            Label_0021:
            num += 1;
            Label_0025:
            if (num >= high0)
            {
                goto Label_0049;
            }
            if (comparison(array[num], local) < 0)
            {
                goto Label_0021;
            }
            goto Label_0049;
            Label_0045:
            num2 -= 1;
            Label_0049:
            if (num2 <= low0)
            {
                goto Label_0064;
            }
            if (comparison(local, array[num2]) < 0)
            {
                goto Label_0045;
            }
            Label_0064:
            if (num > num2)
            {
                goto Label_008A;
            }
            swap<T>(array, num, num2);
            num += 1;
            num2 -= 1;
            goto Label_0085;
            goto Label_008A;
            Label_0085:
            goto Label_001C;
            Label_008A:
            if (low0 >= num2)
            {
                goto Label_009A;
            }
            sort<T>(array, low0, num2, comparison);
            Label_009A:
            if (num >= high0)
            {
                goto Label_00AA;
            }
            sort<T>(array, num, high0, comparison);
            Label_00AA:
            return;

        }

        public static void swap<T>(this T[] array, int i, int j)
        {
            T local;
            local = array[i];
            array[i] = array[j];
            array[j] = local;
            return;
        }
        #endregion

        #region extends list class
        public static void SafeSort<T>(this List<T> array, Comparison<T> comparison)
        {
            array.SafeSort(0, array.Count - 1, comparison);
        }

        public static void SafeSort<T>(this List<T> array, int low0, int high0, Comparison<T> comparison)
        {
            int num;
            int num2;
            int num3;
            T local;
            if (low0 < high0)
            {
                goto Label_0008;
            }
            return;
            Label_0008:
            num = low0;
            num2 = high0;
            num3 = num + ((num2 - num) / 2);
            local = array[num3];
            Label_001C:
            goto Label_0025;
            Label_0021:
            num += 1;
            Label_0025:
            if (num >= high0)
            {
                goto Label_0049;
            }
            if (comparison(array[num], local) < 0)
            {
                goto Label_0021;
            }
            goto Label_0049;
            Label_0045:
            num2 -= 1;
            Label_0049:
            if (num2 <= low0)
            {
                goto Label_0064;
            }
            if (comparison(local, array[num2]) < 0)
            {
                goto Label_0045;
            }
            Label_0064:
            if (num > num2)
            {
                goto Label_008A;
            }
            swap<T>(array, num, num2);
            num += 1;
            num2 -= 1;
            goto Label_0085;
            goto Label_008A;
            Label_0085:
            goto Label_001C;
            Label_008A:
            if (low0 >= num2)
            {
                goto Label_009A;
            }
            SafeSort<T>(array, low0, num2, comparison);
            Label_009A:
            if (num >= high0)
            {
                goto Label_00AA;
            }
            SafeSort<T>(array, num, high0, comparison);
            Label_00AA:
            return;
        }

        public static void swap<T>(this List<T> array, int i, int j)
        {
            T local;
            local = array[i];
            array[i] = array[j];
            array[j] = local;
            return;
        }

        public static string ToString<T>(this List<T> v, char split)
        {
            string str = "";
            for (int i = 0; i < v.Count; i++)
            {
                str += v[i].ToString() + split.ToString();
            }
            if (v.Count > 0)
                str = str.Substring(0, str.Length - 1);
            return str;
        }
        #endregion


    }
}
