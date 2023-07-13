using System;
using CoreFrameWork;
using FrameWork;

public class ErlConnectFactory : ConnectFactory
{
    /// <summary>
    /// 打开指定地址的连接
    /// </summary>
    /// <param name="localAddress"></param>
    /// <param name="localPort"></param>
    /// <returns></returns>
    public override Connect OpenConnect(string localAddress, int localPort, CallBackHandle callBack = null)
    {
        ErlConnect c = new ErlConnect();
        c.Open(localAddress, localPort);
        c.CallBack = callBack;
        connectArray.Add(c);

        c.portHandler = DataAccess.getInstance();
        GameNetBase.SetIp_Port(localAddress, localPort);
        return c;
    }

    public override void Update()
    {
        base.Update();
    }

    protected override void Run()
    {
        try
        {
            ErlConnect c;

            for (int i = connectArray.Count - 1; i >= 0; i--)
            {
                c = connectArray[i] as ErlConnect;
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

}

