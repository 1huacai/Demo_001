using System;
using System.Collections.Generic;
using CoreFrameWork;
using CoreFrameWork.BufferUtils;
using CoreFrameWork.Event;
/** 
* 网络连接管理器 
* */
public class ConnectManager
{
    /** 连接管理器的唯一实例 */
    private static ConnectManager _manager = null;
    public static ConnectManager manager()
    {
        if (_manager == null)
        {
            _manager = new ConnectManager();

        }
        return _manager;
    }


    public ErlConnectFactory factory;
    public ReceiveFun messageHandle;
    //内部消息
    public Event NetEvent;

    public float RealtimeSinceStartup;
    public long RunFrameCount = 0;

    public Dictionary<string, List<ConnectData>> AccessDic
    {
        get
        {
            return m_accessDic;
        }
    }
    private Dictionary<string, List<ConnectData>> m_accessDic = new Dictionary<string, List<ConnectData>>();

    private Dictionary<string, List<ErlEntry>> m_reaccessDic = new Dictionary<string, List<ErlEntry>>();
    /// <summary>
    /// 记录断线重连时未发送完的请求
    /// </summary>
    private Dictionary<string, List<ConnectData>> m_reaccessConnectDataDic = new Dictionary<string, List<ConnectData>>();

    private List<string> m_reaccessIgnoreList = new List<string>();
    /// <summary>
    /// 断线时最多缓存继续访问个数
    /// </summary>
    public const int AccessCountMaxWhenDisconnect = 50;
    public int CurrentAccessCountWhenDisconnect = 0;

    public ConnectManager()
    {
        NetEvent = new Event();
        if (factory == null)
        {
            factory = new ErlConnectFactory();
        }
        factory.StartTime();

        NetEvent.AddEvent(eYKNetEvent.ConnectedDis, OnConnectedDis);
        NetEvent.AddEvent(eYKNetEvent.SocketReconnected, OnSocketReconnected);
    }

    ~ConnectManager()
    {

        NetEvent.RemoveEvent(eYKNetEvent.ConnectedDis, OnConnectedDis);
        NetEvent.RemoveEvent(eYKNetEvent.SocketReconnected, OnSocketReconnected);
    }

    public void init()
    {

    }
    /// <summary>
    /// 获取连接的IP PORT
    /// </summary>
    /// <param name="connect"></param>
    /// <returns></returns>
    public string GetKey(Connect connect)
    {
        if (null == connect)
        {
            return string.Empty;
        }
        return connect.ID;
    }
    /// <summary>
    /// 创建链接
    /// </summary>
    public Connect CreateConnect(string localAddress, int localPort, CallBackHandle callBack)
    {

        Log.Error("创建连接: " + localAddress + ":" + localPort);
        Connect _connect = factory.CheckInstance(localAddress, localPort);
        if (null != _connect)
        {
            Log.Error(string.Format("已存在连接:=> Address:{0}, Poar:{1}", localAddress, localPort));
        }
        else
        {
            _connect = factory.OpenConnect(localAddress, localPort, callBack);
            string _key = GetKey(_connect);
            if (!m_accessDic.ContainsKey(_key))
            {
                m_accessDic.Add(_key, new List<global::ConnectData>());
            }

        }

        return _connect;
    }

    public ErlEntry GetErlEntry(Connect connect, int port)
    {
        List<ErlEntry> _list = null;
        if (m_reaccessDic.TryGetValue(GetKey(connect), out _list))
        {
            ErlEntry entry;
            for (int i = 0; i < _list.Count; i++)
            {
                entry = _list[i] as ErlEntry;
                if (entry.number == port)
                {
                    _list.Remove(entry);
                    return entry;
                }
            }

        }
        return null;
    }


    public List<ErlEntry> GetErlEntryList(Connect connect)
    {
        List<ErlEntry> _list = null;
        if (m_reaccessDic.TryGetValue(GetKey(connect), out _list))
        {
            return _list;
        }
        return new List<ErlEntry>();
    }


    public void Update(float time)
    {
        RealtimeSinceStartup = time;
        if (factory != null)
        {
            factory.Update();
        }

        DoSendMessage();

        if (RunFrameCount > long.MaxValue - 1)
        {
            RunFrameCount = 0;
        }
        RunFrameCount++;

    }




    public bool CheckConnectActive(Connect connect)
    {
        if (null != connect)
        {
            return connect.CheckConnectActive();
        }

        return false;
    }

    public bool CheckConnectActive(string address, int port)
    {
        Connect connect = factory.GetConnect(address, port, null);
        return CheckConnectActive(connect);
    }

    public void SendMessage(Connect connect, ErlKVMessage message, ReceiveFun handle, List<object> argus, NetDef.SendType type)
    {
        SendMessage(connect.LocalAddress, connect.LocalPort, message, handle, argus, type);
    }

    /** 
	 * 向指定地址和端口的连接发送消息
	 * @param address-消息发送地址
	 * @param port-消息发送端口
	 * @param handle-执行回调的函数
	 * @param argus-执行回调的参数数组
	 * @param _data-消息发送数据 ErlKVMessage对象
	 **/
    public void SendMessage(string address, int port, ErlKVMessage message, ReceiveFun handle, List<object> argus, NetDef.SendType type, bool ignoreLimit = false)
    {

        Connect _connect = factory.GetConnect(address, port, null);
        if (null != _connect)
        {
            string _key = GetKey(_connect);
            List<ConnectData> _list = null;
            if (m_accessDic.TryGetValue(_key, out _list))
            {
                ConnectData _data = new ConnectData()
                {
                    address = address,
                    port = port,
                    message = message,
                    receiveFun = handle,
                    argus = argus,
                    type = type,
                    ignoreLimit = ignoreLimit,
                };

                _list.Add(_data);

            }
        }
        else
        {
            Log.Error(string.Format("不存在连接:=> Address:{0}, Poar:{1}", address, port));
        }

    }

    /// <summary>
    /// 从缓存链表里取出访问，开始访问服务器
    /// </summary>
    public void DoSendMessage()
    {

        List<ConnectData> _list = null;
        foreach (KeyValuePair<string, List<ConnectData>> pair in m_accessDic)
        {
            _list = pair.Value;

            if (0 == _list.Count)
            {
                continue;
            }

            ConnectData _data = null;
            ErlConnect _connect = null;
            for (int j = 0; j < _list.Count; ++j)
            {
                _data = _list[j];
                _connect = factory.GetConnect(_data.address, _data.port, null) as ErlConnect;
                if (_data.ignoreLimit)
                {
                    _list.RemoveAt(j--);
                    access(_connect, _data);
                }
                else
                {
                    if (!_connect.CanAccess)
                    {
                        //Log.Error("当前连接访问量为:" + connect.AccessingCount);
                        continue;
                    }
                    _list.RemoveAt(j--);
                    access(_connect, _data);
                }
            }
        }
    }

    private void access(ErlConnect connect, ConnectData data)
    {
        lock (Connect.locker)
        {
            if (data.type != NetDef.SendType.SendLogin)
            {
                if (GameNetBase.isReCounecting || !GameNetBase.isUidOk ||
                    (!GameNetBase.isCounectEd && CurrentAccessCountWhenDisconnect > AccessCountMaxWhenDisconnect))
                {
                    return;
                }
                else
                {
                    connect.AccessingCount++;
                    connect.ResetTimer();
                }
            }

            ByteBuffer _buffer = null;
            ErlEntry _erlEntry = null;
            if (null == data.erlEntry)
            {
                _buffer = new ByteBuffer();
                data.message.bytesWrite(_buffer);

                _erlEntry = new ErlEntry(connect, data, _buffer);
                data.erlEntry = _erlEntry;
            }
            else
            {
                //断线重连的情况
                _buffer = data.erlEntry.buffer;
                _erlEntry = data.erlEntry;
            }
            _erlEntry.sendFrame = RunFrameCount;
            //Log.Error("data.message.getPort(): " + data.message.getPort());

            string _key = GetKey(connect);
            if (!m_reaccessDic.ContainsKey(_key))
            {
                m_reaccessDic.Add(_key, new List<global::ErlEntry>());
            }

            //登录接口不应该大于1
            List<ErlEntry> _list = GetErlEntryList(connect as Connect);
            if (_list.Exists(v => data.type == NetDef.SendType.SendLogin) && data.type == NetDef.SendType.SendLogin)
            {
                //移除旧的登录接口
                for (int i = 0; i < _list.Count; ++i)
                {
                    if (_list[i].type == NetDef.SendType.SendLogin)
                    {
                        _list.RemoveAt(i--);
                    }
                }
            }


            if (!m_reaccessDic[_key].Contains(_erlEntry))
            {
                m_reaccessDic[_key].Add(_erlEntry);
            }




            NetEvent.DispatchEvent(eYKNetEvent.AccessMsg, data.message);
            if (GameNetBase.isDropSend)
            {
                return;
            }
            connect.SendErl(_buffer, ErlConnect.ENCRYPTION, ErlConnect.CRC, ErlConnect.COMPRESS, ErlConnect.KV);

        }
    }


    public void reaccess(ErlConnect c, bool dispatchEvent = true)
    {
        string _key = GetKey(c as Connect);
        List<ErlEntry> _list = m_reaccessDic[_key];
        ErlEntry entry;
        ErlEntry[] array = _list.ToArray();

        int _realCount = 0;
        for (int i = 0; i < array.Length; i++)
        {
            entry = array[i] as ErlEntry;
            _list.Remove(entry);
            if (entry.type == NetDef.SendType.SendNoReSend)
            {
                continue;
            }
            _realCount++;
            //_list.Remove(entry);
            access(c, entry.connectData);
            Log.Error("重发消息： " + entry.connectData.message.Cmd + ", 流水号：" + entry.number);
        }

        if (_realCount > c.MaxAccessCount)
        {
            Log.Error("异常：重连个数 " + array.Length);
        }

        if (m_reaccessConnectDataDic.ContainsKey(_key))
        {
            while (m_reaccessConnectDataDic[_key].Count > 0)
            {
                ConnectData _data = m_reaccessConnectDataDic[_key][0];
                SendMessage(_data.address, _data.port, _data.message, _data.receiveFun, _data.argus, _data.type);
                m_reaccessConnectDataDic[_key].Remove(_data);
            }
        }
        m_reaccessConnectDataDic.Remove(_key);
        CurrentAccessCountWhenDisconnect = 0;
        if (dispatchEvent)
        {
            NetEvent.DispatchEvent(eYKNetEvent.Reconnect, c);
        }
    }
    /// <summary>
    /// 断线重连时不重发的消息
    /// </summary>
    /// <param name="cmd"></param>
    public void AddReaccessIgnoreCMD(string cmd)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            return;
        }
        if (!m_reaccessIgnoreList.Contains(cmd))
        {
            m_reaccessIgnoreList.Add(cmd);
        }
    }

    private bool ContainErlKVMessage(Queue<ConnectData> queque, ErlKVMessage message)
    {
        if (null == queque || 0 == queque.Count)
        {
            return false;

        }
        ConnectData[] _array = queque.ToArray();
        for (int i = 0; i < _array.Length; ++i)
        {
            if (_array[i].message.Equals(message))
            {
                return true;
            }
        }

        return false;
    }

    public void CloseConnect(Connect connect)
    {
        string _key = GetKey(connect);
        List<ConnectData> _list = null;

        if (!m_reaccessConnectDataDic.TryGetValue(_key, out _list))
        {
            _list = new List<ConnectData>();
            m_reaccessConnectDataDic.Add(_key, _list);
        }

        if (m_accessDic.ContainsKey(_key))
        {
            while (m_accessDic[_key].Count > 0)
            {
                ConnectData _data = m_accessDic[_key][0];
                m_accessDic[_key].Remove(_data);
                if (!m_reaccessConnectDataDic[_key].Contains(_data))
                {
                    if (!m_reaccessIgnoreList.Exists(v => v == _data.message.Cmd))
                    {
                        m_reaccessConnectDataDic[_key].Add(_data);
                    }
                }
            }
        }
        m_accessDic.Remove(_key);

        factory.CloseConnect(connect);
    }

    /** 关闭并清空所有连接 */
    public void closeAllConnects()
    {
        GameNetBase.isUidOk = false;

        while (factory.connectArray.Count > 0)
        {
            CloseConnect(factory.connectArray[0]);
        }

        m_accessDic.Clear();
        factory.CloseAllConnects();
        CurrentAccessCountWhenDisconnect = 0;
    }


    public void ClearReaccess(Connect connect)
    {
        if (null == connect || null == m_reaccessDic)
        {
            return;
        }
        List<ErlEntry> _list = null;
        if (m_reaccessDic.TryGetValue(GetKey(connect), out _list))
        {
            _list.Clear();
        }
    }

    public void ClearAllReaccess()
    {
        m_reaccessDic.Clear();
    }

    public void ClearConnectData(Connect connect)
    {
        if (null == connect || null == m_reaccessConnectDataDic)
        {
            return;
        }
        List<ConnectData> _list = null;
        if (m_reaccessConnectDataDic.TryGetValue(GetKey(connect), out _list))
        {
            _list.Clear();
        }
    }


    public void ClearAllConnectDataDic()
    {
        m_reaccessConnectDataDic.Clear();
    }




    #region 事件



    private void OnConnectedDis(params object[] args)
    {
        //Connect _connect = args[0] as Connect;

        //  _connect.ReConnect();

    }

    /// <summary>
    /// 由socket子线程抛出
    /// </summary>
    /// <param name="args"></param>
    private void OnSocketReconnected(params object[] args)
    {
        //Connect _connect = args[0] as Connect;


    }

    #endregion


    public void SetPulseDelay(Connect connect, int delay = 600)
    {
        if (null == connect)
        {
            return;
        }
        connect.SetPulseDelay(delay);
    }


    public void Dispose()
    {
        closeAllConnects();
        m_accessDic.Clear();
        m_reaccessConnectDataDic.Clear();
        m_reaccessDic.Clear();
        m_reaccessIgnoreList.Clear();
        _manager = null;
    }


    [Obsolete("无用的接口")]
    public void ClearAccessList()
    {

    }
    [Obsolete("无用的接口")]
    public int MaxDataAccessCount;



}
