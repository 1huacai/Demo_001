using CoreFrameWork;
using System.Collections.Generic;
using System.IO;
using XCore.IO;
using XCore.Utils;
using zlib;

namespace XCore.Net
{
    public class ErlTcpConnect : TcpConnect
    {
        private readonly object mCompressLock = new object();
        private readonly object mDecompressLock = new object();
        private readonly object mWaitBcQueueLock = new object();

        private const int defaultWaitBcQueueSize = 20;

        private const int defaultBcSize = 10;

        private static int SERIAL_NUMBER;

        private const string pingCmd = "myping";
        private int[] mSendPkCode;

        private int[] mReceivePkCode;

        /// <summary>
        /// 
        /// </summary>
        private bool mDefCode = true;
        /// <summary>
        /// 
        /// </summary>
        private bool mDefCrc = false;
        /// <summary>
        /// 
        /// </summary>
        private bool mDefCompress = false;

        private ByteBuffer mSendBuffer;

        private byte[] mNextSPkCode;
        private byte[] mNextRPkCode;
        private ZOutputStream mCompressBuffer;

        private MemoryStream mCompressMemoryBuffer;

        private ZOutputStream mDeCompressBuffer;

        private MemoryStream mDeCompressMemoryBuffer;

        private Queue<byte[]> mWaitBcQueue;
        private Queue<WaitBcStruct> mWaitBcStructQueue;

        private int mWaitBcSize = defaultWaitBcQueueSize;

        private int mBcSize = defaultBcSize;

        public static int SerialNumber
        {
            get
            {
                if (SERIAL_NUMBER <= short.MinValue)
                {
                    SERIAL_NUMBER = 0;
                }
                return SERIAL_NUMBER--;
            }
        }

        public bool DefCode
        {
            set { mDefCode = value; }
        }
        public bool DefCrc
        {
            set { mDefCrc = value; }
        }
        public bool DefCompress
        {
            set { mDefCompress = value; }
        }

        public int WaitBcSize
        {
            set { mWaitBcSize = value; }
            get { return mWaitBcSize; }
        }

        public int BcSize
        {
            set { mBcSize = value; }
            get { return mBcSize; }
        }

        protected override void Certify(byte[] bytes)
        {
            int a = BytesUtils.ReadInt(bytes);
            int b = BytesUtils.ReadInt(bytes, 4);
            mSendPkCode = PkCodeUtils.GetPkCode(a);
            mReceivePkCode = PkCodeUtils.GetPkCode(b);
            base.Certify(bytes);
        }

        public void Send(byte[] bytes, short cmd, int number)
        {
            if (SEND_BREAK || BREAK_SEND_MSGID == cmd)
                return;
            Send(bytes, cmd, number, mDefCode, mDefCrc, mDefCompress);
        }

        public void Send(byte[] bytes, short cmd, int number, bool isCode, bool isCrc, bool isCompress)
        {
            Pack(bytes, cmd, number, isCode, isCrc, isCompress);
            base.Send(mSendBuffer.GetArray(), 0, mSendBuffer.Top);
        }

        private const int BUFFER_MSG_LEN = 2;
        private const int BUFFER_MSGID_LEN = 2;

        protected void Pack(byte[] bytes, short cmd, int number, bool isCode, bool isCrc, bool isCompress)
        {
            if (bytes == null)
            {
                bytes = ByteBuffer.EMPTY_ARRAY;
            }
            if (mSendBuffer == null)
            {
                mSendBuffer = new ByteBuffer();
            }
            else
            {
                mSendBuffer.Top = 0;
            }
            int compress = isCompress && bytes.Length >= 32 ? 1 : 0;
            if (compress == 1)
            {
                bytes = Compress(bytes, 0, bytes.Length);
            }
            int crc = isCrc ? 1 : 0;
            int crcValue = 0;
            if (isCrc)
            {
                crcValue = (int)ChecksumUtil.Adler32(bytes);
            }
            int code = isCode ? 1 : 0;
            int cmdLength = 2; //Encoding.UTF8.GetByteCount(cmd);
            int crcLength = isCrc ? 4 : 0;
            int hLength = 2;
            int bHLength = cmdLength + 4;
            int bBLength = crcLength + bytes.Length;
            int bLength = bHLength + bBLength;
            int length = hLength + bLength;
            int vesion = (code << 3) | (crc << 2) | (compress << 1);
            //todo 需要验证bLength是否超过ushort上限
            mSendBuffer.SetCapacity(length);
            mSendBuffer.WriteShort(bLength);
            mSendBuffer.WriteByte(cmdLength);
            mSendBuffer.WriteShort(cmd);
            mSendBuffer.Top = mSendBuffer.Top;
            mSendBuffer.WriteShort(number);
            mSendBuffer.WriteByte(vesion);
            if (isCrc)
            {
                mSendBuffer.WriteInt(crcValue);
            }
            mSendBuffer.WriteBytes(bytes);
            if (isCode)
            {
                if (mNextSPkCode == null)
                {
                    mNextSPkCode = new byte[32];
                }
                PkCodeUtils.NextPk(mSendPkCode, mNextSPkCode);
                PkCodeUtils.Code(mSendBuffer.GetArray(), hLength + bHLength, bBLength, mNextSPkCode);
            }
        }

        protected override void UnPack()
        {
            base.UnPack();
            int cmdLength = bodyBuffer.ReadUnsignedByte();
            int cmd = bodyBuffer.ReadUnsignedShort();
            int number = bodyBuffer.ReadUnsignedShort();
            int vesion = bodyBuffer.ReadUnsignedByte();
            bool isCode = (vesion & 8) != 0;
            bool isCrc = (vesion & 4) != 0;
            bool isCompress = (vesion & 2) != 0;
            if (isCode)
            {
                if (mNextRPkCode == null)
                {
                    mNextRPkCode = new byte[32];
                }
                PkCodeUtils.NextPk(mReceivePkCode, mNextRPkCode);
                PkCodeUtils.Code(bodyBuffer.GetArray(), bodyBuffer.Position, bodyBuffer.Length(), mNextRPkCode);
            }
            if (isCrc)
            {
                bodyBuffer.ReadInt();//todo 可以不用验证后台的crc
            }
            byte[] bytes = null;
            if (isCompress)
            {
                bytes = Decompress(bodyBuffer.GetArray(), bodyBuffer.Position, bodyBuffer.Length());
            }
            else
            {
                bytes = bodyBuffer.ToArray();
            }
            AddWaitBcQueue(cmd, number, bytes);
        }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="inputBytes">输入数据</param>
        /// <param name="index">下标</param>
        /// <param name="length">长度</param>
        /// <returns>输出数据</returns>
        private byte[] Compress(byte[] inputBytes, int index, int length)
        {
            lock (mCompressLock)
            {
                if (mCompressBuffer == null)
                {
                    mCompressMemoryBuffer = new MemoryStream();
                    mCompressBuffer = new ZOutputStream(mCompressMemoryBuffer, zlibConst.Z_DEFAULT_COMPRESSION);
                }
                //mCompressBuffer.Clear();
                mCompressMemoryBuffer.Position = 0;
                mCompressBuffer.Write(inputBytes, index, length);
                mCompressBuffer.finish();
                int postion = (int)mCompressMemoryBuffer.Position;
                mCompressMemoryBuffer.Position = 0;
                byte[] bytes = new byte[postion];
                mCompressMemoryBuffer.Read(bytes, 0, postion);
                return bytes;
            }

        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="inputBytes">输入数据</param>
        /// <param name="index">下标</param>
        /// <param name="length">长度</param>
        /// <returns>输出数据</returns>
        private byte[] Decompress(byte[] inputBytes, int index, int length)
        {
            if (DEBUG_LOG)
            {
                Log.Debug("解压前数据"+ TextUtils.ToString(inputBytes, index, length)+ " length:"+ length);
            }
            lock (mDecompressLock)
            {
                if (mDeCompressBuffer == null)
                {
                    mDeCompressMemoryBuffer = new MemoryStream();
                    mDeCompressBuffer = new ZOutputStream(mDeCompressMemoryBuffer);
                }
                //mDeCompressBuffer.Clear();
                mDeCompressMemoryBuffer.Position = 0;
                mDeCompressBuffer.Write(inputBytes, index, length);
                mDeCompressBuffer.finish();
                int postion = (int)mDeCompressMemoryBuffer.Position;
                mDeCompressMemoryBuffer.Position = 0;
                byte[] bytes = new byte[postion];
                mDeCompressMemoryBuffer.Read(bytes, 0, postion);
                return bytes;
            }
        }

        protected override void SendPing()
        {
            if (DEBUG_LOG)
            {
                Log.Debug("ping");
            }
            //Send(null, pingCmd, SerialNumber);
        }

        public override void Update()
        {
            base.Update();
            lock (mWaitBcQueueLock)
            {
                if (mWaitBcQueue != null && mWaitBcQueue.Count > 0)
                {
                    int count = mBcSize;
                    while (mWaitBcQueue.Count > 0 && count != 0)
                    {
                        WaitBcStruct wbcs = mWaitBcStructQueue.Dequeue();
                        byte[] bytes = mWaitBcQueue.Dequeue();
                        count--;
                        if (RECIVE_BREAK || BREAK_RECVIE_MSGID == wbcs.Cmd)
                        {
                            continue;
                        }
                            
                        try
                        {
                            DispatchEvent(EventNumberUtils.CONNECT_DATA_BROADCAST, wbcs.Cmd, wbcs.Number, bytes);
                        }
                        catch(System.Exception e)
                        {
                            Log.Error(e.Message + "\n" + e.StackTrace);
                        }
                    }
                }
            }
        }

        private void AddWaitBcQueue(int cmd, int number, byte[] bytes)
        {
            lock (mWaitBcQueueLock)
            {
                if (mWaitBcQueue == null)
                {
                    mWaitBcQueue = new Queue<byte[]>(mWaitBcSize);
                }
                if (mWaitBcStructQueue == null)
                {
                    mWaitBcStructQueue = new Queue<WaitBcStruct>(mWaitBcSize);
                }
                mWaitBcQueue.Enqueue(bytes);
                mWaitBcStructQueue.Enqueue(new WaitBcStruct(cmd, number));
            }
        }

        internal override void Close()
        {
            base.Close();
            if (mDeCompressBuffer != null)
            {
                mDeCompressBuffer.Close();
                mDeCompressBuffer = null;
            }
            if (mCompressBuffer != null)
            {
                mCompressBuffer.Close();
                mCompressBuffer = null;
            }
        }

        internal ErlTcpConnect(string address, int port) : base(address, port)
        {
        }

        private struct WaitBcStruct
        {
            private readonly int mCmd;

            private readonly int mNumber;

            public int Cmd
            {
                get { return mCmd; }
            }

            public int Number
            {
                get { return mNumber; }
            }

            public WaitBcStruct(int cmd, int number)
            {
                mCmd = cmd;
                mNumber = number;
            }
        }
    }
}
