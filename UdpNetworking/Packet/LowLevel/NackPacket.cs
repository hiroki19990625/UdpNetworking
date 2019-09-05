namespace UdpNetworking.Packet.LowLevel
{
    public class NackPacket : AckPacket
    {
        public override byte PacketId { get; } = Global.NackPacket;

        public NackPacket()
        {
        }

        public NackPacket(uint[] seqIds) : base(seqIds)
        {
        }
    }
}