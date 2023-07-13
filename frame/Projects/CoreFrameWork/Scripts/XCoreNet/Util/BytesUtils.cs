using System.Text;

namespace XCore.Utils
{
    public static class BytesUtils
    {
        public static short ReadShort(byte[] bytes, int startIndex = 0, bool isLittleEndian = false)
        {
            return (short)ReadUnsignedShort(bytes, startIndex, isLittleEndian);
        }

        public static int ReadUnsignedShort(byte[] bytes, int startIndex = 0, bool isLittleEndian = false)
        {
            if (isLittleEndian)
            {
                return ((bytes[startIndex + 1] & 0xff) << 8) + (bytes[startIndex] & 0xff);
            }
            return (bytes[startIndex + 1] & 0xff) + ((bytes[startIndex] & 0xff) << 8);
        }

        public static int ReadInt(byte[] bytes, int startIndex = 0, bool isLittleEndian = false)
        {
            if (isLittleEndian)
            {
                return ((bytes[startIndex + 3] & 0xff) << 24) + ((bytes[startIndex + 2] & 0xff) << 16)
                   + ((bytes[startIndex + 1] & 0xff) << 8) + (bytes[startIndex] & 0xff);
            }
            return (bytes[startIndex + 3] & 0xff) + ((bytes[startIndex + 2] & 0xff) << 8)
                   + ((bytes[startIndex + 1] & 0xff) << 16) + ((bytes[startIndex] & 0xff) << 24);
        }

        public static string ReadUtf(byte[] data)
        {
            return ReadUtf(data, 0, data.Length);
        }

        public static string ReadUtf(byte[] data, int pos, int length)
        {
            return Encoding.UTF8.GetString(data, pos, length);
        }

        public static void WriteInt(int i, byte[] bytes, int pos, bool isLittleEndian = false)
        {
            if (isLittleEndian)
            {
                bytes[pos++] = (byte)(i & 0xff);
                bytes[pos++] = (byte)(i >> 8 & 0xff);
                bytes[pos++] = (byte)(i >> 16 & 0xff);
                bytes[pos++] = (byte)(i >> 24 & 0xff);
            }
            else
            {
                bytes[pos++] = (byte)(i >> 24 & 0xff);
                bytes[pos++] = (byte)(i >> 16 & 0xff);
                bytes[pos++] = (byte)(i >> 8 & 0xff);
                bytes[pos++] = (byte)(i & 0xff);
            }
        }

        public static void WriteShort(int i, byte[] bytes, int pos, bool isLittleEndian = false)
        {
            if (isLittleEndian)
            {
                bytes[pos++] = (byte)(i & 0xff);
                bytes[pos++] = (byte)(i >> 8 & 0xff);
            }
            else
            {
                bytes[pos++] = (byte)(i >> 8 & 0xff);
                bytes[pos++] = (byte)(i & 0xff);
            }
        }

        public static int GetUtfLength(string str)
        {
            return Encoding.UTF8.GetByteCount(str);
        }

        public static int GetUtfLength(char[] chars, int index, int len)
        {
            return Encoding.UTF8.GetByteCount(chars, index, len);
        }

        public static byte[] WriteUtf(string str)
        {
            byte[] data = new byte[GetUtfLength(str)];
            WriteUtf(str, data, 0);
            return data;
        }

        public static void WriteUtf(string str, byte[] data,
            int pos)
        {
            Encoding.UTF8.GetBytes(str, 0, str.Length, data, pos);
        }

        public static void WriteUtf(char[] chars, int index, int len, byte[] data,
            int pos)
        {
            Encoding.UTF8.GetBytes(chars, index, len, data, pos);
        }
    }
}