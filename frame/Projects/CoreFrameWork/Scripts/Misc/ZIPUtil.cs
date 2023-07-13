using System;  
using System.IO;  
using zlib;

namespace CoreFrameWork.Misc
{
    public class ZIPUtil
    {
        public ZIPUtil()
        {
        }

        //zlib—πÀı
        public static byte[] Compress(byte[] inputBytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZOutputStream zOut = new ZOutputStream(ms, 0))
                {
                    zOut.Write(inputBytes, 0, inputBytes.Length);
                    zOut.finish();
                    return ms.ToArray();
                }
            }
        }

        public static byte[] Decompress(Byte[] inputBytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZOutputStream zOut = new ZOutputStream(ms))
                {
                    zOut.Write(inputBytes, 0, inputBytes.Length);
                    zOut.finish();
                    return ms.ToArray();
                }
            }

        }
    }

}
