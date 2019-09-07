namespace UdpNetworking.Packet.HighLevel
{
    public class ConnectionEstablishmentPacket : HighLevelPacket
    {
        public override uint PacketId { get; } = Global.ConnectionEstablishmentPacket;

        public ConnectionEstablishmentPacket()
        {
        }

        public override byte[] Encode()
        {
            base.Encode();

            return _networkStream.GetBuffer();
        }

        public override void Decode(byte[] buf)
        {
            base.Decode(buf);
        }
    }
}