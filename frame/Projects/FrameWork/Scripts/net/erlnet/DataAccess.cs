using System;
using CoreFrameWork;
using CoreFrameWork.BufferUtils;

/**
 * 同步通讯类，需要等待返回，有超时
 * */
public class DataAccess : PortHandler
{
    /** 实例化对象 */
    private static DataAccess _dataAccess;
    /** 接收后台广播的默认处理函数 */
    public ReceiveFun defaultHandle = null;

    public DataAccess()
    {
    }

    /** 获得实例 */
    public static DataAccess getInstance()
    {
        if (_dataAccess == null)
        {
            _dataAccess = new DataAccess();
        }
        return _dataAccess;
    }



    /** 获取返回方法 */
    private ErlEntry removeReciveFun(Connect connect, int port)
    {
        return ConnectManager.manager().GetErlEntry(connect, port);
    }


    public override void receive(Connect connect, ByteBuffer data)
    {
        base.receive(connect, data);
    }

    public override void erlReceive(Connect connect, ErlKVMessage message)
    {
        string cmd = message.Cmd;
        int port = message.getPort();// 获取流水号
        ErlEntry entry = removeReciveFun(connect, port);
        if (cmd == "r_ok" || cmd == "r_err")
        {
            if (entry == null)
            {
                ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.PortZero, entry, message);
                Log.Error("未找到水流号为 " + port + " 的请求。");
                return;
            }


            NetDef.ReceiveFunState state = NetDef.ReceiveFunState.Ok;
            string error = null;
            if (cmd == "r_ok")
            {
                ErlString str = message.getValue("msg") as ErlString;
                if (str != null && str.getValueString() != "ok")
                {
                    state = NetDef.ReceiveFunState.Error;
                    error = str.getValueString();
                    if (null != entry.receiveFun)
                    {
                        Log.Debug(entry.receiveFun.Target != null ? entry.receiveFun.Target.ToString() : "", error);
                    }
                }
            }
            else if (cmd == "r_err")
            {
                state = NetDef.ReceiveFunState.SystemError;
                error = "SystemError";
                if (null != entry.receiveFun)
                {
                    Log.Debug(entry.receiveFun.Target != null ? entry.receiveFun.Target.ToString() : "", message.getLogStr());
                }
            }

            if (error != null && message.IsErrorTips)
            {
                string _cmd = null != entry ? entry.cmd : string.Empty;
                ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ReceiveMsgError, error, state, _cmd);
            }

            connect.AccessingCount--;
            if (connect.AccessingCount <= 0)
            {
                connect.AccessingCount = 0;
            }

            ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.ReceiveMsg, entry, message);
            if (null != entry.receiveFun)
            {
                entry.receiveFun(connect, message, state);
            }
            long _coutFrameCount = ConnectManager.manager().RunFrameCount - entry.sendFrame;
            float costTime = _coutFrameCount * (1 / 30f);
            if (entry != null && costTime >= 5f)
            {
                ConnectManager.manager().NetEvent.DispatchEvent(eYKNetEvent.CcommunicationCostTime, entry.cmd, costTime);
            }
        }
        else
        {// 服务器的广播消息
            message.addValue("cmd", new ErlString(cmd));// 为js服务的代码 
            if (null != defaultHandle)
            {
                defaultHandle(connect, message, NetDef.ReceiveFunState.Ok);
            }
        }
    }

    [Obsolete]
    public void reaccess(ErlConnect connect)
    {
        //ConnectManager.manager().reaccess(connect);
    }
}

