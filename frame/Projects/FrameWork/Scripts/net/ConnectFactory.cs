using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CoreFrameWork;
using FrameWork;

public class ConnectFactory
{
    /// <summary>
    /// 连接链表
    /// </summary>
    public List<Connect> connectArray = new List<Connect>();


    public ConnectFactory()
    {

    }


    /// <summary>
    /// 检查是否存在到指定地址的连接 
    /// </summary>
    /// <param name="localAddress"></param>
    /// <param name="localPort"></param>
    /// <returns></returns>
    public  virtual  Connect CheckInstance(string localAddress, int localPort)
    {
        Connect _connect = null;

        for (int i = connectArray.Count - 1; i >= 0; i--)
        {
            _connect = connectArray[i];

            if (_connect.IsSameConnect(localAddress, localPort))
            {
                return _connect;
            }
        }
        return null;
    }

    /// <summary>
    /// 获得指定地址的连接，并保存该连接
    /// </summary>
    /// <param name="localAddress"></param>
    /// <param name="localPort"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public Connect GetConnect(string localAddress, int localPort, CallBackHandle callBack)
    {
        return CheckInstance(localAddress, localPort);
    }


    /// <summary>
    /// 打开指定地址的连接 
    /// </summary>
    /// <param name="localAddress"></param>
    /// <param name="localPort"></param>
    /// <returns></returns>
    public virtual Connect OpenConnect(string localAddress, int localPort, CallBackHandle callBack = null)
    {
        Connect c = new Connect();
        c.Open(localAddress, localPort);
        c.CallBack = callBack;
        connectArray.Add(c);
        return c;
    }


    protected virtual void Run()
    {
        try
        {
            Connect c;

            for (int i = connectArray.Count - 1; i >= 0; i--)
            {
                c = connectArray[i] as Connect;
                if (c.Active)
                {
                    c.Receive();
                }
                c.Update();
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        }


    }

    public void StartTime()
    {

    }

    public virtual void Update()
    {
        Run();
    }

    /// <summary>
    /// 执行ping通信返回消息的执行方法 
    /// </summary>
    /// <param name="connect"></param>
    /// <param name="erlMessage"></param>
    protected virtual void PingHandle(ErlConnect connect, ErlKVMessage erlMessage)
    {

    }


    public void CloseConnect(Connect connect)
    {
        if (null == connect)
        {
            return;
        }

        if (connectArray.Contains(connect))
        {
            connect.Dispose();
            connectArray.Remove(connect);
        }

    }

    /// <summary>
    /// 关闭并清空指定连接 
    /// </summary>
    /// <param name="localAddress"></param>
    /// <param name="localPort"></param>
    public void CloseConnect(string localAddress, int localPort)
    {
        Connect _connect = null;

        for (int i = connectArray.Count - 1; i >= 0; i--)
        {
            _connect = connectArray[i];

            if (_connect.IsSameConnect(localAddress, localPort))
            {
                _connect.Dispose();
                connectArray.Remove(_connect);
            }
        }
    }

    /// <summary>
    /// 关闭并清空所有连接  
    /// </summary>
    internal void CloseAllConnects()
    {

        Connect _connect = null;

        for (int i = connectArray.Count - 1; i >= 0; i--)
        {
            _connect = connectArray[i];
            _connect.Dispose();
        }
        connectArray.Clear();

    }

    /// <summary>
    /// 清理死连接
    /// </summary>
    public void ClearDeadConnect()
    {
        Connect _connect = null;
        for (int i = connectArray.Count - 1; i >= 0; i--)
        {
            _connect = connectArray[i];
            if (!_connect.Active)
            {
                _connect.Dispose();
                connectArray.Remove(_connect);
            }

        }
    }
}