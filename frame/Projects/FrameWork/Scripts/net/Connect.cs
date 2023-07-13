using System;
using System.Net;
using System.Net.Sockets;
using CoreFrameWork;
using CoreFrameWork.BufferUtils;
using CoreFrameWork.TimeTool;

/**
 * 通讯连接类
 * */
public class Connect : IDisposable
{

    public static object locker = new object();

    /// <summary>
    /// 连接的默认超时时间3分钟
    /// </summary>
    public const int TIMEOUT = 180000;
    /// <summary>
    /// 默认的消息最大长度，400k
    /// </summary>
    public const int MAX_DATA_LENGTH = 126 * 1024;

    /// <summary>
    /// 连接的本地地址
    /// </summary>
    private string m_localAddress;
    /// <summary>
    /// 连接的本地端口
    /// </summary>
    private int m_localPort;
    /// <summary>
    /// 连接活动的标志
    /// </summary>
    volatile bool m_active;
    /// <summary>
    /// 连接的开始时间
    /// </summary>
    private long m_startTime;
    /// <summary>
    /// 连接的最近活动时间
    /// </summary>
    private long m_activeTime;
    /// <summary>
    /// 连接的ping值
    /// </summary>
    private long m_ping = -1;
    /// <summary>
    /// 连接的超时时间
    /// </summary>
    private int m_timeout = TIMEOUT;
    /// <summary>
    /// 连接发出ping的时间
    /// </summary>
    private long m_pingTime;
    /** 是否握手成功 */
    private bool m_isConnectReady = false;
    /** socket */
    //private Socket  _socket;
    /** 转发通讯类 */
    protected PortHandler m_portHandler;
    private CallBackHandle m_callback;
    protected Socket socket;
    private int len = 0;

    /// <summary>
    /// Pulse 脉冲周期  600帧一次
    /// </summary>
    private int PulseDelay = 600;
    private int m_pulsetime;

    /// <summary>
    /// 10秒钟之内未收到回复，则认为超时
    /// </summary>
    public int MaxFrameCount = 300;
    private int m_frameCount = 0;

    /// <summary>
    /// 同时访问的次数
    /// </summary>
    internal int MaxAccessCount = 10;
    internal int AccessingCount = 0;

    private bool m_isReconnect = false;


    //YKTimer timer;//连接超时的监控
    #region get set

    public string LocalAddress
    {
        get
        {
            return m_localAddress;
        }
        set
        {
            m_localAddress = value;

        }
    }

    public int LocalPort
    {
        get
        {
            return m_localPort;
        }
        set
        {
            m_localPort = value;
        }

    }
    /// <summary>
    /// IP Port
    /// </summary>
    public string ID
    {
        get
        {
            if (string.IsNullOrEmpty(m_id))
            {
                m_id = LocalAddress + ":" + LocalPort;
            }
            return m_id;
        }
    }
    private string m_id;

    public bool Active
    {
        get
        {
            return m_active;
        }
        set
        {
            m_active = value;
        }
    }
    protected bool IsConnectReady
    {
        get
        {
            return m_isConnectReady;
        }
        set
        {
            m_isConnectReady = value;
        }
    }
    public long StartTime
    {
        get
        {
            return m_startTime;
        }
    }

    public long ActiveTime
    {
        get
        {
            return m_activeTime;
        }
        set
        {
            this.m_activeTime = value;
        }
    }

    public long ping
    {
        get
        {
            return m_ping;
        }
        set
        {
            this.m_ping = value;
        }
    }

    public int TimeOut
    {
        get
        {
            return m_timeout;
        }
        set
        {
            this.m_timeout = value;
        }
    }

    public long PingTime
    {
        get
        {
            return m_pingTime;
        }
        set
        {
            this.m_pingTime = value;
        }
    }

    public PortHandler portHandler
    {
        get
        {
            return m_portHandler;
        }
        set
        {
            this.m_portHandler = value;
        }
    }

    public CallBackHandle CallBack
    {
        get
        {
            return m_callback;
        }
        set
        {
            this.m_callback = value;
        }
    }

    public bool CanAccess
    {
        get
        {
            return AccessingCount < MaxAccessCount;

        }
    }
    #endregion

    public Connect()
    {

    }

    public void SetMaxAccessCount(int count = 10)
    {
        MaxAccessCount = count;
    }

    /// <summary>
    /// 通过地址判断是否是相同的连接
    /// </summary>
    /// <param name="localAddress"></param>
    /// <param name="localPort"></param>
    /// <returns></returns>
    public bool IsSameConnect(string localAddress, int localPort)
    {
        return LocalAddress == localAddress && LocalPort == localPort;
    }

    protected void Init(string address, int port)
    {
        LocalAddress = address;
        LocalPort = port;
    }

    public void Open(string address, int port, float delay = 3f)
    {
        lock (locker)
        {
            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectStart, this);
            Init(address, port);
            if (Active)
                throw new Exception(this.GetType() + ", open, connect is active");
            TimerManager.AddTimer(delay, ConnectFailed);
            try
            {
                //Domain and IOS ipv6 support
                IPAddress[] ips = Dns.GetHostAddresses(address);
                IPEndPoint ipe = new IPEndPoint(ips[0], port);
                socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //socket.Listen(10);
                socket.BeginConnect(ipe, ConnectSuceessed, socket);
            }
            catch (SocketException e)
            {
                TimerManager.RemoveTimer(ConnectFailed);
                ConnectFailed();
                Log.Error("连接失败: " + m_localAddress + ":" + m_localPort + ":" + e.Message);
            }

        }
    }


    public void ReConnect()
    {
        lock (locker)
        {
            Active = false;
            AccessingCount = 0;
            IsConnectReady = false;
            m_frameCount = 0;

            CleanSocket();

            m_isReconnect = true;
            Open(LocalAddress, LocalPort);

        }
    }

    /// <summary>
    /// 连接失败
    /// </summary>
    void ConnectFailed()
    {
        lock (locker)
        {
            if (socket != null)
                socket.Close();
            TimerManager.RemoveTimer(ConnectFailed);
            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectFailed, this);
            Log.Error("连接失败: " + m_localAddress + ":" + m_localPort);
        }
    }

    /// <summary>
    /// 连接成功
    /// </summary>
    /// <param name="asyncresult"></param>
    void ConnectSuceessed(IAsyncResult asyncresult)
    {
        lock (locker)
        {
            if (socket.Connected)
            {
                Active = true;
                socket.ReceiveBufferSize = 20480;
                ActiveTime = m_startTime = DateTime.Now.ToFileTime();

                TimerManager.RemoveTimer(ConnectFailed);
                ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectSuccessPre, this);

                if (m_isReconnect)
                {
                    m_isReconnect = false;
                    ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.SocketReconnected, this);

                }
                Log.Error("连接成功: " + m_localAddress + ":" + m_localPort);
            }
        }
    }

    public void Update()
    {
        if (AccessingCount > 0)
        {
            m_frameCount++;
            if (m_frameCount % MaxFrameCount == 0)
            {
                m_frameCount = 0;
                TimeOutHandle();
            }
        }

        m_pulsetime++;
        if (m_pulsetime % PulseDelay == 0)
        {
            m_pulsetime = 0;
            Pulse();
        }

    }

    public void SetPulseDelay(int delay = 600)
    {
        delay = delay < 1 ? 1 : delay;
        PulseDelay = delay;

    }
    public void ResetTimer()
    {

        m_frameCount = 0;

    }

    /// <summary>
    /// 脉冲
    /// </summary>
    public virtual void Pulse()
    {
        Ping();
    }

    /// <summary>
    /// Ping 验证连接状态
    /// </summary>
    public virtual void Ping()
    {
        ByteBuffer data = new ByteBuffer();
        data.writeShort(1);
        data.writeByte(1);
        Send(data);
    }


    /// <summary>
    /// 确定下当前连接状态
    /// </summary>
    /// <returns></returns>
    public bool CheckConnectActive()
    {
        return socket != null && socket.Connected;

        lock (locker)
        {
            if (null == socket)
            {
                return false;
            }

            bool blockingState = socket.Blocking;
            try
            {
                byte[] tmp = new byte[1];
                socket.Blocking = false;
                socket.Send(tmp, 0, 0);
                return true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                socket.Blocking = blockingState;
            }
        }
    }

    #region Send Receive


    /// <summary>
    /// 根据头信息创建字节缓存对象
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    protected virtual ByteBuffer CreateDataByHead(ByteBuffer body)
    {
        int len = body.length();
        ByteBuffer data = new ByteBuffer();
        ByteKit.writeLength(data, len);
        data.writeBytes(body.toArray());
        return data;
    }

    /// <summary>
    /// 发送字节数组
    /// </summary>
    /// <param name="data"></param>
    public virtual void Send(ByteBuffer data)
    {
        data = CreateDataByHead(data);
        Send(data.toArray(), 0, data.length());
    }

    private void Send(byte[] data, int offset, int len)
    {
        if (len > MAX_DATA_LENGTH)
        {
            throw new Exception(this.GetType() + ", send, data overflow:" + len + ", " + this);
        }

        try
        {
            socket.Send(data, 0, data.Length, SocketFlags.None);
        }
        catch (SocketException)
        {
            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectedDis, this);
        }
        catch (Exception e)
        {
            Log.Error(this.GetType() + ", send error, " + this, e.ToString());
        }

    }

    /// <summary>
    /// 连接的消息接收方法
    /// </summary>
    public virtual void Receive()
    {
        if (Active)
        {
            if (socket.Available > 0)
            {
                if (len <= 0)
                {
                    len = ReadLength();
                }
                if (len > socket.Available)//等待一个完整数据接受完毕才解析
                {
                    return;
                }
                ByteBuffer data = new ByteBuffer(len);
                data.setTop(len);
                socket.Receive(data.getArray(), SocketFlags.None);
                len = 0;
                Receive(data);
            }
        }
        else
        {

        }
    }

    public virtual void Receive(ByteBuffer data)
    {
        lock (locker)
        {
            if (GameNetBase.isDropRecv)
            {
                return;
            }

            try
            {


            }
            catch (Exception e)
            {
                Log.Error(this.GetType() + ", receive error, " + this, e.ToString());
            }

            if (null == portHandler)
            {
                return;
            }
            ActiveTime = TimeKit.CurrentTimeMillis(); //链接活动时间不用做时间修正  GetNowUnixTimeToMilliSecond

            if (null != portHandler)
            {
                portHandler.receive(this, data);
            }
        }
    }

    #endregion
    public int ReadLength()
    {
        byte[] b1 = new byte[1];
        socket.Receive(b1, SocketFlags.None);
        int n = b1[0];
        if (n >= 0x80)//len<128
        {
            return n - 0x80;
        }
        else if (n >= 0x40)
        {
            b1 = new byte[1];
            socket.Receive(b1, SocketFlags.None);
            return (n << 8) + ByteKit.readUnsignedByte(b1, 0) - 0x4000;
        }
        else if (n >= 0x20)
        {
            b1 = new byte[3];
            return (n << 24) + (ByteKit.readUnsignedByte(b1, 0) << 16)
                    + ByteKit.readUnsignedByte(b1, 1) - 0x20000000;
        }
        else
        {
            throw new Exception(this.GetType() + ", readLength, invalid number:" + n + ", " + this);
        }
    }


    /// <summary>
    /// 访问超时处理
    /// </summary>
    protected void TimeOutHandle()
    {
        lock (locker)
        {

            Active = false;
            //AccessingCount = 0;
            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ConnectedDis, this);


        }
    }


    private void CleanSocket()
    {
        lock (locker)
        {

            bool _connectActive = CheckConnectActive();
            //if (socket != null && socket.Connected)
            if (socket != null)
            {
                if (_connectActive)
                {
                    //关之前把数据读完,避免出现本地和服务器 "强迫关闭了一个连接" 的异常
                    byte[] bytes = new byte[32];
                    while (socket.Available > 0)
                    {
                        socket.Receive(bytes, SocketFlags.None);
                    }

                    try
                    {
                        socket.Disconnect(true);
                        socket.Close();
                    }
                    catch (SocketException e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }
    }

    public virtual void Dispose()
    {
        Active = false;
        AccessingCount = 0;
        m_callback = null;
        portHandler = null;
        CleanSocket();

    }
}

