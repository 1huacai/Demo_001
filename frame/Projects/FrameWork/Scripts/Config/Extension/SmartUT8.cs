using System;
using System.Text;

namespace FrameWork
{
    public class SmartUT8
    {
        private const int MaxCharBytesSize = 128;
        private static char[] m_charBuffer;
        private static StringBuilder sb;
        private static Decoder m_decoder;
        private static object locker = new object();

        public static string GetString(byte[] bytes, int pos, int length)
        {
            lock (locker)
            {
                int currPos = 0;
                int readLength;
                int charsRead;

                // Length of the string in bytes, not chars
                if (length < 0 || pos + length > bytes.Length)
                    throw new IndexOutOfRangeException("string length can not be negative!");

                if (length == 0)
                    return String.Empty;

                if (m_decoder == null)
                    m_decoder = Encoding.UTF8.GetDecoder();

                if (m_charBuffer == null)
                    m_charBuffer = new char[Encoding.UTF8.GetMaxCharCount(MaxCharBytesSize)];

                if (sb == null)
                    sb = new StringBuilder(length);
                else
                    sb.EnsureCapacity(length);
                sb.Length = 0;

                do
                {
                    readLength = ((length - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (length - currPos);
                    charsRead = m_decoder.GetChars(bytes, pos + currPos, readLength, m_charBuffer, 0);
                    if (currPos == 0 && readLength == length)
                        return new String(m_charBuffer, 0, charsRead);
                    sb.Append(m_charBuffer, 0, charsRead);
                    currPos += readLength;

                } while (currPos < length);
                return sb.ToString();
            }
        }
    }
}
