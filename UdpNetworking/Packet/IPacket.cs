namespace UdpNetworking.Packet
{
    public interface IPacket
    {
        byte[] Encode();
        void Decode(byte[] buf);
    }
}