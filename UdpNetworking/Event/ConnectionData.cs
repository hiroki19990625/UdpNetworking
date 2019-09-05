using System.Net;

namespace UdpNetworking.Event
{
    public class ConnectionData
    {
        public IPEndPoint EndPoint { get; }
        public ushort MtuSize { get; }

        public ConnectionData(IPEndPoint endPoint, ushort mtuSize)
        {
            EndPoint = endPoint;
            MtuSize = mtuSize;
        }
    }
}