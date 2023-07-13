using System;
using System.Collections.Generic;
using size = System.Byte;

namespace FrameWork
{
    public interface IConfigManager
    {
        T GetConfigFromDic<T>(long id) where T : Config;
    }

    public interface ISerializable
    {
        void Serialize(BinaryBuffer w);
    }

    public partial class BinaryBuffer
    {
        private byte[] _buffer;
        private int _position;
        private int _length;
        private int _maxPos;
        private int _origin;

        public int Position
        {
            get { return this._position; }
            set { this._position = value; }
        }

        public int Origin
        {
            get { return this._origin; }
        }

        public int Length
        {
            get { return this._length; }
        }

        public byte[] Buffer
        {
            get { return this._buffer; }
        }

        public BinaryBuffer(int capacity) : this(new byte[capacity], 0, capacity)
        {
        }

        public BinaryBuffer(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        public BinaryBuffer(byte[] buffer, int offset, int count)
        {
            Refill(buffer, offset, count);
        }

        public void Refill(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length == 0 || offset + count > buffer.Length || offset < 0 || count < 1)
                throw new ArgumentOutOfRangeException(string.Format("offset {0} count {1} buffer length {2}", offset, count, buffer.Length));
            this._buffer = buffer;
            _origin = _position = offset;
            _length = count;
            _maxPos = offset + count;
        }

        public void Refill(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            Refill(buffer, 0, buffer.Length);
        }

        private void CheckRange(int delta)
        {
            if (_position + delta > _maxPos)
                throw new IndexOutOfRangeException();
        }
        #region read
        private ulong Read(int count)
        {
            ulong temp = 0L;
            for (int i = 0; i < count; i++)
            {
                temp <<= 8;
                temp |= _buffer[_position++];
            }
            return temp;
        }

        private byte[] ReadRawBytes(int count)
        {
            CheckRange(count);
            byte[] rawbytes = new byte[count];
            ByteArray.Copy(_buffer, _position, rawbytes, 0, count);
            _position += count;
            return rawbytes;
        }

        public void Read(out bool value)
        {
            value = ReadBoolean();
        }

        public bool ReadBoolean()
        {
            CheckRange(1);
            return _buffer[_position++] == 1;
        }

        public void Read(out bool[] value)
        {
            value = ReadBooleanA();
        }

        public bool[] ReadBooleanA()
        {
            size len;
            Read(out len);
            bool[] value = new bool[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out byte value)
        {
            value = ReadByte();
        }

        public byte ReadByte()
        {
            CheckRange(1);
            return _buffer[_position++];
        }

        public void Read(out sbyte value)
        {
            value = ReadSbyte();
        }

        public sbyte ReadSbyte()
        {
            CheckRange(1);
            return (sbyte)_buffer[_position++];
        }

        public void Read(out byte[] value)
        {
            ushort len;
            Read(out len);
            CheckRange(len);
            value = new byte[len];
            ByteArray.Copy(_buffer, _position, value, 0, len);
            _position += len;
        }

        public void Read(out short value)
        {
            value = ReadInt16();
        }

        public short ReadInt16()
        {
            CheckRange(2);
            return (short)Read(2);
        }

        public void Read(out ushort value)
        {
            value = ReadUInt16();
        }

        public ushort ReadUInt16()
        {
            CheckRange(2);
            return (ushort)Read(2);
        }

        public void Read(out int value)
        {
            value = ReadInt32();
        }

        public int ReadInt32()
        {
            CheckRange(4);
            return (int)Read(4);
        }

        public void Read(out int[] value)
        {
            value = ReadInt32A();
        }

        public int[] ReadInt32A()
        {
            size len;
            Read(out len);
            var value = new int[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out int[][] value)
        {
            value = ReadInt32AA();
        }

        public int[][] ReadInt32AA()
        {
            size len1;
            Read(out len1);
            var value = new int[len1][];
            for (int i = 0; i < len1; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out int[][][] value)
        {
            value = ReadInt32AAA();
        }

        public int[][][] ReadInt32AAA()
        {
            size len1;
            Read(out len1);
            var value = new int[len1][][];
            for (int i = 0; i < len1; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out uint value)
        {
            value = ReadUInt32();
        }

        public uint ReadUInt32()
        {
            CheckRange(4);
            return (uint)Read(4);
        }

        public void Read(out uint[] value)
        {
            value = ReadUInt32A();
        }

        public uint[] ReadUInt32A()
        {
            size len;
            Read(out len);
            var value = new uint[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out uint[][] value)
        {
            value = ReadUInt32AA();
        }

        public uint[][] ReadUInt32AA()
        {
            size len;
            Read(out len);
            var value = new uint[len][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out uint[][][] value)
        {
            value = ReadUInt32AAA();
        }

        public uint[][][] ReadUInt32AAA()
        {
            size len;
            Read(out len);
            var value = new uint[len][][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out long value)
        {
            value = ReadInt64();
        }

        public long ReadInt64()
        {
            CheckRange(8);
            return (long)Read(8);
        }

        public void Read(out long[] value)
        {
            value = ReadInt64A();
        }

        public long[] ReadInt64A()
        {
            size len;
            Read(out len);
            var value = new long[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out long[][] value)
        {
            value = ReadInt64AA();
        }

        public long[][] ReadInt64AA()
        {
            size len;
            Read(out len);
            var value = new long[len][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out long[][][] value)
        {
            size len1;
            Read(out len1);
            value = new long[len1][][];
            for (int i = 0; i < len1; i++)
                Read(out value[i]);
        }

        public long[][][] ReadInt64AAA()
        {
            size len;
            Read(out len);
            var value = new long[len][][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out ulong value)
        {
            value = ReadUInt64();
        }

        public ulong ReadUInt64()
        {
            CheckRange(8);
            return Read(8);
        }

        public void Read(out ulong[] value)
        {
            value = ReadUInt64A();
        }

        public ulong[] ReadUInt64A()
        {
            size len;
            Read(out len);
            var value = new ulong[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out ulong[][] value)
        {
            value = ReadUInt64AA();
        }

        public ulong[][] ReadUInt64AA()
        {
            size len;
            Read(out len);
            var value = new ulong[len][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out ulong[][][] value)
        {
            value = ReadUInt64AAA();
        }

        public ulong[][][] ReadUInt64AAA()
        {
            size len;
            Read(out len);
            var value = new ulong[len][][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out float value)
        {
            value = ReadSingle();
        }

        public float ReadSingle()
        {
            float value;
            if (!BitConverter.IsLittleEndian)
            {
                var bytes = ReadRawBytes(4);
                ByteArray.Reverse(bytes);
                value = BitConverter.ToSingle(bytes, 0);
            }
            else
            {
                CheckRange(4);
                value = BitConverter.ToSingle(_buffer, _position);
                _position += 4;
            }
            return value;
        }

        public void Read(out float[] value)
        {
            value = ReadSingleA();
        }

        public float[] ReadSingleA()
        {
            size len;
            Read(out len);
            var value = new float[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out float[][] value)
        {
            value = ReadSingleAA();
        }

        public float[][] ReadSingleAA()
        {
            size len;
            Read(out len);
            var value = new float[len][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out float[][][] value)
        {
            value = ReadSingleAAA();
        }

        public float[][][] ReadSingleAAA()
        {
            size len;
            Read(out len);
            var value = new float[len][][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out double value)
        {
            value = ReadDouble();
        }

        public double ReadDouble()
        {
            long temp;
            Read(out temp);
            return BitConverter.Int64BitsToDouble(temp);
        }

        public void Read(out double[] value)
        {
            value = ReadDoubleA();
        }

        public List<double> ReadDoubleList()
        {
            size len;
            Read(out len);
            var value = new List<double>(len);
            for (int i = 0; i < len; i++)
                value.Add(ReadDouble());
            return value;
        }

        public double[] ReadDoubleA()
        {
            size len;
            Read(out len);
            var value = new double[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out double[][] value)
        {
            size len1;
            Read(out len1);
            value = new double[len1][];
            for (int i = 0; i < len1; i++)
                Read(out value[i]);
        }

        public double[][] ReadDoubleAA()
        {
            size len;
            Read(out len);
            var value = new double[len][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out double[][][] value)
        {
            size len1;
            Read(out len1);
            value = new double[len1][][];
            for (int i = 0; i < len1; i++)
                Read(out value[i]);
        }

        public double[][][] ReadDoubleAAA()
        {
            size len;
            Read(out len);
            var value = new double[len][][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out string value)
        {
            value = ReadString();
        }

        public string ReadString()
        {
            string value;
            if (!BitConverter.IsLittleEndian)
            {
                byte[] rawbytes;
                Read(out rawbytes);
                ByteArray.Reverse(rawbytes);
                value = SmartUT8.GetString(rawbytes, 0, rawbytes.Length);//System.Text.Encoding.UTF8.GetString(rawbytes);
            }
            else
            {
                ushort len;
                Read(out len);
                value = SmartUT8.GetString(_buffer, _position, len);//System.Text.Encoding.UTF8.GetString(_buffer, _position, len);
                _position += len;
            }
            return value;
        }

        public void Read(out string[] value)
        {
            size len;
            Read(out len);
            value = new string[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
        }

        public string[] ReadStringA()
        {
            size len;
            Read(out len);
            var value = new string[len];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out string[][] value)
        {
            size len;
            Read(out len);
            value = new string[len][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
        }

        public string[][] ReadStringAA()
        {
            size len;
            Read(out len);
            var value = new string[len][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public void Read(out string[][][] value)
        {
            size len;
            Read(out len);
            value = new string[len][][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
        }

        public string[][][] ReadStringAAA()
        {
            size len;
            Read(out len);
            var value = new string[len][][];
            for (int i = 0; i < len; i++)
                Read(out value[i]);
            return value;
        }

        public T ReadEnum<T>(Func<long, T> toEnum)
        {
            long temp;
            Read(out temp);
            return toEnum(temp);
        }

        public T[] ReadEnumA<T>(Func<long, T> toEnum)
        {
            long[] temp = ReadInt64A();
            T[] enumarray = new T[temp.Length];
            for (int i = 0; i < temp.Length; i++)
                enumarray[i] = toEnum(temp[i]);
            return enumarray;
        }

        public List<T> ReadEnumList<T>(Func<long, T> toEnum)
        {
            long[] temp = ReadInt64A();
            List<T> enumarray = new List<T>(temp.Length);
            for (int i = 0; i < temp.Length; i++)
                enumarray.Add(toEnum(temp[i]));
            return enumarray;
        }

        public VarDouble ReadVarDouble(ref bool hasVariant)
        {
            VarDouble temp = new VarDouble();
            if (this.ReadByte() > 0)
                temp.Deserialize(this);
            hasVariant |= temp.HasVariants;
            return temp;
        }

        public VarDoubleA ReadVarDoubleA(ref bool hasVariant)
        {
            VarDoubleA temp = new VarDoubleA();
            if (this.ReadByte() > 0)
                temp.Deserialize(this);
            hasVariant |= temp.HasVariants;
            return temp;
        }

        public VarDoubleAA ReadVarDoubleAA(ref bool hasVariant)
        {
            VarDoubleAA temp = new VarDoubleAA();
            if (this.ReadByte() > 0)
                temp.Deserialize(this);
            hasVariant |= temp.HasVariants;
            return temp;
        }

        public List<VarDouble> ReadVarDoubleList(ref bool hasVariant)
        {
            size len;
            Read(out len);
            List<VarDouble> list = new List<VarDouble>(len);
            for (int i = 0; i < len; i++)
                list.Add(ReadVarDouble(ref hasVariant));
            return list;
        }
        #endregion
        #region write
        private void Write(ulong value, int count)
        {
            for (int i = count - 1; i >= 0; i--)
                _buffer[_position++] = (byte)(value >> (i * 8));
        }

        public void Write(bool value)
        {
            CheckRange(1);
            if (value)
                _buffer[_position++] = 1;
            else
                _buffer[_position++] = 0;
        }

        public void Write(bool[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(byte value)
        {
            CheckRange(1);
            _buffer[_position++] = value;
        }

        public void Write(byte[] value)
        {
            Write((ushort)value.Length);
            CheckRange(value.Length);
            ByteArray.Copy(value, 0, _buffer, _position, value.Length);
            _position += value.Length;
        }

        public void Write(sbyte value)
        {
            CheckRange(1);
            _buffer[_position++] = (byte)value;
        }

        public void Write(short value)
        {
            CheckRange(2);
            Write((ushort)value, 2);
        }

        public void Write(ushort value)
        {
            CheckRange(2);
            Write(value, 2);
        }

        public void Write(int value)
        {
            CheckRange(4);
            Write((uint)value, 4);
        }

        public void Write(int[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(int[][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(int[][][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(uint value)
        {
            CheckRange(4);
            Write(value, 4);
        }

        public void Write(uint[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(uint[][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(uint[][][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(long value)
        {
            CheckRange(8);
            Write((ulong)value, 8);
        }

        public void Write(long[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(long[][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(long[][][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(ulong value)
        {
            CheckRange(8);
            Write(value, 8);
        }

        public void Write(ulong[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(ulong[][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(ulong[][][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(float value)
        {
            CheckRange(4);
            byte[] rawBytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                ByteArray.Reverse(rawBytes);
            _buffer[_position++] = rawBytes[0];
            _buffer[_position++] = rawBytes[1];
            _buffer[_position++] = rawBytes[2];
            _buffer[_position++] = rawBytes[3];
        }

        public void Write(float[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(float[][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(float[][][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(double value)
        {
            long temp = BitConverter.DoubleToInt64Bits(value);
            Write(temp);
        }

        public void Write(List<double> value)
        {
            Write((size)value.Count);
            for (int i = 0; i < value.Count; i++)
                Write(value[i]);
        }

        public void Write(double[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(double[][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(double[][][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(string value)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(string.IsNullOrEmpty(value) ? "" : value);
            if (!BitConverter.IsLittleEndian)
                ByteArray.Reverse(bytes);
            Write(bytes);
        }

        public void Write(string[] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(string[][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write(string[][][] value)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void WriteEnum<T>(T value, Func<T, int> fromEnum)
        {
            Write(fromEnum(value));
        }
        public void WriteEnum<T>(T value, Func<T, long> fromEnum)
        {
            Write(fromEnum(value));
        }
        public void WriteEnumA<T>(T[] value, Func<T, long> fromEnum)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                WriteEnum(value[i], fromEnum);

        }

        public void WriteEnumList<T>(List<T> value, Func<T, long> fromEnum)
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Count);
            for (int i = 0; i < value.Count; i++)
                WriteEnum(value[i], fromEnum);
        }

        public void Write<T>(List<T> value) where T : ISerializable
        {
            Write((size)value.Count);
            for (int i = 0; i < value.Count; i++)
                Write(value[i]);
        }

        public void Write<T>(T value) where T : ISerializable
        {
            if (value == null)
            {
                Write((byte)0);//标记是否有
                return;
            }
            Write((byte)1);
            value.Serialize(this);
        }

        public void Write<T>(T[] value) where T : ISerializable
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        public void Write<T>(T[][] value) where T : ISerializable
        {
            if (value == null)
            {
                Write((size)0);
                return;
            }
            Write((size)value.Length);
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }
        #endregion
    }

    internal static class ByteArray
    {
        /// <summary>
        /// The threshold above which you should use Buffer.BlockCopy rather than ByteArray.Copy
        /// </summary>
        private const int CopyThreshold = 12;

        /// <summary>
        /// Determines which copy routine to use based on the number of bytes to be copied.
        /// </summary>
        internal static void Copy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (count > CopyThreshold)
            {
                Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
            }
            else
            {
                int stop = srcOffset + count;
                for (int i = srcOffset; i < stop; i++)
                {
                    dst[dstOffset++] = src[i];
                }
            }
        }

        /// <summary>
        /// Reverses the order of bytes in the array
        /// </summary>
        internal static void Reverse(byte[] bytes)
        {
            for (int first = 0, last = bytes.Length - 1; first < last; first++, last--)
            {
                byte temp = bytes[first];
                bytes[first] = bytes[last];
                bytes[last] = temp;
            }
        }
    }

    public partial class BinaryBuffer
    {
        public T ReadConfig<T>(ref bool hasVariant, VaryingExtension ex = null) where T : Config
        {
            int id = ReadInt32();
            if (id == 0)
                return null;
            T t = ConfigManager.Instance.GetConfigFromDic<T>(id);
            if (t != null)
                hasVariant |= t.HasVariants;
            return t;
        }

        public T[] ReadConfigA<T>(ref bool hasVariant, VaryingExtension ex = null) where T : Config
        {
            size len;
            Read(out len);
            var value = new T[len];
            for (int i = 0; i < len; i++)
                value[i] = ReadConfig<T>(ref hasVariant, ex);
            return value;
        }

        public T[][] ReadConfigAA<T>(ref bool hasVariant, VaryingExtension ex = null) where T : Config
        {
            size len;
            Read(out len);
            var value = new T[len][];
            for (int i = 0; i < len; i++)
                value[i] = ReadConfigA<T>(ref hasVariant, ex);
            return value;
        }

        public List<T> ReadConfigList<T>(ref bool hasVariant, VaryingExtension ex = null) where T : Config
        {
            size len;
            Read(out len);
            var value = new List<T>(len);
            for (int i = 0; i < len; i++)
                value.Add(ReadConfig<T>(ref hasVariant, ex));
            return value;
        }

        public void WriteConfig<T>(T config) where T : Config
        {
            Write(config == null ? 0 : (int)config.id);
        }

        public void WriteConfigA<T>(T[] config) where T : Config
        {
            int len = config == null ? 0 : config.Length;
            Write((size)len);
            for (int i = 0; i < len; i++)
            {
                WriteConfig(config[i]);
            }
        }

        public void WriteConfigAA<T>(T[][] config) where T : Config
        {
            int len = config == null ? 0 : config.Length;
            Write((size)len);
            for (int i = 0; i < len; i++)
            {
                WriteConfigA(config[i]);
            }
        }

        public void WriteConfigList<T>(List<T> config) where T : Config
        {
            int len = config == null ? 0 : config.Count;
            Write((size)len);
            for (int i = 0; i < len; i++)
            {
                WriteConfig(config[i]);
            }
        }

        public void Encrypt(string key, int offset = 0)
        {
            if (string.IsNullOrEmpty(key))
                return;
            Encrypt(System.Text.Encoding.UTF8.GetBytes(key), offset);
        }

        public void Encrypt(byte[] keyBytes, int offset = 0)
        {
            for (int i = 0; i < keyBytes.Length; i++)//key混淆
                keyBytes[i] ^= (byte)keyBytes.Length;
            DoXOR(keyBytes, offset);
        }

        public void Decrypt(string key, int offset = 0)
        {
            if (string.IsNullOrEmpty(key))
                return;
            Decrypt(System.Text.Encoding.UTF8.GetBytes(key), offset);
        }

        public void Decrypt(byte[] keyBytes, int offset = 0)
        {
            for (int i = 0; i < keyBytes.Length; i++)//key混淆
                keyBytes[i] ^= (byte)keyBytes.Length;
            DoXOR(keyBytes, offset);
        }

        private void DoXOR(byte[] keyBytes, int offset)
        {
            if (keyBytes == null || keyBytes.Length == 0 || offset < 0)
                return;
            var rawLen = _buffer.Length;
            var keylen = keyBytes.Length;
            for (int i = offset; i < rawLen; i++)
                _buffer[i] ^= keyBytes[(i - offset) % keylen];
        }
    }
}
