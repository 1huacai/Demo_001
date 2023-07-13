using XCore.Utils;

namespace XCore.IO
{
    using System;

    /**
     * @author 刘耀鑫
     */
    public class ByteBuffer : ICloneable
    {

        /* static fields */
        /** 默认的初始容量大小 */
        private const int capacity = 32;
        /** 默认的动态数据或文字的最大长度，1M */
        public const int maxDataLength = 1024 * 1024;
        /** 零长度的字节数组 */
        public static readonly byte[] EMPTY_ARRAY = { };
        /** 零长度的字符串 */
        public const string emptyString = "";

        /* fields */
        /** 字节数组 */
        private byte[] mArray;
        /** 字节缓存的长度 */
        private int mTop;
        /** 字节缓存的偏移量 */
        private int mPosition;

        public int Top
        {
            get { return mTop; }
            set
            {
                if (value < Position)
                    throw new Exception(this + " SetTop, invalid top:" + value);
                if (value > mArray.Length)
                    SetCapacity(value);
                mTop = value;
            }
        }

        public int Position
        {
            get { return mPosition; }
            set
            {
                if (value < 0 || value > Top)
                    throw new Exception(this
                                        + " setOffset, invalid offset:" + value);
                mPosition = value;
            }
        }

        public ByteBuffer()
            : this(capacity)
        {
        }

        public ByteBuffer(int capacity)
        {
            if (capacity < 1)
                throw new Exception(this
                                    + " <init>, invalid capatity:" + capacity);
            mArray = new byte[capacity];
            Top = 0;
            Position = 0;
        }

        public ByteBuffer(byte[] data)
        {
            if (data == null)
                throw new Exception(this
                                    + " <init>, null data");
            mArray = data;
            Top = data.Length;
            Position = 0;
        }

        public ByteBuffer(byte[] data, int index, int length)
        {
            if (data == null)
                throw new Exception(this
                                    + " <init>, null data");
            if (index < 0 || index > data.Length)
                throw new Exception(this
                                    + " <init>, invalid index:" + index);
            if (length < 0 || data.Length < index + length)
                throw new Exception(this
                                    + " <init>, invalid length:" + length);
            mArray = data;
            Top = index + length;
            Position = index;
        }

        public int Capacity()
        {
            return mArray.Length;
        }

        public void SetCapacity(int len)
        {
            int c = mArray.Length;
            if (len <= c)
                return;
            for (; c < len; c = (c << 1) + 1)
            {
            }
            byte[] temp = new byte[c];
            Buffer.BlockCopy(mArray, 0, temp, 0, Top);
            mArray = temp;
        }

        public int Length()
        {
            return Top - Position;
        }

        public byte[] GetArray()
        {
            return mArray;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = Top - 1; i >= 0; i--)
                hash = 65537 * hash + mArray[i];
            return hash;
        }

        public byte Read(int pos)
        {
            return mArray[pos];
        }

        public void Write(int b, int pos)
        {
            mArray[pos] = (byte)b;
        }

        public void Read(byte[] data, int pos, int len)
        {
            Buffer.BlockCopy(mArray, Position, data, pos, len);
            Position += len;
        }

        public bool ReadBoolean()
        {
            return mArray[Position++] != 0;
        }

        public byte ReadByte()
        {
            return mArray[Position++];
        }

        public int ReadUnsignedByte()
        {
            return mArray[Position++] & 0xff;
        }

        public char ReadChar()
        {
            return (char)ReadUnsignedShort();
        }

        public short ReadShort()
        {
            return (short)ReadUnsignedShort();
        }

        public int ReadUnsignedShort()
        {
            int pos = Position;
            Position += 2;
            return (mArray[pos + 1] & 0xff) + ((mArray[pos] & 0xff) << 8);
        }

        public int ReadInt()
        {
            int pos = Position;
            Position += 4;
            return (mArray[pos + 3] & 0xff) + ((mArray[pos + 2] & 0xff) << 8)
                   + ((mArray[pos + 1] & 0xff) << 16) + ((mArray[pos] & 0xff) << 24);
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt()), 0);
        }

        public long ReadLong()
        {
            int pos = Position;
            Position += 8;
            return (mArray[pos + 7] & 0xffL) + ((mArray[pos + 6] & 0xffL) << 8)
                   + ((mArray[pos + 5] & 0xffL) << 16) + ((mArray[pos + 4] & 0xffL) << 24)
                   + ((mArray[pos + 3] & 0xffL) << 32) + ((mArray[pos + 2] & 0xffL) << 40)
                   + ((mArray[pos + 1] & 0xffL) << 48) + ((mArray[pos] & 0xffL) << 56);
        }

        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        public int ReadLength()
        {
            int n = mArray[Position] & 0xff;
            if (n >= 0x80)
            {
                Position++;
                return n - 0x80;
            }
            if (n >= 0x40)
                return ReadUnsignedShort() - 0x4000;
            if (n >= 0x20)
                return ReadInt() - 0x20000000;
            throw new Exception(this
                                + " readLength, invalid number:" + n);
        }

        public byte[] ReadData()
        {
            int len = ReadLength() - 1;
            if (len < 0)
                return null;
            if (len > maxDataLength)
                throw new Exception(this
                                    + " readData, data overflow:" + len);
            if (len == 0)
                return EMPTY_ARRAY;
            byte[] data = new byte[len];
            Read(data, 0, len);
            return data;
        }

        public string ReadString()
        {
            return ReadString(null);
        }

        public string ReadString(string charsetName)
        {
            int len = ReadLength() - 1;
            if (len < 0)
                return null;
            if (len > maxDataLength)
                throw new Exception(this
                                    + " readString, data overflow:" + len);
            if (len == 0)
                return emptyString;
            byte[] data = new byte[len];
            Read(data, 0, len);
            if (charsetName == null)
                return System.Text.Encoding.Default.GetString(data);
            try
            {
                return System.Text.Encoding.GetEncoding(charsetName).GetString(data);
            }
            catch (Exception e)
            {
                throw new Exception(this
                                    + " readString, invalid charsetName:" + charsetName + " " + e);
            }
        }

        public string ReadUtf()
        {
            int len = ReadLength() - 1;
            if (len < 0)
                return null;
            if (len == 0)
                return emptyString;
            if (len > maxDataLength)
                throw new Exception(this
                                    + " readUTF, data overflow:" + len);
            string str = BytesUtils.ReadUtf(mArray, Position, len);
            Position += len;
            return str;
        }

        public void Write(byte[] data, int pos, int len)
        {
            if (len <= 0)
                return;
            if (mArray.Length < Top + len)
                SetCapacity(Top + len);
            Buffer.BlockCopy(data, pos, mArray, Top, len);
            Top += len;
        }

        public void WriteBoolean(bool b)
        {
            if (mArray.Length < Top + 1)
                SetCapacity(Top + capacity);
            mArray[Top++] = (byte)(b ? 1 : 0);
        }

        public void WriteByte(int b)
        {
            if (mArray.Length < Top + 1)
                SetCapacity(Top + capacity);
            mArray[Top++] = (byte)b;
        }

        public void WriteChar(int c)
        {
            WriteShort(c);
        }

        public void WriteShort(int s)
        {
            int pos = Top;
            if (mArray.Length < pos + 2)
                SetCapacity(pos + capacity);
            mArray[Top++] = (byte)(s >> 8 & 0xff);
            mArray[Top++] = (byte)(s & 0xff);
        }

        public void WriteInt(int i)
        {
            int pos = Top;
            if (mArray.Length < pos + 4)
                SetCapacity(pos + capacity);
            mArray[Top++] = (byte)(i >> 24 & 0xff);
            mArray[Top++] = (byte)(i >> 16 & 0xff);
            mArray[Top++] = (byte)(i >> 8 & 0xff);
            mArray[Top++] = (byte)(i & 0xff);
        }

        public void WriteFloat(float f)
        {
            WriteInt(BitConverter.ToInt32(BitConverter.GetBytes(f), 0));
        }

        public void WriteLong(long l)
        {
            int pos = Top;
            if (mArray.Length < pos + 8)
                SetCapacity(pos + capacity);
            byte[] temp = BitConverter.GetBytes(l);
            Array.Reverse(temp);
            Buffer.BlockCopy(temp, 0, mArray, pos, 8);
            Top += 8;
        }

        public void WriteDouble(double d)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(d));
        }

        public void WriteLength(int len)
        {
            if (len >= 0x20000000 || len < 0)
                throw new Exception(this
                                    + " writeLength, invalid len:" + len);
            if (len < 0x80)
                WriteByte(len + 0x80);
            else if (len < 0x4000)
                WriteShort(len + 0x4000);
            else
                WriteInt(len + 0x20000000);
        }

        public void WriteData(byte[] data)
        {
            WriteData(data, 0, data != null ? data.Length : 0);
        }

        public void WriteData(byte[] data, int pos, int len)
        {
            if (data == null)
            {
                WriteLength(0);
                return;
            }
            WriteLength(len + 1);
            Write(data, pos, len);
        }

        public void WriteString(string str)
        {
            WriteString(str, null);
        }

        public void WriteString(string str, string charsetName)
        {
            if (str == null)
            {
                WriteLength(0);
                return;
            }
            if (str.Length <= 0)
            {
                WriteLength(1);
                return;
            }
            byte[] data;
            if (charsetName != null)
            {
                try
                {
                    data = System.Text.Encoding.GetEncoding(charsetName).GetBytes(str);
                }
                catch (Exception e)
                {
                    //Log.error(null,e);
                    throw new Exception(this
                                        + " writeString, invalid charsetName:" + charsetName + " " + e);
                }
            }
            else
                data = System.Text.Encoding.Default.GetBytes(str);
            WriteLength(data.Length + 1);
            Write(data, 0, data.Length);
        }

        public void WriteUtf(string str)
        {
            if (str == null)
            {
                WriteLength(0);
                return;
            }
            int len = BytesUtils.GetUtfLength(str);
            WriteLength(len + 1);
            if (len <= 0)
                return;
            int pos = Top;
            if (mArray.Length < pos + len)
                SetCapacity(pos + len);
            BytesUtils.WriteUtf(str, mArray, pos);
            Top += len;
        }

        public byte[] ToArray()
        {
            byte[] data = new byte[Top - Position];
            Buffer.BlockCopy(mArray, Position, data, 0, data.Length);
            return data;
        }

        public void Clear()
        {
            Top = 0;
            Position = 0;
        }

        public object BytesRead(ByteBuffer data)
        {
            int len = data.ReadLength() - 1;
            if (len < 0)
                return null;
            if (len > maxDataLength)
                throw new Exception(this
                                    + " bytesRead, data overflow:" + len);
            if (mArray.Length < len)
                mArray = new byte[len];
            if (len > 0)
                data.Read(mArray, 0, len);
            Top = len;
            Position = 0;
            return this;
        }

        public void BytesWrite(ByteBuffer data)
        {
            data.WriteData(mArray, Position, Top - Position);
        }

        public object Clone()
        {
            try
            {
                ByteBuffer bb = (ByteBuffer)MemberwiseClone();
                byte[] array = bb.mArray;
                bb.mArray = new byte[bb.Top];
                Buffer.BlockCopy(array, 0, bb.mArray, 0, bb.Top);
                return bb;
            }
            catch (Exception e)
            {
                throw new Exception(this + " clone, capacity=" + mArray.Length, e);
            }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (!(obj is ByteBuffer))
                return false;
            ByteBuffer bb = (ByteBuffer)obj;
            if (bb.Top != Top)
                return false;
            if (bb.Position != Position)
                return false;
            for (int i = Top - 1; i >= 0; i--)
            {
                if (bb.mArray[i] != mArray[i])
                    return false;
            }
            return true;
        }

        public override string ToString()
        {
            return base.ToString() + "[" + Top + "," + Position + "," + mArray.Length + "]";
        }

        public void WriteBytes(byte[] b)
        {
            Write(b, 0, b.Length);
        }

        public void WriteBytes(byte[] b, int offset, int len)
        {
            Write(b, offset, len);
        }

        public void WriteBytes(ByteBuffer b, uint offset, uint len)
        {
            Write(b.ToArray(), (int)offset, (int)len);
        }

        public void ReadBytes(ByteBuffer data, int pos, int len)
        {
            //如果被写入的数据过多 则扩大容器
            if (data.mArray.Length < pos + len + 1)
                data.SetCapacity(data.Top + capacity + len);
            Buffer.BlockCopy(mArray, Position, data.mArray, pos, len);
            Position += len;
            data.Top = len;
            data.Position = pos;
        }

        public void ReadBytes(byte[] bytes, int pos, int len)
        {
            Buffer.BlockCopy(mArray, Position, bytes, pos, len);
            Position += len;
        }
    }
}
