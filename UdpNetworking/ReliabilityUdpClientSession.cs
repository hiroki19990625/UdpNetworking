using System.Net;
using UdpNetworking.Packet.ConnectedPacket;
using UdpNetworking.Packet.HighLevel;
using UdpNetworking.Packet.LowLevel;

namespace UdpNetworking
{
    public class ReliabilityUdpClientSession
    {
        private ReliabilityUdpClient _client;

        public IPEndPoint EndPoint { get; }
        public ushort MtuSize { get; }

        public uint SendSequenceId { get; private set; }
        public uint SendMessageId { get; private set; }
        public ushort SendSplitId { get; private set; }

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

        public void SendPacket(HighLevelPacket packet)
        {
            byte[] buf = packet.Encode();
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket(Reliability.Reliable, SendMessageId++, buf);
            EncapsulatedPacket[] packets = encapsulatedPacket.GetSplitEncapsulatedPackets(SendSplitId++, MtuSize);

            for (int i = 0; i < packets.Length; i++)
                SendDataPacket(packets[i].Encode());
        }

        public void SendDataPacket(byte[] buf)
        {
            DataPacket dataPacket = new DataPacket(SendSequenceId++, buf);
            _client.Send(EndPoint, dataPacket);
        }
    }
}