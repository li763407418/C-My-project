
namespace PLC
{
    public class OMRONConnection
    {
        private static OMRON.Compolet.CIP.CommonCompolet nx701 = null;

        public static void ConnectPlc()
        {
            nx701 = new OMRON.Compolet.CIP.CommonCompolet();
            nx701.ConnectionType = OMRON.Compolet.CIP.ConnectionType.Class3;
            nx701.LocalPort = 2;
            nx701.PeerAddress = "192.168.0.9";//plc地址
            nx701.ReceiveTimeLimit = ((long)(2000));//超时时间
            nx701.Active = true;
        }

    }
}
