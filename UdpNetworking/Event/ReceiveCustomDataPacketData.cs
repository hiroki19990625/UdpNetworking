using UdpNetworking.Packet.HighLevel;

namespace UdpNetworking.Event
{
    public class ReceiveCustomDataPacketData
    {
        public CustomDataPacket CustomDataPacket { get; }

        public ReceiveCustomDataPacketData(CustomDataPacket customDataPacket)
        {
            CustomDataPacket = customDataPacket;
        }
    }
}