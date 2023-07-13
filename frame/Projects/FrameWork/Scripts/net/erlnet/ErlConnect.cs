using System;
using System.Net.Sockets;
using CoreFrameWork;
using CoreFrameWork.BufferUtils;
using CoreFrameWork.Misc;
using CoreFrameWork.TimeTool;
using FrameWork;
/**
 * ErlConnect
 * */
public class ErlConnect : Connect
{

    /** 版本号 */
    public const int VERSION = 0;
    /** 默认加密效验 */
    public const int ENCRYPTION = 1;
    /** 默认CRC效验 */
    public const int CRC = 1;
    /** 默认压缩 */
    public const int COMPRESS = 1;
    /** 默认kv类型 为0表示消息为二进制数据，为1表示消息为KeyValue类型，key为字符串，Value为标准格式的数据 */
    public const int KV = 1;
    /***/
    private const int RAND_MASK = 123459876;
    /***/
    private const int RAND_A = 16807;
    private const int RAND_Q = 127773;

    /** 发送挑战码 */
    private int[] _sendChallengeCode;
    /** 接受挑战码 */
    private int[] _receiveChallengeCode;

    /** 是否 加密效验*/
    private int _encryption = ENCRYPTION;
    /** 是否 CRC效验*/
    private int _crc = CRC;
    /** 是否压缩 */
    private int _compress = COMPRESS;
    /** kv类型 */
    private int _kv = KV;
    /** 消息接收到的长度 */
    private int length = 0;

    public ErlConnect()
    {
    }


    public override void Pulse()
    {
        ErlKVMessage message = new ErlKVMessage("echo");
        ConnectManager.manager().SendMessage(LocalAddress, LocalPort, message, pingHandle, null, NetDef.SendType.SendNoReSend, true);



    }


    /// <summary>
    /// 根据头信息创建字节缓存对象
    /// </summary>
    /// <param name="head"></param>
    /// <returns></returns>
    protected override ByteBuffer CreateDataByHead(ByteBuffer head)
    {
        ByteBuffer data = new ByteBuffer();
        int versionInfo = (VERSION << 4) | (_encryption << 3) | (_crc << 2) | (_compress << 1) | _kv;
        data.writeShort(head.length() + 1);
        data.writeByte(versionInfo);
        data.writeBytes(head.toArray());
        return data;
    }

    /** 发送方法 
         * @param _data 数据
         * @param isEncryption 是否加密
         * @param isCrc 是否crc
         * @param isCompress 是否压缩
         * @param kv kv类型 为0表示消息为二进制数据，为1表示消息为KeyValue类型，key为字符串，Value为标准格式的数据
         * */
    public void SendErl(ByteBuffer data, int encryption, int crc, int compress, int kv)
    {
        //没有得到pk码,一般出现在连接有,但是接不到后台消息
        if (_sendChallengeCode == null || _sendChallengeCode.Length < 0)
            return;

        _encryption = encryption;
        _crc = crc;
        _compress = compress;
        _kv = kv;
        int crcValue = 0;
        ByteBuffer data1 = new ByteBuffer();

        if (_compress == COMPRESS && data.length() >= 64)
        {
            // 根据参数和数据长度判断是否执行压缩  
            byte[] bb = ZIPUtil.Compress(data.toArray());
            data = new ByteBuffer(bb);
        }
        else
        {
            _compress = 0;
        }

        if (_crc == 1 && _compress == 0)
        {
            crcValue = (int)ChecksumUtil.Adler32(data);
            data1.writeInt(crcValue);
        }
        else
        {
            _crc = 0;
        }
        data1.writeBytes(data.toArray());

        if (_encryption == 1)
        {
            data1 = EncryptionCode(data1, _sendChallengeCode);// 执行加密
        }


        Send(data1);

        _encryption = ENCRYPTION;
        _crc = CRC;
        _compress = COMPRESS;
        _kv = KV;

    }

    /** 连接的消息接收方法 */
    public override void Receive()
    {

        if (Active)
        {
            if (socket.Available > 0)
            {
                if (!IsConnectReady)
                {
                    //设置 _isConnectReady=true  connect pk receive 
                    //抛掉前两位
                    byte[] b1 = new byte[1];
                    socket.Receive(b1, SocketFlags.None);
                    byte[] b2 = new byte[1];
                    socket.Receive(b2, SocketFlags.None);

                    byte[] b3 = new byte[4];
                    socket.Receive(b3, SocketFlags.None);
                    Array.Reverse(b3);
                    int i = BitConverter.ToInt32(b3, 0);

                    byte[] b4 = new byte[4];
                    socket.Receive(b4, SocketFlags.None);
                    Array.Reverse(b4);
                    int ii = BitConverter.ToInt32(b4, 0);

                    _sendChallengeCode = GetPK(i);
                    _receiveChallengeCode = GetPK(ii);
                    IsConnectReady = true;
                    if (this.CallBack != null)
                    {
                        this.CallBack();
                    }

                    ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectSuccessComplete, this);
                }
                else
                {
                    if (GameNetBase.isDropRecv)
                    {
                        return;
                    }

                    if (length <= 0)
                    {
                        if (socket.Available < 2)
                        {
                            return;
                        }
                        //	length = _data.readUnsignedShort (); 
                        byte[] b = new byte[2];
                        socket.Receive(b, SocketFlags.None);
                        length = ByteKit.readUnsignedShort(b, 0);
                        //length = readLength ();
                    }

                    for (int i = 0; length > 0 && socket.Available >= length && i < 20; i++)
                    {

                        ByteBuffer data = new ByteBuffer(length);
                        data.setTop(length);
                        socket.Receive(data.getArray(), SocketFlags.None);
                        Receive(data);
                        ParseMessage(data);

                    }
                }
            }
        }
    }

    /// <summary>
    /// 解析单次消息内容
    /// </summary>
    /// <param name="socketbuffer"></param>
    public void ParseMessage(ByteBuffer socketbuffer)
    {

        int versionInfo = socketbuffer.readByte();
        bool encryption = ((versionInfo & 8) != 0);
        bool crc = ((versionInfo & 4) != 0);
        bool compress = ((versionInfo & 2) != 0);

        ByteBuffer data = new ByteBuffer(length - 1);
        data.write(socketbuffer.toArray(), 0, length - 1);

        try
        {
            //为下次数据处理做判断
            if (socket.Available >= 2)
            {
                byte[] b = new byte[2];
                socket.Receive(b, SocketFlags.None);
                length = ByteKit.readUnsignedShort(b, 0);
            }
            else
                length = 0;

            if (encryption)
            {
                data = EncryptionCode(data, _receiveChallengeCode);
            }
            if (compress)
            {
                byte[] bb = ZIPUtil.Decompress(data.toArray());
                data = new ByteBuffer(bb);
            }
        }
        catch (Exception e)
        {
            //抛出断线重连事件
            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectedDis, this);
            return;
        }

        if (crc)
        {
            int crcValue = data.readInt();
            ByteBuffer data1 = new ByteBuffer();
            data1.writeBytes(data.toArray(), 0, (data.top - data.position));
            int nowCrc = (int)ChecksumUtil.Adler32(data1);
            if (crcValue != nowCrc)
            {
                //抛出断线重连事件
                ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectedDis, this);

                Log.Error("crc is err,crcValue" + crcValue + ",nowCrc=" + nowCrc);
                return;
            }
        }

        ErlKVMessage message = new ErlKVMessage(null);
        try
        {
            message.bytesRead(data);
        }
        catch (Exception e)
        {
            //抛出断线重连事件
            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectedDis, this);
            return;
        }

        if (portHandler != null)
        {
            // _portHandler可以是DataAccess或者ErlTransmitPort，如果要保存funcUid就要设置为DataAccess
            portHandler.erlReceive(this, message);
        }
    }


    /// <summary>
    /// 发送消息时用作加密，接收消息时用作解密
    /// </summary>
    /// <param name="data"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    private ByteBuffer EncryptionCode(ByteBuffer data, int[] code)
    {
        byte[] bytes = data.toArray();
        bytes = EncodeXor(bytes, NextPK(code));
        data = new ByteBuffer();
        for (int i = 0; i < bytes.Length; i++)
        {
            data.writeByte(bytes[i]);
        }
        data.position = 0;
        return data;
    }

    /// <summary>
    /// 获取指定种子的密码
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    protected int[] GetPK(int seed)
    {
        //ByteBuffer _data = new ByteBuffer ();
        int seed1 = GetRandome(seed + 11);
        int seed2 = GetRandome(seed1 + 13);
        int seed3 = GetRandome(seed2 + 17);
        int seed4 = GetRandome(seed3 + 19);
        int seed5 = GetRandome(seed4 + 23);
        int seed6 = GetRandome(seed5 + 29);
        int seed7 = GetRandome(seed6 + 31);
        int seed8 = GetRandome(seed7 + 37);
        return new int[] { seed1, seed2, seed3, seed4, seed5, seed6, seed7, seed8 };
    }

    /// <summary>
    /// 获取指定种子的随机数
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    private int GetRandome(int seed)
    {
        int r = seed ^ RAND_MASK;
        int s = RAND_A * r - (int)Math.Round((double)(r / RAND_Q)) * int.MaxValue;
        if (s < 0)
            return s + int.MaxValue;
        else
            return s;
    }

    /// <summary>
    /// 获取下一个密码
    /// </summary>
    /// <param name="pk"></param>
    /// <returns></returns>
    protected byte[] NextPK(int[] pk)
    {
        if (pk == null)
            return null;
        for (int i = 0, length = pk.Length; i < length; i++)
        {
            pk[i] = GetRandome(pk[i]);
        }
        return ToPK(pk);
    }
    /// <summary>
    /// 获取指定密码的字节数组
    /// </summary>
    /// <param name="pks"></param>
    /// <returns></returns>
    protected byte[] ToPK(int[] pks)
    {
        ByteBuffer data = new ByteBuffer();
        data.writeInt(pks[0]);
        data.writeInt(pks[1]);
        data.writeInt(pks[2]);
        data.writeInt(pks[3]);
        data.writeInt(pks[4]);
        data.writeInt(pks[5]);
        data.writeInt(pks[6]);
        data.writeInt(pks[7]);
        return data.getArray();
    }

    /**
	 * 字节数组加密
	 * @param bytes 源数据
	 * @param keys 密钥
	 * @return byte[] 加密后的数据
	 */
    public static byte[] EncodeXor(byte[] bytes, byte[] keys)
    {
        if (bytes == null || bytes.Length < 1 || keys == null
            || keys.Length < 1)
            return null;

        int blength = bytes.Length;
        int klength = keys.Length;
        byte[] result = new byte[blength];
        int j = 0;
        for (int i = 0; i < blength; i++)
        {
            if (j == klength) j = 0;
            int k = (bytes[i] ^ keys[j]);
            k <<= 24;
            k >>= 24;
            result[i] = (byte)k;
            j++;
        }
        return result;
    }

    public string ToString()
    {
        return "[_sendChallengeCode=" + _sendChallengeCode + ",_receiveChallengeCode=" + _receiveChallengeCode + "]";
    }

    /// <summary>
    /// 执行ping通信返回消息的执行方法
    /// </summary>
    /// <param name="erlConnect"></param>
    /// <param name="erlMessage"></param>
    /// <param name="state"></param>
    protected void pingHandle(Connect erlConnect, object erlMessage, NetDef.ReceiveFunState state)
    {
        long time = TimeKit.CurrentTimeMillis(); //链接活动时间不用做时间修正
        erlConnect.ping = time - erlConnect.PingTime;

        if (state == NetDef.ReceiveFunState.Ok)
        {
            ErlKVMessage _msg = erlMessage as ErlKVMessage;
            if (null == _msg)
            {
                return;
            }
            ErlLong _ms = _msg.getValue("ms") as ErlLong; //毫秒
            long _value = _ms.getValueLong();
            long _timestamp = (long)(_value * 0.001d);
            int _millisecond = (int)(((_value * 0.001d) - _timestamp) * 1000);



            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.Ping, _timestamp, _millisecond);
        }
    }


    /// <summary>
    /// 连接关闭方法
    /// </summary>
    public void close()
    {
        Dispose();

    }
}

