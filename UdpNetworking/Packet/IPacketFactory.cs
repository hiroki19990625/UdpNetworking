namespace UdpNetworking.Packet
{
    public interface IPacketFactory
    {
        IPacket GetPacket(byte id);
    }
}