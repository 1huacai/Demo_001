
using CoreFrameWork;
using CoreFrameWork.BufferUtils;

public class PingPort:PortHandler
{
	override public void receive(Connect connect, ByteBuffer data)
	{
		long time=(long)data.readDouble();
		connect.ping=time-connect.PingTime;
		connect.PingTime=0;
	}
}

