using System;
using System.Collections.Generic;
using CoreFrameWork;
using CoreFrameWork.BufferUtils;

public class ErlEntry
{
    public Connect connect;
    /// <summary>
    /// </summary>
    public int number;
    /// <summary>
    /// ∞¸¿‡–Õ
    /// </summary>
    public NetDef.SendType type;
    /// <summary>
    /// </summary>
    public ReceiveFun receiveFun;

    public List<Object> argus;
    public ByteBuffer buffer;

    public ConnectData connectData;

    public long sendFrame;
    public string cmd
    {
        get
        {
            return null == connectData ? string.Empty : connectData.message.Cmd;
        }
    }

    public ErlEntry(Connect connect, ConnectData connectData, ByteBuffer buffer)
    {
        this.connect = connect;
        this.connectData = connectData;
        this.number = connectData.message.getPort();
        this.receiveFun = connectData.receiveFun;
        this.argus = connectData.argus;
        this.type = connectData.type;
        this.buffer = buffer;
    }

    public ErlEntry(Connect connect, int number, ReceiveFun receiveFun, List<Object> argus,  NetDef.SendType type, ByteBuffer buffer)
    {
        this.connect = connect;
        this.number = number;
        this.receiveFun = receiveFun;
        this.argus = argus;
        this.type = type;
        this.buffer = buffer;
    }
}

