using System.Net;
using UdpNetworking.Packet.LowLevel;

namespace UdpNetworking
{
    public class ReliabilityUdpClientSession
    {
        private ReliabilityUdpClient _client;

        public IPEndPoint EndPoint { get; }
        public ushort MtuSize { get; }

        public ReliabilityUdpClientSession(IPEndPoint endPoint, ushort mtuSize, ReliabilityUdpClient client)
        {
            EndPoint = endPoint;
            MtuSize = mtuSize;
            _client = client;
        }

        public void OnReceive(DataPacket dataPacket)
        {
        }

        public void OnAck(AckPacket ackPacket)
        {
        }

        public void OnNack(NackPacket nackPacket)
        {
        }

        public void SendDataPacket(DataPacket dataPacket)
        {
            _client.Send(EndPoint, dataPacket);
        }
    }
}