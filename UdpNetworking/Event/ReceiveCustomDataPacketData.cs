using UdpNetworking.Packet.HighLevel;

namespace UdpNetworking.Event
{
    public class ReceiveCustomDataPacketData
    {
        public ReliabilityUdpClientSession Session { get; }
        public CustomDataPacket CustomDataPacket { get; }

        public ReceiveCustomDataPacketData(ReliabilityUdpClientSession session, CustomDataPacket customDataPacket)
        {
            Session = session;
            CustomDataPacket = customDataPacket;
        }
    }
}