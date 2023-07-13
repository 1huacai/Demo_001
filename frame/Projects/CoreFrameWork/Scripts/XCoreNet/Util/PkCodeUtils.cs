using System;

namespace XCore.Utils
{
    public static class PkCodeUtils
    {
        /***/
        private const int rMask = 123459876;
        /***/
        private const int rA = 16807;
        private const int rQ = 127773;

        /** 获取指定种子的密码 */
        public static int[] GetPkCode(int seed)
        {
            int seed1 = GetRandom(seed + 11);
            int seed2 = GetRandom(seed1 + 13);
            int seed3 = GetRandom(seed2 + 17);
            int seed4 = GetRandom(seed3 + 19);
            int seed5 = GetRandom(seed4 + 23);
            int seed6 = GetRandom(seed5 + 29);
            int seed7 = GetRandom(seed6 + 31);
            int seed8 = GetRandom(seed7 + 37);
            return new int[] { seed1, seed2, seed3, seed4, seed5, seed6, seed7, seed8 };
        }

        public static void Code(byte[] bytes, int index, int length, byte[] keys)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (bytes.Length < 1 || keys.Length < 1)
                return;
            int blength = index + length;
            int klength = keys.Length;
            int j = 0;
            for (int i = index; i < blength; i++)
            {
                if (j == klength) j = 0;
                int k = bytes[i] ^ keys[j];
                k <<= 24;
                k >>= 24;
                bytes[i] = (byte)k;
                j++;
            }
        }

        /** 获取指定种子的随机数 */
        private static int GetRandom(int seed)
        {
            int r = seed ^ rMask;
            int s = rA * r - (int)System.Math.Round((double)(r / rQ)) * int.MaxValue;
            if (s < 0)
                return s + int.MaxValue;
            return s;
        }

        /** 获取下一个密码 */
        public static void NextPk(int[] pk, byte[] bytes)
        {
            if (pk == null || bytes == null)
                return;
            for (int i = 0, length = pk.Length; i < length; i++)
            {
                pk[i] = GetRandom(pk[i]);
            }
            ToPk(pk, bytes);
        }

        /** 获取指定密码的字节数组 */
        public static void ToPk(int[] pks, byte[] bytes)
        {
            if (pks == null) throw new ArgumentNullException(nameof(pks));
            BytesUtils.WriteInt(pks[0], bytes, 0);
            BytesUtils.WriteInt(pks[1], bytes, 4);
            BytesUtils.WriteInt(pks[2], bytes, 8);
            BytesUtils.WriteInt(pks[3], bytes, 12);
            BytesUtils.WriteInt(pks[4], bytes, 16);
            BytesUtils.WriteInt(pks[5], bytes, 20);
            BytesUtils.WriteInt(pks[6], bytes, 24);
            BytesUtils.WriteInt(pks[7], bytes, 28);
        }
    }
}