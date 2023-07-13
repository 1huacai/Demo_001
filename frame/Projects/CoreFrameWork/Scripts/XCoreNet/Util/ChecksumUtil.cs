namespace XCore.Utils
{
    public class ChecksumUtil
    {

        /**
             * @private
             */
        private static readonly uint[] CRC_TABLE = MakeCrcTable();

        /**
         * @private
         */
        private static uint[] MakeCrcTable()
        {
            uint[] table = new uint[256];

            uint i;
            uint j;
            uint c;
            for (i = 0; i < 256; i++)
            {
                c = i;
                for (j = 0; j < 8; j++)
                {
                    if ((c & 1) == 1)
                    {
                        c = 0xEDB88320 ^ (c >> 1);
                    }
                    else
                    {
                        c >>= 1;
                    }
                }

                table[i] = c;
            }
            return table;
        }

        /**
             * Calculates a CRC-32 checksum over a ByteArray
             * 
             * @see http://www.w3.org/TR/PNG/#D-CRCAppendix
             * 
             * @param _data 
             * @param len
             * @param start
             * @return CRC-32 checksum
             */
        public static uint Crc32(byte[] bytes, uint start, uint len)
        {
            uint i;
            uint c = 0xffffffff;
            for (i = start; i < len; i++)
            {
                c = CRC_TABLE[(c ^ bytes[i]) & 0xff] ^ (c >> 8);
            }
            return c ^ 0xffffffff;
        }

        /**
             * Calculates an Adler-32 checksum over a ByteArray
             * 
             * @see http://en.wikipedia.org/wiki/Adler-32#Example_implementation
             * 
             * @param _data 
             * @param len
             * @param start
             * @return Adler-32 checksum
             */
        public static uint Adler32(byte[] bytes)
        {
            return Adler32(bytes, 0, (uint)bytes.Length);
        }

        public static uint Adler32(byte[] bytes, uint start, uint len)
        {
            uint i = start;
            uint a = 1;
            uint b = 0;

            while (i < start + len)
            {
                a = (a + bytes[i]) % 65521;
                b = (a + b) % 65521;
                i++;
            }
            return (b << 16) | a;
        }
    }



}